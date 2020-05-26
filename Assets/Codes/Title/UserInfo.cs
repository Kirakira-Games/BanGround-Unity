using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class UserInfo : MonoBehaviour
{

    private const string Prefix = "https://api.reikohaku.fun/api";
    private const string API = "/auth/info?username=";
    private const string FullAPI = Prefix + API;

    public static UserInfoRespone result = null;
    public static string username;

    private Text username_Text;
    private Image userAvatar;

    private void Start()
    {
        username_Text = GameObject.Find("Username").GetComponent<Text>();
        userAvatar = GameObject.Find("Avatar").GetComponent<Image>();
    }

    public void ShowUserInfo()
    {
        StartCoroutine(GetUserInfo());
    }

    private IEnumerator GetUserInfo()
    {
        if (result == null) 
        {
            var req = new KirakiraWebRequest<UserInfoRespone>();
            yield return req.Get(FullAPI + username);
            result = req.resp;
        }
        username_Text.text = result.nickname;
        using (UnityWebRequest ub = UnityWebRequestTexture.GetTexture(result.avatar))
        {
            yield return ub.SendWebRequest();
            var tex = DownloadHandlerTexture.GetContent(ub);
            userAvatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}

public class UserInfoRespone
{
    public bool status;
    public string nickname;
    public string avatar;
}