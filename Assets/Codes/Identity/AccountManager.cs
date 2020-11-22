﻿using BanGround.Web;
using BanGround.Web.Auth;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BanGround.Identity
{
    public class AccountManager : MonoBehaviour, IAccountManager
    {
        [Inject]
        private IKiraWebRequest web;
        [Inject]
        private IMessageBannerController messageBannerController;
        [Inject]
        private ILoadingBlocker loadingBlocker;

        public InputField UsernameField;
        public InputField PasswordField;

        public bool isTokenSaved => !string.IsNullOrEmpty(web.AccessToken) && !string.IsNullOrEmpty(web.RefreshToken);
        public bool isAuthing { get; private set; } = false;
        public bool isOfflineMode => mActiveUser == null;
        public int LoginAttemptCount { get; private set; } = 0;

        private UserLite mActiveUser;
        public static readonly UserLite OfflineUser = new UserLite
        {
            Nickname = "Guest",
            Username = "Guest"
        };
        public UserLite ActiveUser => mActiveUser ?? OfflineUser;

        /// <summary>
        /// Opens the login panel and wait for user to cancel or successfully login.
        /// </summary>
        public async UniTask<bool> DoLogin()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                LoginAttemptCount++;
            }
            await UniTask.WaitUntil(() => !gameObject.activeSelf);
            return !isOfflineMode;
        }

        public void HideLoginPanel()
        {
            gameObject.SetActive(false);
        }

        public async void SubmitLogin()
        {
            if (isAuthing)
                return;

            isAuthing = true;
            try
            {
                loadingBlocker.Show("Logging in...");
                LoadUserInfo(await web.DoLogin(UsernameField.text, PasswordField.text));
                HideLoginPanel();
            }
            catch (KiraWebException e)
            {
                if (e.isNetworkError)
                    messageBannerController.ShowMsg(LogLevel.ERROR, "Unable to connect to the server! Check your network");
                else
                    messageBannerController.ShowMsg(LogLevel.ERROR, e.Message);
            }
            catch (Exception e)
            {
                messageBannerController.ShowMsg(LogLevel.ERROR, e.Message);
            }
            finally
            {
                loadingBlocker.Close();
            }
            isAuthing = false;
        }

        public void OnRegisterClicked()
        {
            Application.OpenURL(web.ServerSite + "/user/reg");
        }

        private void LoadUserInfo(UserAuth user)
        {
            mActiveUser = user.User;
        }

        /// <summary>
        /// Prompts the user to login for the first time called. Otherwise returns !<see cref="isOfflineMode"/>.
        /// </summary>
        public async UniTask<bool> TryLogin()
        {
            if (!isOfflineMode)
                return true;
            if (LoginAttemptCount == 0)
            {
                if (isTokenSaved)
                {
                    try
                    {
                        loadingBlocker.Show("Logging in...");
                        LoadUserInfo(await web.DoRefreshAccessToken());
                        return true;
                    }
                    catch (KiraWebException e)
                    {
                        Debug.Log("Refresh token failed: " + e.Message);
                        if (e.isNetworkError)
                        {
                            LoginAttemptCount++;
                            return false;
                        }
                    }
                    finally
                    {
                        loadingBlocker.Close();
                    }
                }
                return await DoLogin();
            }
            return !isOfflineMode;
        }
    }
}