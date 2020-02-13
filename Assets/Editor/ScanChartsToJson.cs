using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;


public class ScanChartsToJson : MonoBehaviour
{
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
}

