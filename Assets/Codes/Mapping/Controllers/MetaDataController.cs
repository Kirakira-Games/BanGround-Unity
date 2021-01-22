using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Zenject;
using BanGround.Web;
using Cysharp.Threading.Tasks;
using BanGround.Utils;

namespace BGEditor
{
    public class MetaDataController : MonoBehaviour
    {
        [Inject]
        private IChartCore Core;
        [Inject]
        private IDataLoader dataLoader;
        [Inject]
        private IChartLoader chartLoader;
        [Inject]
        private IAudioProgressController Progress;
        [Inject]
        private IMessageBox messageBox;
        [Inject(Id = "Blocker")]
        private Button Blocker;

        public InputField Title;
        public InputField Artist;
        public FloatInput[] BPM;
        public FloatInput[] MusicPreview;
        public InputField Designer;
        public IntInput Difficulty;
        public Text DifficultyText;
        public InputField Tags;
        public FloatInput[] ChartPreview;
        public Text MidTxt;
        public Text SidTxt;

        private mHeader mHeader;
        private cHeader cHeader;
        private byte[] cover;
        private string coverExt;

        public void Show()
        {
            if (gameObject.activeSelf)
                return;

            cHeader = chartLoader.header;
            mHeader = dataLoader.GetMusicHeader(cHeader.mid);
            // just fill in
            float duration = Progress.audioLength / 1000f;
            mHeader.Sanitize();
            cHeader.Sanitize(mHeader);

            // Id & source
            var musicSource = IDRouterUtil.GetSource(mHeader.mid, out int mid);
            var chartSource = IDRouterUtil.GetSource(cHeader.sid, out int sid);
            MidTxt.text = $"{musicSource}: {mid}";
            SidTxt.text = $"{chartSource}: {sid}";

            // Misc
            Blocker.gameObject.SetActive(true);
            gameObject.SetActive(true);
            Title.text = mHeader.title;
            Artist.text = mHeader.artist;

            BPM[0].SetValue(mHeader.BPM[0]);
            BPM[1].SetValue(mHeader.BPM[1]);

            MusicPreview[0].MaxVal = duration;
            MusicPreview[1].MaxVal = duration;
            MusicPreview[0].SetValue(mHeader.preview[0]);
            MusicPreview[1].SetValue(mHeader.preview[1]);

            Designer.text = cHeader.authorNick;
            Difficulty.SetValue(chartLoader.chart.level);
            DifficultyText.text = chartLoader.chart.difficulty.ToString();
            Tags.text = string.Join(",", cHeader.tag);

            ChartPreview[0].MaxVal = duration;
            ChartPreview[1].MaxVal = duration;
            ChartPreview[0].SetValue(cHeader.preview[0]);
            ChartPreview[1].SetValue(cHeader.preview[1]);

            cover = null;
        }

        public void Save()
        {
            // reverse the above
            mHeader.title = Title.text;
            mHeader.artist = Artist.text;
            int swap = BPM[0].value > BPM[1].value ? 1 : 0;
            mHeader.BPM = new float[]
            {
                BPM[0 ^ swap].value,
                BPM[1 ^ swap].value
            };
            swap = MusicPreview[0].value > MusicPreview[1].value ? 1 : 0;
            mHeader.preview = new float[]
            {
                MusicPreview[0 ^ swap].value,
                MusicPreview[1 ^ swap].value
            };

            cHeader.authorNick = Designer.text;
            cHeader.difficultyLevel[(int)chartLoader.chart.difficulty] = Difficulty.value;
            Core.chart.level = Difficulty.value;
            cHeader.tag = (from tag in Tags.text.Split(',')
                           where tag.Trim().Length > 0
                           select tag.Trim())
                          .ToList();
            swap = ChartPreview[0].value > ChartPreview[1].value ? 1 : 0;
            cHeader.preview = new float[]
            {
                ChartPreview[0 ^ swap].value,
                ChartPreview[1 ^ swap].value
            };

            if (cover != null)
                cHeader.backgroundFile.pic = "bg" + coverExt;

            dataLoader.SaveHeader(mHeader);
            dataLoader.SaveHeader(cHeader, coverExt, cover);
            Core.Save();
        }

        public async void Hide(bool save)
        {
            if (save)
            {
                if (!await messageBox.ShowMessage("Editor.Title.SaveMetaData".L(), "Editor.Prompt.SaveMetaDataInfo".L()))
                    return;
                Save();
            }
            Blocker.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public async void SelectCover()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var sfd = await new SelectFileDialog()
           .SetFilter("File contains cover\0*.jpg;*.png;*.flac;*.mp3;*.aac\0")
           .SetTitle("Select Cover file")
           .SetDefaultExt("jpg")
           .ShowAsync();

            if (sfd.IsSucessful)
            {
                var coverfi = new System.IO.FileInfo(sfd.File);

                if (coverfi.Extension == ".jpg" || coverfi.Extension == ".png")
                {
                    cover = System.IO.File.ReadAllBytes(coverfi.FullName);
                    coverExt = coverfi.Extension;
                }
                else
                {
                    var coverFile = TagLib.File.Create(coverfi.FullName);

                    if (coverFile.Tag.Pictures.Length > 0)
                    {
                        var pic = coverFile.Tag.Pictures[0];

                        cover = pic.Data.ToArray();
                        coverExt = pic.MimeType.Replace("image/", ".");
                    }
                }
            }
#elif (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                bool cancel = false;
                string coverPath = null;

                NativeGallery.GetImageFromGallery(path =>
                {
                    if (path == null)
                        cancel = true;

                    coverPath = path;
                });

                await UniTask.WaitUntil(() => cancel || coverPath != null);

                if (!cancel && coverPath != null)
                {
                    var texture = NativeGallery.LoadImageAtPath(coverPath, -1, false, false, true);
                    cover = texture.EncodeToJPG(75);

                    coverExt = ".jpg";

                    Destroy(texture);
                }
#endif
        }
    }
}