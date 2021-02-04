using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Zenject;
using BanGround.Identity;

public class UserInfo : MonoBehaviour
{
    [Inject]
    private IAccountManager accountManager;

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


        using (UnityWebRequest ub = UnityWebRequestTexture.GetTexture(userInfo.Avatar))
        {
            await ub.SendWebRequest();
            if (this == null)
                return;
            var tex = DownloadHandlerTexture.GetContent(ub);
            UserAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}