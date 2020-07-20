using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BGEditor
{
    public static class MappingInit
    {
        private static Chart chart;

        private static IEnumerator InitCoroutine(ChartCore core)
        {
            yield return new WaitForEndOfFrame();
            core.LoadChart(chart);
        }

        public static void Init(ChartCore core)
        {
            // Load chart
            int sid = LiveSetting.CurrentHeader.sid;
            Difficulty difficulty = (Difficulty)LiveSetting.actualDifficulty;
            chart = DataLoader.LoadChart<Chart>(sid, difficulty);

            // Load music
            core.progress.Init(KiraFilesystem.Instance.Read(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)));

            core.StartCoroutine(InitCoroutine(core));
        }
    }
}
