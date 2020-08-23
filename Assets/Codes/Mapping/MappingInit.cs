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
        private static IEnumerator InitCoroutine(ChartCore core)
        {
            yield return new WaitForEndOfFrame();
            core.LoadChart();
        }

        public static void Init(ChartCore core)
        {
            core.StartCoroutine(InitCoroutine(core));
        }
    }
}
