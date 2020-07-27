using UnityEngine.UI;
using System;
using System.Linq;

namespace BGEditor
{
    public class MetaDataController : CoreMonoBehaviour
    {
        public InputField Title;
        public InputField Artist;
        public FloatInput[] BPM;
        public FloatInput[] MusicPreview;
        public InputField Designer;
        public IntInput Difficulty;
        public Text DifficultyText;
        public InputField Tags;
        public FloatInput[] ChartPreview;

        private mHeader mHeader;
        private cHeader cHeader;

        private void Awake()
        {
            cHeader = LiveSetting.CurrentHeader;
            mHeader = DataLoader.GetMusicHeader(cHeader.mid);
        }

        public void Show()
        {
            if (gameObject.activeSelf)
                return;
            // just fill in
            float duration = Progress.audioLength / 1000f;

            Blocker.gameObject.SetActive(true);
            gameObject.SetActive(true);
            Title.text = mHeader.title;
            Artist.text = mHeader.artist;
            BPM[0].SetValue(mHeader.BPM[0]);
            BPM[1].SetValue(mHeader.BPM.Length > 1 ? mHeader.BPM[1] : mHeader.BPM[0]);
            MusicPreview[0].MaxVal = duration;
            MusicPreview[1].MaxVal = duration;
            if (mHeader.preview != null)
            {
                MusicPreview[0].SetValue(mHeader.preview[0]);
                MusicPreview[1].SetValue(mHeader.preview[1]);
            }
            else
            {
                MusicPreview[0].SetValue(0f);
                MusicPreview[1].SetValue(duration);
            }

            Designer.text = cHeader.authorNick;
            Difficulty.SetValue(cHeader.difficultyLevel[LiveSetting.actualDifficulty]);
            DifficultyText.text = Enum.GetName(typeof(Difficulty), LiveSetting.actualDifficulty);
            Tags.text = string.Join(",", cHeader.tag);
            ChartPreview[0].MaxVal = duration;
            ChartPreview[1].MaxVal = duration;
            if (cHeader.preview != null)
            {
                ChartPreview[0].SetValue(cHeader.preview[0]);
                ChartPreview[1].SetValue(cHeader.preview[1]);
            }
            else
            {
                ChartPreview[0].SetValue(0f);
                ChartPreview[1].SetValue(duration);
            }
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
            cHeader.difficultyLevel[LiveSetting.actualDifficulty] = Difficulty.value;
            Chart.level = Difficulty.value;
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

            DataLoader.SaveHeader(mHeader);
            DataLoader.SaveHeader(cHeader);
            Core.Save();
        }

        public async void Hide(bool save)
        {
            if (save)
            {
                if (!await MessageBox.ShowMessage("Save Metadata", "To overwrite metadata, your chart will be saved. Continue?"))
                    return;
                Save();
            }
            Blocker.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}