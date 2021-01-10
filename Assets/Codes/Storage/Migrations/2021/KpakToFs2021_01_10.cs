using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System.Linq;
using Zenject;
using System.IO.Compression;
using System.IO;
using System;

namespace BanGround.Database.Migrations
{
    public class KpakToFs2021_01_10 : MigrationBase
    {
        [Inject]
        private IFileSystem fs;

        public override int Id => 1;
        public override string Description => "Migrate kpak to local file system";

        public override async UniTask<bool> Commit()
        {
            Progress = 0;
            Debug.Assert(fs != null);

            var kpaks = fs.GetSearchPatchs().Where(path => path.EndsWith(".kpak"));
            var kpakStep = 1.0f / kpaks.Count();

            foreach(var kpak in kpaks)
            {
                fs.RemoveSearchPath(kpak);

                using(var archive = ZipFile.OpenRead(kpak))
                {
                    if(archive.Entries.Count == 0)
                    {
                        File.Delete(kpak);
                        Progress += kpakStep;
                        continue;
                    }

                    var fileStep = kpakStep / archive.Entries.Count;

                    foreach(var entry in archive.Entries)
                    {
                        if(entry.FullName.EndsWith("/") || entry.Length == 0)
                        {
                            Progress += fileStep;
                            continue;
                        }

                        var newPath = KiraPath.Combine(DataLoader.DataDir, entry.FullName);
                        if (File.Exists(newPath))
                        {
                            var newTime = entry.LastWriteTime.UtcDateTime;
                            var oldTime = File.GetLastWriteTimeUtc(newPath);

                            if (oldTime > newTime)
                            {
                                continue;
                            }
                        }

                        var newDirname = KiraPath.GetDirectoryName(newPath);

                        if (!Directory.Exists(newDirname))
                            Directory.CreateDirectory(newDirname);

                        using (var fs = File.OpenWrite(newPath))
                        {
                            using (var stream = entry.Open())
                            {
                                await stream.CopyToAsync(fs);
                                await fs.FlushAsync();
                            }
                        }

                        File.SetLastWriteTimeUtc(newPath, entry.LastWriteTime.UtcDateTime);
                        File.SetLastAccessTime(newPath, DateTime.Now);

                        Progress += fileStep;
                    }
                }

                File.Delete(kpak);
            }

            Progress = 1;
            return true;
        }
    }
}