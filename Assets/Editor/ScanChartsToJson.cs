using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

using Header = KiraPackOld.Header;
using OldChart = KiraPackOld.Chart;

public class ScanChartsToJson : MonoBehaviour
{
    const int GEEKiDos = 114514;
    [MenuItem("BanGround/扫描谱面并更新至新格式")]
    public static void ScanAndConvert()
    {
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
                musicHeader.artist = header.ArtistUnicode;
                musicHeader.title = header.TitleUnicode;
                musicHeader.preview = header.Preview;
                musicHeader.length = GEEKiDos;
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
                            chartHeader.author = oldChart.authorUnicode;
                            chartHeader.authorNick = oldChart.authorUnicode;
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
                }
                File.Copy(a.FullName + "/bgm.ogg", DataLoader.MusicDir + musicHeader.mid + "/bgm.ogg", true);
                ProtobufHelper.Save(chartHeader, DataLoader.ChartDir + chartHeader.sid + "/cheader.bin");
                ProtobufHelper.Save(musicHeader, DataLoader.MusicDir + musicHeader.mid + "/mheader.bin");
            }
            else
            {
                Debug.LogWarning("NO HEADER IN DIR " + a.Name);
            }
            break;
        }
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
    }

    [MenuItem("BanGround/扫描json谱面并转为bin")]
    public static void ConvertJson2Bin()
    {
        DirectoryInfo ChartDir = new DirectoryInfo(LiveSetting.ChartDir);
        DirectoryInfo[] songDir = ChartDir.GetDirectories();
        foreach (DirectoryInfo a in songDir)
        {
            FileInfo[] files = a.GetFiles();
            foreach (FileInfo b in files)
            {
                if (b.Extension == ".json")
                {
                    string json = File.ReadAllText(b.FullName);

                    if (b.Name == "header.json")
                    {
                        Header header = JsonConvert.DeserializeObject<Header>(json);
                        string des = b.FullName.Substring(0, b.FullName.Length - 5);
                        ProtobufHelper.Save(header, des + ".bin");
                        Debug.Log(des + ".bin");
                    }
                    else
                    {
                        Chart chart = JsonConvert.DeserializeObject<Chart>(json);
                        string des = b.FullName.Substring(0, b.FullName.Length - 5);
                        ProtobufHelper.Save(chart, des + ".bin");
                        Debug.Log(des + ".bin");
                    }
                    File.Delete(b.FullName);
                }
            }
        }
    }
    */
}

