using BanGround.Scene.Params;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BGEditor
{
    public class EditorPlayHandler : MonoBehaviour
    {
        public ModPanel modPanel;
        public Button Blocker;

        [Inject]
        private IAudioManager audioManager;

        [Inject]
        private IChartLoader chartLoader;

        [Inject]
        private IAudioProgressController audioProgress;

        public void Open()
        {
            if (modPanel.gameObject.activeSelf)
            {
                return;
            }
            modPanel.Refresh();
            audioProgress.Pause();
            Blocker.gameObject.SetActive(true);
            modPanel.gameObject.SetActive(true);
        }

        public void Close()
        {
            if (!modPanel.gameObject.activeSelf)
            {
                return;
            }
            Blocker.gameObject.SetActive(false);
            modPanel.gameObject.SetActive(false);
        }

        public void Play(bool fromStart)
        {
            var flag = modPanel.GetCurrentFlag();
            modPanel.Save();
            Close();

            float seekTime = fromStart ? 0 : audioManager.gameBGM.GetPlaybackTime() / 1000f;
            var parameters = SceneLoader.GetParamsOrDefault<MappingParams>();
            var param = new InGameParams
            {
                sid = parameters.sid,
                difficulty = parameters.difficulty,
                mods = flag,
                seekPosition = seekTime,
                saveRecord = false,
                saveReplay = false,
            };
            SceneLoader.LoadScene("InGame",
                () => chartLoader.LoadChart(parameters.sid, parameters.difficulty, true),
                pushStack: true,
                parameters: param);
        }
    }
}