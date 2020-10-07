using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Zenject;
using Web.Auth;
using WebSocketSharp;
using TMPro;

public class UserInfo : MonoBehaviour
{
    public static UserLite user;
    public static bool isOffline;
    private TextMeshPro username_Text;
    private Image userAvatar;

    private void Start()
    {
        username_Text = GameObject.Find("Username").GetComponent<TextMeshPro>();
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
            if (this == null)
                return;
            var tex = DownloadHandlerTexture.GetContent(ub);
            userAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}