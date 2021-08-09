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
        private V2.ReplayFile demoFile;

        int currentIndex;

        List<KirakiraTouchState[]> convertedFrames = new List<KirakiraTouchState[]>();

        public DemoReplayTouchProvider(V2.ReplayFile demoFile)
        {
            this.demoFile = demoFile;

            currentIndex = 0;

            foreach (var frame in demoFile.frames)
            {
                var events = new List<KirakiraTouchState>();

                for (int i = 0; i < frame.events.Count; i++)
                {
                    var touchEvent = frame.events[i];

                    var ray = new Ray(touchEvent.pos.ToUnity(), Vector3.forward);
                    var worldPoint = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;

                    var e = new KirakiraTouchState
                    {
                        time = touchEvent.time,
                        touchId = touchEvent.touchId,
                        pos = touchEvent.pos.ToUnity(),
                        phase = (KirakiraTouchPhase)touchEvent.phase
                    };

                    events.Add(e);
                }

                convertedFrames.Add(events.ToArray());
            }
        }

        public KirakiraTouchState[][] GetTouches()
        {
            var frames = new List<KirakiraTouchState[]>();
            while (currentIndex < demoFile.frames.Count && demoFile.frames[currentIndex].judgeTime <= NoteController.judgeTime)
            {
                foreach(var e in convertedFrames[currentIndex])
                {
                    var ray = new Ray(e.pos, Vector3.forward);
                    var worldPoint = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;
                    e.screenPos = NoteController.mainCamera.WorldToScreenPoint(worldPoint);
                }

                frames.Add(convertedFrames[currentIndex]);
                currentIndex++;
            }

            return frames.ToArray();
        }
    }
}
