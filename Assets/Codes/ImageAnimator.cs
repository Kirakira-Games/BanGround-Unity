using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BanGround
{
    [RequireComponent(typeof(RawImage))]
    class ImageAnimator : MonoBehaviour
    {
        public TextAsset image;

        private RawImage target;

        float lastUpdate = -1;
        float delayTime = 0.0f;

        int curFrame = -1;
        Texture2D[] frames = null;

        private void Start()
        {
            target = GetComponent<RawImage>();

            var frameList = new List<Texture2D>();

            using(var zip = new ZipArchive(new MemoryStream(image.bytes)))
            {
                foreach(var item in zip.Entries)
                {
                    byte[] data = new byte[item.Length];
                    item.Open().Read(data, 0, (int)item.Length);

                    if (item.Name == "fps.txt")
                    {
                        delayTime = 1.0f / int.Parse(Encoding.UTF8.GetString(data));
                    }
                    else
                    {
                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(data);

                        frameList.Add(tex);
                    }
                }
            }

            frames = frameList.ToArray();
        }

        private void Update()
        {
            if (frames == null)
                return;

            if(Time.time - lastUpdate > delayTime)
            {
                lastUpdate = Time.time;

                if (++curFrame >= frames.Length)
                    curFrame = 0;

                target.texture = frames[curFrame];
            }
        }
    }
}
