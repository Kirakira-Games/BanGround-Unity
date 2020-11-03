using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using BanGround;

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

        for(int i = 0; i < kirakiraTouchStates.Length; i++)
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

    public void Save(string fileName)
    {
        /* TODO: Use protobuf
        using (var sw = new StreamWriter(new DeflateStream(File.OpenWrite(fileName), System.IO.Compression.CompressionLevel.Optimal)))
        {
            sw.Write(JsonConvert.SerializeObject(this));
        }*/
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
    string demoName;
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

    public void Save() => demoFile.Save(KiraPath.Combine(DataLoader.DataDir, demoName));
} 