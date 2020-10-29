using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MainBlocker : MonoBehaviour
{
    public static MainBlocker Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetBlock(bool enable)
    {
        gameObject.SetActive(enable);
    }
}