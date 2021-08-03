using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Zenject;
using BanGround;
using BanGround.Identity;
using System.Net;

public class UserInfo : MonoBehaviour
{
    [Inject]
    private IAccountManager accountManager;
    [Inject]
    private IMessageBox messageBox;
    [Inject]
    private IFileSystem fs;

    public GameObject FishDisplay;
    public GameObject LevelDisplay;
    public Text UsernameText;
    public Text FishText;
    public Text LevelText;
    public Image UserAvatar;

    public async UniTaskVoid GetUserInfo()
    {
        if (this == null)
            return;

        var user = accountManager.ActiveUser;
        UsernameText.text = user.Nickname;

        var userInfo = accountManager.ActiveUserInfo;

        if (userInfo == null)
        {
            FishDisplay.SetActive(false);
            LevelDisplay.SetActive(false);
            return;
        }

        FishText.text = userInfo.Fish.ToString();

        // ?
        //LevelText.text = ?;

        if (userInfo.Avatar != null) 
        {
            var tex = await userInfo.GetAvatarTexure();

            if (tex == null || this == null)
                return;

            UserAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }

    public async void OnClickedAvatar()
    {
        if (accountManager.isOfflineMode)
            return;

        if (!await messageBox.ShowMessage("Account.Title.Logout".L(), "Account.Prompt.Logout".L()))
            return;

        accountManager.GoOffline();
        GetUserInfo().Forget();
    }
}