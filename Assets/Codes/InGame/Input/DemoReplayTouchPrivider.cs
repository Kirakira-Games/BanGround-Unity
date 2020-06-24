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
        private List<KirakiraTouchState> events = new List<KirakiraTouchState>();

        int currentIndex;

        public DemoReplayTouchPrivider(string demoFile)
        {
            using(var sr = new StreamReader(new DeflateStream(File.OpenRead(demoFile), CompressionMode.Decompress)))
            {
                var savedData = JsonConvert.DeserializeObject<DemoFile>(sr.ReadToEnd());

                foreach (var touch in savedData.events)
                    events.Add(touch);
            }

            events.Sort((a, b) =>
            {
                if (a.time > b.time)
                    return 1;

                if (a.time < b.time)
                    return -1;

                if ((int)a.phase > (int)a.phase)
                    return -1;

                if ((int)a.phase < (int)a.phase)
                    return 1;

                return 0;
            });

            currentIndex = 0;
        }

        public KirakiraTouchState[] GetTouches()
        {
            var states = new List<KirakiraTouchState>();
            while (currentIndex < events.Count && events[currentIndex].time < NoteController.audioTime)
            {
                var state = events[currentIndex];
                state.realtime = Time.realtimeSinceStartup;
                state.screenPos = NoteController.mainCamera.WorldToScreenPoint(state.pos);

                bool f = true;

                foreach(var s in states)
                {
                    if(s.touchId == state.touchId && (s.pos - state.pos).sqrMagnitude < 0.01)
                    {
                        f = false;
                        break;
                    }
                }

                if(f)
                    states.Add(state);

                currentIndex++;
            }

            return states.ToArray();
        }
    }
}
