using BanGround.Database;
//using Boo.Lang;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace BanGround
{
    public class ExtractKirapackTask : ITaskWithProgress
    {
        public float Progress { get; private set; }

        private string path;
        private IFileSystem fs;
        private IDatabaseAPI db;
        private IDataLoader dataLoader;
        private bool forceOverwrite;

        public ExtractKirapackTask(string path, IFileSystem fs, IDatabaseAPI db, IDataLoader dataLoader, bool forceOverwrite = false)
        {
            this.path = path;
            this.fs = fs;
            this.db = db;
            this.dataLoader = dataLoader;
            this.forceOverwrite = forceOverwrite;
        }

        private async UniTask ConvertJsonToBin(string path)
        {
            if (!path.StartsWith("chart") && !path.StartsWith("music"))
                return;

            var file = fs.GetFile(path);
            var binPath = path.Replace(".json", ".bin");
            Type type = null;
            var name = KiraPath.GetFileName(path);

            if (name == "cheader.json")
                type = typeof(cHeader);
            else if (name == "mheader.json")
                type = typeof(mHeader);

            object obj;
            using (var sr = new StreamReader(file.Open(FileAccess.Read)))
            {
                var json = await sr.ReadToEndAsync();
                if (type == null)
                {
                    // v2!
                    if (json.Contains("\"version\""))
                    {
                        obj = JsonConvert.DeserializeObject<V2.Chart>(json);
                    }
                    else
                    {
                        obj = JsonConvert.DeserializeObject<Chart>(json);
                    }
                }
                else
                {
                    obj = JsonConvert.DeserializeObject(json, type);
                }
            }

            Debug.Log("Convert " + path + " to " + binPath);
            var newFile = fs.GetOrNewFile(binPath);
            ProtobufHelper.Write(obj, newFile);
            file.Delete();
        }

        public async UniTask Run()
        {
            Progress = 0;
            var jsonFiles = new List<string>();
            var cheaders = new List<string>();

            using (var archive = ZipFile.OpenRead(path))
            {
                if (archive.Entries.Count > 0)
                {
                    var fileStep = 0.9f / archive.Entries.Count;

                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("/") || entry.Length == 0)
                        {
                            Progress += fileStep;
                            continue;
                        }

                        var newPath = KiraPath.Combine(DataLoader.DataDir, entry.FullName);
                        if (fs.FileExists(newPath))
                        {
                            var newTime = entry.LastWriteTime.UtcDateTime;
                            var oldTime = File.GetLastWriteTimeUtc(newPath);

                            if (oldTime > newTime && !forceOverwrite)
                            {
                                continue;
                            }
                        }

                        var newDirname = KiraPath.GetDirectoryName(newPath);

                        if (!Directory.Exists(newDirname))
                            Directory.CreateDirectory(newDirname);

                        using (var fstream = File.OpenWrite(newPath))
                        {
                            using (var stream = entry.Open())
                            {
                                await stream.CopyToAsync(fstream);
                                await fstream.FlushAsync();
                            }
                        }

                        File.SetLastWriteTimeUtc(newPath, entry.LastWriteTime.UtcDateTime);
                        File.SetLastAccessTime(newPath, DateTime.Now);

                        string nameLower = entry.Name.ToLower();
                        if (nameLower.EndsWith(".json"))
                            jsonFiles.Add(entry.FullName);

                        if (nameLower.EndsWith("cheader.bin") || nameLower.EndsWith("cheader.json"))
                            cheaders.Add(KiraPath.Combine(
                                KiraPath.GetDirectoryName(entry.FullName), "cheader.bin"));

                        Progress += fileStep;
                    }
                }
            }
            File.Delete(path);

            // Convert json to bin
            if (jsonFiles.Count > 0)
            {
                var convertStep = (1 - Progress) / jsonFiles.Count;
                foreach (var file in jsonFiles)
                {
                    await ConvertJsonToBin(file);
                    Progress += convertStep;
                }
            }

            // Register charts
            foreach (var header in cheaders)
            {
                try
                {
                    var cheader = ProtobufHelper.Load<cHeader>(fs.GetFile(header));
                    cheader.LoadDifficultyLevels(dataLoader);
                    db.SaveChartSet(cheader.sid, cheader.mid, cheader.difficultyLevel.ToArray());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError("While trying to read cHeader from " + header);
                }
            }

            Progress = 1;
        }
    }
}