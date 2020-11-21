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

    public Text UsernameText;
    public Image UserAvatar;

    public async UniTaskVoid GetUserInfo()
    {
        if (this == null)
            return;
        var user = accountManager.ActiveUser;
        UsernameText.text = user.Nickname;

        if (string.IsNullOrEmpty(user.Avatar))
            return;

        using (UnityWebRequest ub = UnityWebRequestTexture.GetTexture(user.Avatar))
        {
            await ub.SendWebRequest();
            if (this == null)
                return;
            var tex = DownloadHandlerTexture.GetContent(ub);
            UserAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}