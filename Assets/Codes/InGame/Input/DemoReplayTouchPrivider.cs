using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Codes.InGame.Input
{
    class DemoReplayTouchPrivider : KirakiraTouchProvider
    {
        private DemoFile demoFile;

        int currentIndex;

        public DemoReplayTouchPrivider(string demoFile)
        {
            this.demoFile = DemoFile.LoadFrom(demoFile);

            currentIndex = 0;
        }

        public KirakiraTouchState[][] GetTouches()
        {
            var frames = new List<KirakiraTouchState[]>();
            while (currentIndex < demoFile.frames.Count && demoFile.frames[currentIndex].audioTime <= NoteController.audioTime)
            {
                for(int i = 0; i < demoFile.frames[currentIndex].events.Length; i ++)
                {
                    demoFile.frames[currentIndex].events[i].screenPos = NoteController.mainCamera.WorldToScreenPoint(demoFile.frames[currentIndex].events[i].pos);
                    demoFile.frames[currentIndex].events[i].realtime = Time.realtimeSinceStartup;
                }

                frames.Add(demoFile.frames[currentIndex].events);

                currentIndex++;
            }

            return frames.ToArray();
        }
    }
}
