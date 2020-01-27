using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class LiveSettingLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (File.Exists(LiveSetting.settingsPath)) {
            string sets = File.ReadAllText(LiveSetting.settingsPath);
            LiveSettingTemplate loaded =  JsonConvert.DeserializeObject<LiveSettingTemplate>(sets);
            LiveSettingTemplate.ApplyToLiveSetting(loaded);
        }
        else
        {
            Debug.LogWarning("Live setting file not found");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
