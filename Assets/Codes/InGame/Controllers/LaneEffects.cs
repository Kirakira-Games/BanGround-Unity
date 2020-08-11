using UnityEngine;
using System.Collections.Generic;
using V2;
using Zenject;

public class LaneEffects : MonoBehaviour
{
    [Inject]
    private ILiveSetting liveSetting;

    private ParticleSystem[] ps;
    private List<TimingPoint> speed;
    private float prevSpeed;
    private Quaternion[] rotation;

    static KVarRef effect = new KVarRef("r_showeffect");

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
        if (!effect)
        {
            foreach (var particle in ps) 
                particle.gameObject.SetActive(false);

            return;
        }
        prevSpeed = ps[0].main.simulationSpeed;
        rotation = new Quaternion[]
        {
            Quaternion.Euler(-90, 0, 0),
            Quaternion.Euler(90, 0, 0)
        };
    }

    static KVarRef r_notespeed = new KVarRef("r_notespeed");

    public void UpdateLaneEffects()
    {
        if (!effect)
            return;

        float speed = GetSpeed(NoteController.audioTimef) * liveSetting.SpeedCompensationSum * (r_notespeed + 4f) / 14;
        if (UIManager.Instance.SM.isRewinding)
        {
            speed = -speed;
        }
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
