using System.IO;
using UnityEngine;
using AudioProvider;
using UnityEngine.UI;
using UniRx.Async;
using Zenject;

public class TitleLoader : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IMessageBannerController messageBannerController;

    public TextAsset titleMusic;
    public TextAsset[] voice;
    public Text Title;
    public Text touchStart;
    public Material backgroundMat;
    public MeshRenderer background;

    public InputField usernameField;
    public InputField passwordField;

    public GameObject loginPanel;

    public static TitleLoader instance;

    public ISoundTrack music;
    private ISoundEffect banGround;

    private Authenticate auth = new Authenticate();

    const string BACKGROUND_PATH = "backgrounds";

    [Inject(Id = "cl_accesstoken")]
    KVar cl_accessToken;
    [Inject(Id = "cl_refreshtoken")]
    KVar cl_refreshToken;
    /*
     * Test User for editor:
     * Username:
     * unity_editor
     * Password:
     * Nic3P4ssword
     */

    [Inject]
    private void Inject(IDataLoader dataLoader)
    {
        instance = this;
        CheckUpdate();
        _ = dataLoader.Init();

        var backgrounds = KiraFilesystem.Instance.ListFiles(filename => filename.StartsWith(BACKGROUND_PATH));

        if (backgrounds.Length != 0)
        {
            var tex = KiraFilesystem.Instance.ReadTexture2D(backgrounds[Random.Range(0, backgrounds.Length)]);

            var matCopy = Instantiate(backgroundMat);

            matCopy.SetTexture("_MainTex", tex);
            matCopy.SetFloat("_TexRatio", tex.width / tex.height);

            background.sharedMaterial = matCopy;
        }
    }

    private void Start()
    {
        PlayTitle();

        if (!string.IsNullOrEmpty(cl_accessToken) && !string.IsNullOrEmpty(cl_refreshToken))
            _ = auth.TryAuthenticate();

        //MessageBoxController.ShowMsg(LogLevel.INFO, SystemInfo.deviceUniqueIdentifier.Substring(0, 8));
    }

    async void PlayTitle()
    {
        //yield return new WaitForSeconds(0.5f);
        music = await audioManager.PlayLoopMusic(titleMusic.bytes);
        music.SetVolume(0.7f);
        await UniTask.Delay(3000); //yield return new WaitForSeconds(3f);

        banGround = await audioManager.PrecacheSE(voice[UnityEngine.Random.Range(0,voice.Length)].bytes);
        banGround.PlayOneShot();
    }

    public void HideLoginPanel()
    {
        loginPanel.SetActive(false);
    }

    public async void SubmitLogin()
    {
        if (Authenticate.isAuthing)
            return;

        var good = await auth.TryAuthenticate(usernameField.text, passwordField.text, false);

        if (good)
        {
            loginPanel.SetActive(false);
        }
        else
        {
            if(Authenticate.isNetworkError)
                messageBannerController.ShowMsg(LogLevel.ERROR, "Unable to connect to the server! Check your network");
            else
                messageBannerController.ShowMsg(LogLevel.ERROR, "Username or Password is wrong!");
        }
    }

    public void OnRegisterClicked()
    {
        Application.OpenURL("https://banground.live/user/reg");
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

    [Inject(Id = "cl_language")]
    KVar cl_language;

    private void OnDestroy()
    {
        music?.Dispose();
        banGround?.Dispose();
        LocalizedStrings.Instanse.ReloadLanguageFile(cl_language);
        LocalizedText.ReloadAll();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) music?.Play();
        else music?.Pause();
    }
}