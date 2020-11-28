using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using BanGround;
using System.Security.Cryptography;
using BrotliSharpLib;

public class ReplayFrame
{
    public int audioTime;
    public int judgeTime;

    public KirakiraTouchState[] events;
}

public class DemoFile
{
    public int sid;
    public Difficulty difficulty;
    public List<ReplayFrame> frames = new List<ReplayFrame>();

    public void Add(KirakiraTouchState[] kirakiraTouchStates)
    {
        var frame = new ReplayFrame
        {
            audioTime = NoteController.audioTime,
            events = new KirakiraTouchState[kirakiraTouchStates.Length]
        };

        for (int i = 0; i < kirakiraTouchStates.Length; i++)
        {
            frame.events[i] = new KirakiraTouchState
            {
                touchId = kirakiraTouchStates[i].touchId,
                phase = kirakiraTouchStates[i].phase,
                pos = kirakiraTouchStates[i].pos,
                time = kirakiraTouchStates[i].time
            };
        }

        frames.Add(frame);
    }

    const ushort version = 1;

    public void Save(IFile demofile, IFile[] fileList)
    {
        var dic = new Dictionary<string, long>();

        
        byte[] content = null;
        using (var ms = new MemoryStream())
        {
            using (var br = new BinaryWriter(ms, System.Text.Encoding.UTF8, true))
            {
                // char checksum[2]; "KP"
                br.Write((ushort)0x504bu);
                // ushort version;
                br.Write(version);

                // uint fileCount;
                br.Write(fileList.Length);
                // FileChecksum checksums[];
                foreach (var file in fileList)
                {
                    // char *fileName;
                    // write a placeholder and write the position of it later
                    dic.Add(file.Name, br.BaseStream.Position);
                    br.Write(0);

                    // char checksum[32];
                    // sha256 checksum
                    using (var sha256 = SHA256.Create())
                    {
                        using (var fs = file.Open(FileAccess.Read))
                        {
                            br.Write(sha256.ComputeHash(fs));
                        }
                    }
                }

                // uint frameCount;
                br.Write(frames.Count);

                foreach (var frame in frames)
                {
                    // uint judgeTime;
                    br.Write(frame.judgeTime);
                    // char eventCount;
                    br.Write((byte)frame.events.Length);

                    //KirakiraTouchState events[];
                    foreach (var e in frame.events)
                    {
                        // short deltaTime;
                        br.Write((short)(e.time - frame.judgeTime));
                        // char phase
                        br.Write((byte)e.phase);
                        // int touchId;
                        br.Write((byte)(e.touchId % 256));

                        // vec2 pos
                        br.Write((short)(e.pos.x * 64));
                        br.Write((short)(e.pos.y * 64));
                    }
                }

                foreach (var (str, pos) in dic)
                {
                    var strPointer = br.BaseStream.Position;

                    var bytes = System.Text.Encoding.UTF8.GetBytes(str);

                    br.Write(bytes);
                    br.Write(0);

                    var curPos = br.BaseStream.Position;

                    br.BaseStream.Position = pos;
                    br.Write((int)strPointer);

                    br.BaseStream.Position = curPos;
                }

                content = ms.ToArray();
            }
        }

        // var compressed = Brotli.CompressBuffer(content, 0, content.Length, 11);

        demofile.WriteBytes(content);//compressed);
    }

    public static DemoFile LoadFrom(string fileName)
    {
        DemoFile file = null;
        using (var sr = new StreamReader(new DeflateStream(File.OpenRead(fileName), CompressionMode.Decompress)))
        {
            file = JsonConvert.DeserializeObject<DemoFile>(sr.ReadToEnd());
        }

        return file;
    }
}

public class DemoRecorder
{
    public string demoName;
    DemoFile demoFile;

    public DemoRecorder(int chartId, Difficulty diff)
    {
        demoFile = new DemoFile
        {
            sid = chartId,
            difficulty = diff
        };

        demoName = $"{chartId}_{diff:g}_{DateTime.Now.ToLongDateString()}_{DateTime.Now.ToLongTimeString()}.kirareplay".Replace(":", "-").Replace("/", "-").Replace("\\", "-");
    }

    public void Add(KirakiraTouchState[] kirakiraTouchStates)
    {
        demoFile.Add(kirakiraTouchStates);
    }

    public void Save(IFile demofile, IFile[] fileList) => demoFile.Save(demofile, fileList);
}