﻿using UnityEngine;
using System.Collections.Generic;
using V2;

public class LaneEffects : MonoBehaviour
{
    private ParticleSystem[] ps;
    private List<TimingPoint> speed;
    private float prevSpeed;
    private Quaternion[] rotation;

    public static TimingPoint GetIndexByTime(List<TimingPoint> list, float time)
    {
        int l = -1, r = list.Count - 1;
        while (r > l)
        {
            int mid = (l + r + 1) >> 1;
            if (list[mid].time >= time)
                r = mid - 1;
            else
                l = mid;
        }
        return l == -1 ? null : list[l];
    }


    public void Init(GameTimingGroup timingGroup)
    {
        speed = timingGroup.points;
    }

    public float GetSpeed(int audioTime)
    {
        return GetSpeed(audioTime / 1000f);
    }

    public float GetSpeed(float audioTimeS)
    {
        var data = GetIndexByTime(speed, audioTimeS);
        return data == null ? speed[0].speed : data.speed;
    }

    void Start()
    {
        ps = GetComponentsInChildren<ParticleSystem>();
        prevSpeed = ps[0].main.simulationSpeed;
        rotation = new Quaternion[]
        {
            Quaternion.Euler(-90, 0, 0),
            Quaternion.Euler(90, 0, 0)
        };
    }

    static KVarRef r_notespeed = new KVarRef("r_notespeed");

    public void UpdateLaneEffects(int audioTime)
    {
        float speed = GetSpeed(audioTime) * LiveSetting.SpeedCompensationSum * (r_notespeed + 1f) / 12;
        float abs = Mathf.Abs(speed);
        if (prevSpeed != speed)
        {
            foreach (var system in ps)
            {
                var main = system.main;
                main.simulationSpeed = abs;
                if (Mathf.Sign(speed) != Mathf.Sign(prevSpeed))
                {
                    Vector3 pos = system.transform.position;
                    pos.z = speed >= 0 ? transform.position.z : 0;
                    system.transform.SetPositionAndRotation(pos, rotation[speed <= 0 ? 1 : 0]);
                }
            }
            prevSpeed = speed;
        }
    }
}
