﻿using System.IO;
using UnityEngine;
using AudioProvider;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Zenject;
using BanGround;
using System.Linq;
using BanGround.Identity;
using System;
using BanGround.Database.Migrations;

public class TitleLoader : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IMessageBannerController messageBannerController;
    [Inject]
    private ILoadingBlocker loadingBlocker;
    [Inject]
    private IFileSystem fs;
    [Inject]
    private IAccountManager accountManager;
    [Inject]
    private IMigrationManager migrationManager;
    [Inject]
    private LocalizedStrings localizedStrings;

    [Inject(Id = "cl_language")]
    KVar cl_language;

    public TextAsset titleMusic;
    public TextAsset[] voice;
    public Text Title;
    public Text touchStart;
    public Material backgroundMat;
    public MeshRenderer background;
    public UserInfo userCanvas;

    public InputField usernameField;
    public InputField passwordField;

    public static TitleLoader instance;

    public ISoundTrack music;
    private ISoundEffect banGround;

    //const string BACKGROUND_PATH = "backgrounds";

    [Inject]
    private void Inject()
    {
        instance = this;
        CheckUpdate();

        var backgrounds = fs.Find(file => file.Name.EndsWith(".jpg") || file.Name.EndsWith(".png") || file.Name.EndsWith(".jpeg"));

        if (backgrounds.Count() != 0)
        {
            var tex = backgrounds.ElementAt(UnityEngine.Random.Range(0, backgrounds.Count())).ReadAsTexture();

            var matCopy = Instantiate(backgroundMat);

            matCopy.SetTexture("_MainTex", tex);
            matCopy.SetFloat("_TexRatio", tex.width / (float)tex.height);

            background.sharedMaterial = matCopy;
        }
    }

    private async UniTask RunMigrations()
    {
        if (!migrationManager.Init())
            return;

        var task = migrationManager.Migrate();
        loadingBlocker.Show($"Preparing migrations...");
        while (task.Status == UniTaskStatus.Pending)
        {
            loadingBlocker.SetText($"Migrating: {migrationManager.CurrentMigrationIndex} / {migrationManager.TotalMigrations}", true);
            loadingBlocker.SetProgress(migrationManager.CurrentMigrationProgress);
            await UniTask.WaitForEndOfFrame();
        }
        loadingBlocker.Close();
    }

    private async void Start()
    {
        userCanvas.GetUserInfo().Forget();
        PlayTitle().Forget();

        await RunMigrations();

        await UniTask.Delay(500);

        await accountManager.TryLogin();
        userCanvas.GetUserInfo().Forget();
    }

    async UniTask PlayTitle()
    {
        music = await audioManager.PlayLoopMusic(titleMusic.bytes);
        music.SetVolume(0.7f);
        await UniTask.Delay(2000); //yield return new WaitForSeconds(3f);

        banGround = await audioManager.PrecacheSE(voice[UnityEngine.Random.Range(0,voice.Length)].bytes);
        banGround.PlayOneShot();
    }

    async void CheckUpdate()
    {
        //MessageBoxController.ShowMsg(LogLevel.INFO, VersionCheck.CheckUpdate);
        TouchEvent te = GameObject.Find("TouchStart").GetComponent<TouchEvent>();
        var check = VersionCheck.Instance;
        await check.GetVersionInfo();

        if (check == null || check.response == null || check.response.result == false) 
        {
            //网络错误
            messageBannerController.ShowMsg(LogLevel.ERROR, VersionCheck.CheckError, false);
            te.waitingUpdate = false; // 椰叶先别强制更新罢
        }
        else if (Application.version != check.response.data.version)
        {
            //有更新
            if (check.response.data.force)
            {
                string result = string.Format(VersionCheck.UpdateForce, check.response.data.version);
                //强制更新
                messageBannerController.ShowMsg(LogLevel.ERROR, result, false);
            }
            else
            {
                string result = string.Format(VersionCheck.UpdateNotForce, check.response.data.version);
                //不强制更新
                messageBannerController.ShowMsg(LogLevel.OK, result, true);
                te.waitingUpdate = false;
            }
        }
        else
        {
            //无更新
            messageBannerController.ShowMsg(LogLevel.OK, VersionCheck.NoUpdate, true);
            te.waitingUpdate = false;
        }
    }

    public void ShowCredits()
    {
        SceneLoader.LoadScene("Credits", pushStack: true);
    }

    private void OnDestroy()
    {
        music?.Dispose();
        banGround?.Dispose();
        localizedStrings.ReloadLanguageFile(cl_language);
        LocalizedText.ReloadAll();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) music?.Play();
        else music?.Pause();
    }
}