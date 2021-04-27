using System.IO;
using UnityEngine;
using AudioProvider;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Zenject;
using BanGround;
using System.Linq;
using BanGround.Identity;
using BanGround.Database.Migrations;
using System.Collections.Generic;

public class TitleLoader : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IMessageBannerController messageBannerController;
    [Inject]
    private ILoadingBlocker loadingBlocker;
    [Inject]
    private IFileSystem fs;
    [Inject]
    private IResourceLoader resourceLoader;
    [Inject]
    private IAccountManager accountManager;
    [Inject]
    private IMigrationManager migrationManager;
    [Inject]
    private LocalizedStrings localizedStrings;
    [Inject]
    private VersionCheck versionCheck;

    [Inject(Id = "cl_language")]
    KVar cl_language;

    public TextAsset titleMusic;
    public TextAsset[] voice;
    public Text Title;
    public Text touchStart;
    public Material backgroundMat;
    public MeshRenderer background;
    public UserInfo userCanvas;

    public InputField usernameField;
    public InputField passwordField;

    public ISoundTrack music;
    private ISoundEffect banGround;

    //const string BACKGROUND_PATH = "backgrounds";

    [Inject]
    private void Inject(IDataLoader dataLoader)
    {
        CheckUpdate();

        var backgrounds = dataLoader.chartList
            .Select(chart => dataLoader.GetBackgroundPath(chart.sid).Item1)
            .Where(path => path != null && fs.GetFile(path) != null)
            .ToArray();

        if (backgrounds.Length != 0)
        {
            var tex = resourceLoader.LoadTextureFromFs(
                backgrounds[Random.Range(0, backgrounds.Count())]
            );

            var matCopy = Instantiate(backgroundMat);

            matCopy.SetTexture("_MainTex", tex);
            matCopy.SetFloat("_TexRatio", tex.width / (float)tex.height);

            background.sharedMaterial = matCopy;
        }
    }

    private async UniTask RunMigrations()
    {
        if (!migrationManager.Init())
            return;

        var task = migrationManager.Migrate();
        loadingBlocker.Show("Migration.Prompt.Prepare".L());
        loadingBlocker.SetProgress(migrationManager);
        while (task.Status == UniTaskStatus.Pending)
        {
            loadingBlocker.SetText("Migration.Prompt.Progress".L(migrationManager.Description, migrationManager.CurrentMigrationIndex, migrationManager.TotalMigrations), true);
            await UniTask.WaitForEndOfFrame();
        }
        loadingBlocker.Close();
    }

    private async void Start()
    {
        userCanvas.GetUserInfo().Forget();
        PlayTitle().Forget();

        await RunMigrations();

        await UniTask.Delay(500);

        await accountManager.TryLogin();
        userCanvas.GetUserInfo().Forget();
    }

    async UniTask PlayTitle()
    {
        music = await audioManager.PlayLoopMusic(titleMusic.bytes);
        music.SetVolume(0.7f);
        await UniTask.Delay(2000); //yield return new WaitForSeconds(3f);

        banGround = await audioManager.PrecacheSE(voice[UnityEngine.Random.Range(0,voice.Length)].bytes);
        banGround.PlayOneShot();
    }

    static int[] ParseVersion(string v)
    {
        var v1 = v.Split('.');
        var result = new List<int>();

        foreach (var n in v1)
            result.Add(int.Parse(n));

        return result.ToArray();
    }

    static int CompareVersion(string a, string b)
    {
        var a1 = ParseVersion(a);
        var b1 = ParseVersion(b);

        for(int i = 0; i < 3; i++)
        {
            if (a1[i] > b1[i])
                return 1;
            else if (a1[i] < b1[i])
                return -1;
        }

        return 0;
    }

    async void CheckUpdate()
    {
        //MessageBoxController.ShowMsg(LogLevel.INFO, VersionCheck.CheckUpdate);
        TouchEvent te = GameObject.Find("TouchStart").GetComponent<TouchEvent>();
        var check = versionCheck;
        var data = await check.GetVersionInfo();

        if (data == null) 
        {
            //网络错误
            messageBannerController.ShowMsg(LogLevel.ERROR, VersionCheck.CheckError, false);
            te.waitingUpdate = false; // 椰叶先别强制更新罢
        }
        else if (CompareVersion(Application.version, data.version) < 0)
        {
            //有更新
            if (data.force)
            {
                string result = string.Format(VersionCheck.UpdateForce, data.version);
                //强制更新
                messageBannerController.ShowMsg(LogLevel.ERROR, result, false);
            }
            else
            {
                string result = string.Format(VersionCheck.UpdateNotForce, data.version);
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

    public void ShowCredits()
    {
        SceneLoader.LoadScene("Credits", pushStack: true);
    }

    private void OnDestroy()
    {
        music?.Dispose();
        banGround?.Dispose();
        localizedStrings.ReloadLanguageFile(cl_language);
        LocalizedText.ReloadAll();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) music?.Play();
        else music?.Pause();
    }
}