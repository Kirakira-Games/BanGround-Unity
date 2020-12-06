﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using BanGround;
using System.Security.Cryptography;
using SevenZip;

using LZMAEncoder = SevenZip.Compression.LZMA.Encoder;
using LZMADecoder = SevenZip.Compression.LZMA.Decoder;
using System.Text;
using UnityEngine;

public class ReplayFrame
{
    public int audioTime;
    public int judgeTime;

    public KirakiraTouchState[] events;
}

public class FileChecksum
{
    public string file;
    public byte[] checksum;
}

public class DemoFile
{
    const ushort KIRAREPLAY_VERSION = 2;

    public uint uid;
    public int sid;
    public Difficulty difficulty;
    public ushort mods;
    public List<FileChecksum> checksums = new List<FileChecksum>();
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

    public void Save(IFile demofile, IFile[] fileList)
    {
        var dic = new Dictionary<string, long>();

        using (var ms = new MemoryStream())
        {
            using (var br = new BinaryWriter(ms, System.Text.Encoding.UTF8, true))
            {
                // ushort version;
                br.Write(KIRAREPLAY_VERSION);

                br.Write(uid);
                br.Write(sid);
                br.Write((ushort)difficulty);
                br.Write(mods);

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

                // ReplayFrame frames[];
                foreach (var frame in frames)
                {
                    // int audioTime;
                    br.Write(frame.audioTime);
                    // char eventCount;
                    br.Write((byte)frame.events.Length);

                    // KirakiraTouchState events[];
                    foreach (var e in frame.events)
                    {
                        // short deltaTime;
                        br.Write((short)(e.time - frame.audioTime));
                        // char phase
                        br.Write((byte)e.phase);
                        // char touchId;
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
            }

            var size = ms.Position;

            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            using (var fs = demofile.Open(FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    var encoder = new LZMAEncoder();

                    // char checksum[2]; "KP"
                    bw.Write((ushort)0x504b);
                    // __int64 uncompressedSize;
                    bw.Write(size);
                    // char lzmaProp[5]
                    encoder.WriteCoderProperties(fs);

                    encoder.Code(ms, fs, size, -1, null);

                    fs.Flush();
                }
            }
        }
    }

    public static DemoFile LoadFrom(IFile demofile)
    {
        var result = new DemoFile();

        using (var ms = new MemoryStream())
        {
            using (var fs = demofile.Open(FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    // char checksum[2]; "KP"
                    int checksum = br.ReadUInt16();

                    if (checksum != 0x504b)
                        throw new InvalidDataException("This is not a kira replay file");

                    // __int64 uncompressedSize;
                    var outSize = br.ReadInt64();

                    // char lzmaProp[5]
                    var properties = br.ReadBytes(5);

                    var compressedSize = fs.Length - fs.Position;

                    var decoder = new LZMADecoder();
                    decoder.SetDecoderProperties(properties);
                    decoder.Code(fs, ms, compressedSize, outSize, null);
                }
            }

            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            using (var br = new BinaryReader(ms))
            {
                // ushort version;
                var version = br.ReadUInt16();

                if (version != KIRAREPLAY_VERSION)
                    throw new InvalidDataException("Kira replay version mismatch");

                result.uid = br.ReadUInt32();
                result.sid = br.ReadInt32();
                result.difficulty = (Difficulty)br.ReadUInt16();
                result.mods = br.ReadUInt16();

                // uint fileCount;
                var fileCount = br.ReadUInt32();

                // FileChecksum checksums[];
                for (int i = 0; i < fileCount; ++i)
                {
                    // char *fileName;
                    // read pointer, then get the null-terminated string at the pointer
                    var pointer = br.ReadInt32();
                    var curPos = ms.Position;

                    ms.Seek(pointer, SeekOrigin.Begin);

                    var byteList = new List<byte>();

                    byte ch = 0;
                    while ((ch = br.ReadByte()) != 0)
                        byteList.Add(ch);

                    ms.Seek(curPos, SeekOrigin.Begin);

                    string filename = Encoding.UTF8.GetString(byteList.ToArray());

                    // char checksum[32];
                    byte[] checksum = br.ReadBytes(32);

                    result.checksums.Add(new FileChecksum
                    {
                        file = filename,
                        checksum = checksum
                    });
                }

                // uint frameCount;
                var frameCount = br.ReadUInt32();

                // ReplayFrame frames[];
                for (int i = 0; i < frameCount; ++i)
                {
                    // int audioTime;
                    var audioTime = br.ReadInt32();
                    // char eventCount;
                    var eventCount = br.ReadByte();

                    Console.WriteLine($"Frame {i} at {audioTime}ms, contains {eventCount} events");

                    var eventList = new List<KirakiraTouchState>();

                    // KirakiraTouchState events[];
                    for (int j = 0; j < eventCount; j++)
                    {
                        // short deltaTime;
                        short deltaTime = br.ReadInt16();
                        // char phase
                        KirakiraTouchPhase phase = (KirakiraTouchPhase)br.ReadByte();
                        // char touchId
                        int touchId = br.ReadByte();

                        // vec2 pos
                        var x = br.ReadInt16() / 64.0f;
                        var y = br.ReadInt16() / 64.0f;

                        eventList.Add(new KirakiraTouchState
                        {
                            phase = phase,
                            time = audioTime + deltaTime,
                            touchId = touchId,
                            screenPos = new Vector2
                            {
                                x = x,
                                y = y
                            }
                        });
                    }

                    var frame = new ReplayFrame
                    {
                        audioTime = audioTime,
                        events = eventList.ToArray()
                    };

                    result.frames.Add(frame);
                }
            }
        }

        return result;
    }
}

public class DemoRecorder
{
    public string demoName;
    public DemoFile demoFile;

    public DemoRecorder(int chartId, Difficulty diff, ushort mods)
    {
        demoFile = new DemoFile
        {
            sid = chartId,
            difficulty = diff,
            mods = mods
        };

        demoName = $"{chartId}_{diff:g}_{DateTime.Now.ToLongDateString()}_{DateTime.Now.ToLongTimeString()}.kirareplay".Replace(":", "-").Replace("/", "-").Replace("\\", "-");
    }

    public void Add(KirakiraTouchState[] kirakiraTouchStates)
    {
        demoFile.Add(kirakiraTouchStates);
    }

    public void Save(IFile demofile, IFile[] fileList) => demoFile.Save(demofile, fileList);
}