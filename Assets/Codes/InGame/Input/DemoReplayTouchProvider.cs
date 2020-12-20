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
    class DemoReplayTouchProvider : IKirakiraTouchProvider
    {
        private DemoFile demoFile;

        int currentIndex;

        public DemoReplayTouchProvider(DemoFile demoFile)
        {
            this.demoFile = demoFile;

            currentIndex = 0;
        }

        public KirakiraTouchState[][] GetTouches()
        {
            var frames = new List<KirakiraTouchState[]>();
            while (currentIndex < demoFile.frames.Count && demoFile.frames[currentIndex].judgeTime <= NoteController.judgeTime)
            {
                for(int i = 0; i < demoFile.frames[currentIndex].events.Length; i ++)
                {
                    var touchEvent = demoFile.frames[currentIndex].events[i];
                    var ray = new Ray(touchEvent.pos, Vector3.forward);
                    var worldPoint = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;

                    touchEvent.screenPos = NoteController.mainCamera.WorldToScreenPoint(worldPoint);
                }

                frames.Add(demoFile.frames[currentIndex].events);

                currentIndex++;
            }

            return frames.ToArray();
        }
    }
}
