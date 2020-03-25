using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace System.IO
{
    class KiraFilesystem
    {
        public static KiraFilesystem Instance;

        // file, kirapack
        Dictionary<string, string> index;
        string indexFile;
        public KiraFilesystem(string indexFile)
        {
            if (Instance == null)
                Instance = this;

            this.indexFile = indexFile;

            if (File.Exists(indexFile))
            {
                using (FileStream fs = File.OpenRead(indexFile))
                {
                    var bf = new BinaryFormatter();
                    index = bf.Deserialize(fs) as Dictionary<string, string>;
                }
            }
            else
            {
                index = new Dictionary<string, string>();
            }
        }

        public void AddToIndex(string kiraPack)
        {
            using (var s = File.OpenRead(kiraPack))
            {
                using (var zip = new ZipArchive(s))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (index.ContainsKey(entry.FullName))
                            index.Remove(entry.FullName);

                        index.Add(entry.FullName, kiraPack);
                    }
                }
            }
        }

        public void RemoveFromIndex(string kiraPack)
        {
            var entries = (from x in index where x.Value == kiraPack select x.Key).ToArray<string>();

            foreach (var entry in entries)
                index.Remove(entry);
        }

        public void SaveIndex()
        {
            if (File.Exists(indexFile))
                File.Delete(indexFile);

            using (FileStream fs = File.OpenWrite(indexFile))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, index);
            }
        }

        public string[] IndexedFiles
        {
            get
            {
                string[] files = new string[index.Count];
                int i = 0;
                foreach (var kv in index)
                    files[i++] = kv.Key;

                return files;
            }
        }

        public byte[] Read(string fileName)
        {
            if (!index.ContainsKey(fileName))
                throw new FileNotFoundException($"File {fileName} not found in filesystem.");

            var targetKirapack = index[fileName];

            byte[] buffer;

            using (var s = File.OpenRead(targetKirapack))
            {
                using (var zip = new ZipArchive(s))
                {
                    var entry = zip.GetEntry(fileName);
                    using (var stream = entry.Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);

                            buffer = new byte[ms.Length];
                            ms.Read(buffer, 0, buffer.Length);
                        }
                    }
                }
            }

            return buffer;
        }

        public string ReadString(string fileName, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            var bytes = Read(fileName);

            return encoding.GetString(bytes);
        }

        public Texture2D ReadTexture2D(string fileName)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(Read(fileName));

            return tex;
        }
    }
}
