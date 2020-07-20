using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx.Async;
using UnityEngine;

namespace BGEditor
{
    public static class MappingInit
    {
        private static V2.Chart chart;

        private static IEnumerator InitCoroutine(ChartCore core)
        {
            yield return new WaitForEndOfFrame();
            core.LoadChart(chart);
        }

        public static void Init(ChartCore core)
        {
            // Load chart
            chart = LiveSetting.chart;

            // Load music
            core.progress.Init(KiraFilesystem.Instance.Read(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)));

            core.StartCoroutine(InitCoroutine(core));
        }
    }
}
