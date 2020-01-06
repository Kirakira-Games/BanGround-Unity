using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static int maxCombo;
    public static ComboManager manager;

    private int combo;

    void Start()
    {
        maxCombo = combo = 0;
        manager = this;
    }

    public void UpdateCombo(JudgeResult result)
    {
        if (result >= JudgeResult.Bad)
        {
            combo = 0;
        }
        else
        {
            combo++;
            if (combo > maxCombo)
            {
                maxCombo = combo;
            }
        }
        print("COMBO: " + combo);
    }

    void Update()
    {
        // TODO
    }
}
