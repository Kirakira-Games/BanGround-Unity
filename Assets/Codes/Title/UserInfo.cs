using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniRx.Async;
using Zenject;
using Web.Auth;

public class UserInfo : MonoBehaviour
{
    public static User user;
    public static bool isOffline;
    private Text username_Text;
    private Image userAvatar;

    private void Start()
    {
        username_Text = GameObject.Find("Username").GetComponent<Text>();
        userAvatar = GameObject.Find("Avatar").GetComponent<Image>();
    }

    public async UniTaskVoid GetUserInfo()
    {
        if (user == null)
            return;

        username_Text.text = user.Nickname;

        if (user.Avatar == "N/A")
            return;

        using (UnityWebRequest ub = UnityWebRequestTexture.GetTexture(user.Avatar))
        {
            await ub.SendWebRequest();
            var tex = DownloadHandlerTexture.GetContent(ub);
            userAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}