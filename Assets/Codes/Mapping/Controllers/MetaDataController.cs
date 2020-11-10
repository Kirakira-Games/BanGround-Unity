using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Zenject;
using BanGround.Web;
using Cysharp.Threading.Tasks;

namespace BGEditor
{
    public class MetaDataController : MonoBehaviour
    {
        [Inject]
        private IChartCore Core;
        [Inject]
        private IDataLoader dataLoader;
        [Inject]
        private IChartListManager chartListManager;
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

        public void Show()
        {
            if (gameObject.activeSelf)
                return;

            cHeader = chartListManager.current.header;
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
            Difficulty.SetValue(cHeader.difficultyLevel[(int)chartListManager.current.difficulty]);
            DifficultyText.text = Enum.GetName(typeof(Difficulty), chartListManager.current.difficulty);
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
            cHeader.difficultyLevel[(int)chartListManager.current.difficulty] = Difficulty.value;
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

            dataLoader.SaveHeader(mHeader);
            dataLoader.SaveHeader(cHeader, ".jpg", cover);
            Core.Save();
        }

        public async void Hide(bool save)
        {
            if (save)
            {
                if (!await messageBox.ShowMessage("Save Metadata", "To overwrite metadata, your chart will be saved. Continue?"))
                    return;
                Save();
            }
            Blocker.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public async void SelectCover()
        {
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

                Destroy(texture);
            }
        }
    }
}