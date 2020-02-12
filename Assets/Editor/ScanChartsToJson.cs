using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;


public class ScanChartsToJson : MonoBehaviour
{
    [MenuItem("BanGround/扫描谱面并写入Json")]
    public static void ScanAndSave()
    {
        DirectoryInfo ChartDir = new DirectoryInfo(LiveSetting.ChartDir);
        DirectoryInfo[] songDir = ChartDir.GetDirectories();
        List<Header> headerList = new List<Header>();
        foreach(DirectoryInfo a in songDir)
        {
            //print(a.Name);
            
            if (File.Exists(a.FullName + "/header.json"))
            {
                string HeaderJson = File.ReadAllText(a.FullName + "/header.json");
                Header header = JsonConvert.DeserializeObject<Header>(HeaderJson);
                Debug.Log("Scan: " + header.TitleUnicode + " in " + a.Name);
                header.DirName = a.Name;
                header.charts = new List<Chart>();
                FileInfo[] files = a.GetFiles();
                foreach (FileInfo b in files)
                {
                    if (b.Extension == ".json" && b.Name != "header.json")
                    {
                        //print(b.Name);
                        string chartJson = File.ReadAllText(b.FullName);
                        Chart chart = JsonConvert.DeserializeObject<Chart>(chartJson);
                        chart.notes = new List<Note>();//删除note信息
                        chart.fileName = b.Name.Replace(".json","");
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
}

