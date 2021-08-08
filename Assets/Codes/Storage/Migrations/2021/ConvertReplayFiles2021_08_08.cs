using Cysharp.Threading.Tasks;
using System.IO;
using System.Linq;
using Zenject;

namespace BanGround.Database.Migrations
{
    class ConvertReplayFiles2021_08_08 : MigrationBase
    {
        [Inject]
        private IFileSystem fs;

        public override int Id => 6;
        public override string Description => "Convert old binary replay files to protobuf";

        public override UniTask<bool> Commit()
        {
            Progress = 0;

            var files = fs.ListDirectory("replay");
            var p = 1.0f / files.Count();

            foreach (var file in files)
            {
                try
                {
                    var demo = DemoFile.LoadFrom(file);
                    var replay = new V2.ReplayFile
                    {
                        uid = demo.uid,
                        sid = demo.sid,
                        difficulty = demo.difficulty,
                        mods = demo.mods,
                    };

                    replay.checksums.AddRange(
                        demo.checksums.Select(f => new V2.FileChecksum
                        {
                            file = f.file,
                            checksum = f.checksum
                        })
                    );

                    foreach (var frame in demo.frames)
                    {
                        var f = new V2.ReplayFrame
                        {
                            judgeTime = frame.judgeTime,
                        };

                        f.events.AddRange(
                            frame.events.Select(e => new V2.ReplayTouchState {
                                touchId = e.touchId,
                                phase = (int)e.phase,
                                pos = e.pos.ToProto(),
                                time = e.time
                            })
                        );

                        replay.frames.Add(f);
                    }

                    ProtobufHelper.Write(replay, file);
                    Progress += p;
                }
                catch (InvalidDataException)
                {
                    // already converted?
                    break;
                }
            }

            Progress = 1;
            return UniTask.FromResult(true);
        }
    }
}
