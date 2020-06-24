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
        List<KeyValuePair<float, KirakiraTouchState[]>> demoContent = new List<KeyValuePair<float, KirakiraTouchState[]>>();

        int currentIndex;

        public DemoReplayTouchPrivider(string demoFile)
        {
            using(var sr = new StreamReader(new DeflateStream(File.OpenRead(demoFile), CompressionMode.Decompress)))
            {
                var savedData = JsonConvert.DeserializeObject<DemoFile>(sr.ReadToEnd());

                foreach(var (time, touches) in savedData.demoContent)
                    demoContent.Add(new KeyValuePair<float, KirakiraTouchState[]>(time, KirakiraSerializableTouchState.To(touches)));
            }

            currentIndex = 0;
        }

        public KirakiraTouchState[] GetTouches()
        {
            var states = new List<KirakiraTouchState>();
            while(currentIndex < demoContent.Count && demoContent[currentIndex].Key <= NoteController.audioTime)
            {
                var cur = demoContent[currentIndex].Value;

                foreach (var state in cur)
                {
                    state.time = NoteController.audioTime;
                    state.realtime = Time.realtimeSinceStartup;
                    state.screenPos = NoteController.mainCamera.WorldToScreenPoint(state.pos);
                }

                states.AddRange(cur);

                currentIndex++;
            }

            return states.ToArray();
        }
    }
}
