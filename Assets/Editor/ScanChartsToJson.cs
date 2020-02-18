using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

#pragma warning disable CS0618 // Type or member is obsolete: KiraPackOld

using Header = KiraPackOld.Header;
using OldChart = KiraPackOld.Chart;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using Newtonsoft.Json;


public class ScanChartsToJson : MonoBehaviour
{
    [MenuItem("BanGround/扫描谱面并更新至新格式")]
    public static void ScanAndConvert()
    {
        Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, default);
        DirectoryInfo OldDir = new DirectoryInfo(Application.streamingAssetsPath + "/TestCharts/");
        DirectoryInfo[] songDir = OldDir.GetDirectories();

        foreach (DirectoryInfo a in songDir)
        {
            if (File.Exists(a.FullName + "/header.bin"))
            {
                mHeader musicHeader = new mHeader();
                cHeader chartHeader = null;
                Header header = ProtobufHelper.Load<Header>(a.FullName + "/header.bin");
                Debug.Log("Scan: " + header.TitleUnicode + " in " + a.Name);

                musicHeader.mid = int.Parse(a.Name);
                musicHeader.artist = header.ArtistUnicode ?? header.Artist;
                musicHeader.title = header.TitleUnicode ?? header.Title;
                musicHeader.preview = header.Preview;
                var music = File.ReadAllBytes(a.FullName + "/bgm.ogg");
                var pinnedObject = GCHandle.Alloc(music, GCHandleType.Pinned);
                var pinnedObjectPtr = pinnedObject.AddrOfPinnedObject();

                var stream = Bass.BASS_StreamCreateFile(pinnedObjectPtr, 0, music.Length, BASSFlag.BASS_DEFAULT);
                var length = Bass.BASS_ChannelGetLength(stream);
                var time = Bass.BASS_ChannelBytes2Seconds(stream, length);
                musicHeader.length = (float)time;
                Bass.BASS_StreamFree(stream);

                if (!Directory.Exists(DataLoader.ChartDir + musicHeader.mid))
                    Directory.CreateDirectory(DataLoader.ChartDir + musicHeader.mid);
                if (!Directory.Exists(DataLoader.MusicDir + musicHeader.mid))
                    Directory.CreateDirectory(DataLoader.MusicDir + musicHeader.mid);

                FileInfo[] files = a.GetFiles();

                foreach (FileInfo b in files)
                {
                    if (b.Extension == ".bin" && b.Name != "header.bin")
                    {
                        OldChart oldChart = ProtobufHelper.Load<OldChart>(b.FullName);
                        if (chartHeader == null)
                        {
                            // Create cHeaders from old chart
                            chartHeader = new cHeader();
                            chartHeader.author = oldChart.authorUnicode ?? oldChart.author;
                            chartHeader.authorNick = chartHeader.author;
                            chartHeader.backgroundFile = new BackgroundFile
                            {
                                pic = oldChart.backgroundFile
                            };
                            chartHeader.mid = musicHeader.mid;
                            chartHeader.sid = musicHeader.mid;
                            chartHeader.preview = musicHeader.preview;
                            chartHeader.version = DataLoader.ChartVersion;
                        }

                        // Create chart from old chart
                        Chart chart = new Chart();
                        chart.Difficulty = oldChart.difficulty;
                        chart.level = oldChart.level;
                        chart.notes = oldChart.notes;
                        chart.offset = oldChart.offset;
                        ProtobufHelper.Save(chart, DataLoader.ChartDir + chartHeader.sid + "/" +
                            chart.Difficulty.ToString("G").ToLower() + ".bin");
                    }
                    else if (b.Extension != ".ogg" && b.Extension != ".bin")
                    {
                        File.Copy(b.FullName, DataLoader.ChartDir + musicHeader.mid + "/" + b.Name, true);
                    }
                }
                File.Copy(a.FullName + "/bgm.ogg", DataLoader.MusicDir + musicHeader.mid + "/bgm.ogg", true);
                ProtobufHelper.Save(chartHeader, DataLoader.ChartDir + chartHeader.sid + "/cheader.bin");
                ProtobufHelper.Save(musicHeader, DataLoader.MusicDir + musicHeader.mid + "/mheader.bin");
            }
            else
            {
                Debug.LogWarning("NO HEADER IN DIR " + a.Name);
            }
        }
        Bass.BASS_Free();
        Debug.LogWarning("Connverrt Success");
    }
    /*
    [MenuItem("BanGround/扫描谱面并写入SongList")]
    public static void ScanAndSave()
    {
        DirectoryInfo ChartDir = new DirectoryInfo(LiveSetting.ChartDir);
        DirectoryInfo[] songDir = ChartDir.GetDirectories();
        List<Header> headerList = new List<Header>();
        foreach(DirectoryInfo a in songDir)
        {
            //print(a.Name);
            
            if (File.Exists(a.FullName + "/header.bin"))
            {
                //string HeaderJson = File.ReadAllText(a.FullName + "/header.json");
                //Header header = JsonConvert.DeserializeObject<Header>(HeaderJson);
                Header header = ProtobufHelper.Load<Header>(a.FullName + "/header.bin");
                Debug.Log("Scan: " + header.TitleUnicode + " in " + a.Name);
                header.DirName = a.Name;
                header.charts = new List<Chart>();
                FileInfo[] files = a.GetFiles();
                foreach (FileInfo b in files)
                {
                    if (b.Extension == ".bin" && b.Name != "header.bin")
                    {
                        //print(b.Name);
                        //string chartJson = File.ReadAllText(b.FullName);
                        //Chart chart = JsonConvert.DeserializeObject<Chart>(chartJson);
                        Chart chart = ProtobufHelper.Load<Chart>(b.FullName);
                        chart.notes = new List<Note>();//删除note信息
                        chart.fileName = b.Name.Replace(".bin","");
                        header.charts.Add(chart);
                    }
                }
                headerList.Add(header);
            }
            else
            {
                Debug.LogWarning("NO HEADER IN DIR " + a.Name);
            }
        }
        SongList songList = new SongList(System.DateTime.Now.ToString(), headerList);
        string listJson = JsonConvert.SerializeObject(songList);
        print(listJson);
        Debug.LogWarning("Gennerrate Success");
        ProtobufHelper.Save(songList, Application.streamingAssetsPath + "/SongList.bin");
        //File.WriteAllText(Application.streamingAssetsPath + "/SongList.json", listJson);
    }*/

    private static void ConvertJsonHelper(DirectoryInfo dir)
    {
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension == ".json")
            {
                string json = File.ReadAllText(file.FullName);

                if (file.Name == "cheader.json")
                {
                    cHeader header = JsonConvert.DeserializeObject<cHeader>(json);
                    string des = file.FullName.Substring(0, file.FullName.Length - 5);
                    ProtobufHelper.Save(header, des + ".bin");
                    Debug.Log(des + ".bin");
                }
                else if (file.Name == "mheader.json")
                {
                    mHeader header = JsonConvert.DeserializeObject<mHeader>(json);
                    string des = file.FullName.Substring(0, file.FullName.Length - 5);
                    ProtobufHelper.Save(header, des + ".bin");
                    Debug.Log(des + ".bin");
                }
                else
                {
                    Chart chart = JsonConvert.DeserializeObject<Chart>(json);
                    string des = file.FullName.Substring(0, file.FullName.Length - 5);
                    ProtobufHelper.Save(chart, des + ".bin");
                    Debug.Log(des + ".bin");
                }
                File.Delete(file.FullName);
            }
        }
    }

    [MenuItem("BanGround/扫描json谱面并转为bin")]
    public static void ConvertJson2Bin()
    {
        DirectoryInfo ChartDir = new DirectoryInfo(DataLoader.ChartDir);
        DirectoryInfo[] charts = ChartDir.GetDirectories();
        foreach (DirectoryInfo chart in charts)
        {
            ConvertJsonHelper(chart);
        }

        DirectoryInfo MusicDir = new DirectoryInfo(DataLoader.MusicDir);
        DirectoryInfo[] songs = MusicDir.GetDirectories();
        foreach (DirectoryInfo song in songs)
        {
            ConvertJsonHelper(song);
        }
    }
}

