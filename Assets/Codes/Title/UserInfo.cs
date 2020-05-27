using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniRx.Async;

public class UserInfo : MonoBehaviour
{

    private const string Prefix = "https://api.reikohaku.fun/api";
    private const string API = "/auth/info?username=";
    private const string FullAPI = Prefix + API;

    public static UserInfoResponse result = null;
    public static string username;

    private Text username_Text;
    private Image userAvatar;

    private void Start()
    {
        username_Text = GameObject.Find("Username").GetComponent<Text>();
        userAvatar = GameObject.Find("Avatar").GetComponent<Image>();
    }

    public async void GetUserInfo()
    {
        if (result == null)
        {
            result = await new KirakiraWebRequest<UserInfoResponse>().Get(FullAPI + username);
            if (result == null)
                return;
        }
        username_Text.text = result.nickname;
        using (UnityWebRequest ub = UnityWebRequestTexture.GetTexture(result.avatar))
        {
            await ub.SendWebRequest();
            var tex = DownloadHandlerTexture.GetContent(ub);
            userAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}

public class UserInfoResponse
{
    public bool status;
    public string nickname;
    public string avatar;
}