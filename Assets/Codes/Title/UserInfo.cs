using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniRx.Async;

public class UserInfo : MonoBehaviour
{
    private Text username_Text;
    private Image userAvatar;

    public static string username;

    private void Start()
    {
        username_Text = GameObject.Find("Username").GetComponent<Text>();
        userAvatar = GameObject.Find("Avatar").GetComponent<Image>();
    }

    public async void GetUserInfo()
    {
        if (Authenticate.user == null)
            return;

        username_Text.text = Authenticate.user.Username;
        username = Authenticate.user.Username;

        if (Authenticate.user.Avatar == "N/A")
            return;

        using (UnityWebRequest ub = UnityWebRequestTexture.GetTexture(Authenticate.user.Avatar))
        {
            await ub.SendWebRequest();
            var tex = DownloadHandlerTexture.GetContent(ub);
            userAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}