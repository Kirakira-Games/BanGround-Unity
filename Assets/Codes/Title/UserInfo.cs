using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniRx.Async;
using Zenject;
using Web.Auth;
using WebSocketSharp;

public class UserInfo : MonoBehaviour
{
    public static UserLite user;
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

        if (user.Avatar == "N/A" || string.IsNullOrEmpty(user.Avatar))
            return;

        using (UnityWebRequest ub = UnityWebRequestTexture.GetTexture(user.Avatar))
        {
            await ub.SendWebRequest();
            if (gameObject == null)
                return;
            var tex = DownloadHandlerTexture.GetContent(ub);
            userAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}