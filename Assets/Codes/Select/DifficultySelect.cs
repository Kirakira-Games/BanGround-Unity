using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelect : MonoBehaviour
{
    public GameObject[] cards = new GameObject[5];
    Image[] cardImg = new Image[5];
    RectTransform[] Rects = new RectTransform[5];
    public int[] levels = new int[5];
    List<int> enabledCards = new List<int>();
    int selected =0;
    Text difficultyText;
    Text levelText;
    SelectManager selectManager;

    void Start()
    {
        //levels = new int[]{ 1,1,18,26,28 };
        for (int i = 0; i < cards.Length; i++)
        {
            cardImg[i] = cards[i].GetComponent<Image>();
            Rects[i] = cards[i].GetComponent<RectTransform>();
        }
        levelText = GameObject.Find("Text_SelectedLevel").GetComponent<Text>();
        difficultyText = GameObject.Find("Text_SelectedDifficulty").GetComponent<Text>();
        selectManager = GameObject.Find("SelectManager").GetComponent<SelectManager>();
        //OnSongChange();
    }
    
    public void OnSongChange()
    {
        enabledCards = new List<int>();
        for (int i = 0; i < cards.Length; i++) //找出需要被激活的卡片
        {
            if (levels[i] < 0)
            {
                cards[i].SetActive(false);
            }
            else
            {
                enabledCards.Add(i);
                cards[i].SetActive(true);
            }
        }
        UpdateView();
    }

    public void UpdateView()
    {
        for (int i = 0; i < enabledCards.Count; i++)
        {
            Rects[enabledCards[i]].anchoredPosition = new Vector2(0 - (i * 10), 0); //图层 位置
            Rects[enabledCards[i]].SetAsFirstSibling();
        }
        selected = enabledCards[0]; //最顶上一层
        difficultyText.text = Enum.GetName(typeof(Difficulty), selected).ToUpper();
        levelText.text = levels[selected].ToString();

        foreach (Chart a in selectManager.songList[LiveSetting.selectedIndex].charts) {
            if ((int)a.difficulty == selected)
                LiveSetting.selectedChart = a.fileName; //更新选择的难度的文件名
        }
        selectManager.DisplayRecord();
        //LiveSetting.selectedDifficulty = (Difficulty)enabledCards[0];
        //print(Enum.GetName(typeof(Difficulty), LiveSetting.selectedDifficulty));
    }

    public void OnSwipeNext()
    {
        if (enabledCards.Count <= 1)
            return;
        List<int> oldList = enabledCards;
        enabledCards = new List<int>();
        for (int i = 1; i < oldList.Count; i++)
        {
            enabledCards.Add(oldList[i]);
        }
        enabledCards.Add(oldList[0]);

        StartCoroutine(SwipeOutAnimation());
    }
    IEnumerator SwipeOutAnimation()
    {
        int last = enabledCards.Count - 1;
        int no = enabledCards[last];
        while (Rects[no].anchoredPosition.x < 100)
        {
            Rects[no].anchoredPosition += new Vector2(10, 0);
            cardImg[no].color -= new Color(0, 0, 0, 0.1f);
            for(int i = 0; i < enabledCards.Count; i++)
            {
                if (i != last)
                {
                    Rects[enabledCards[i]].anchoredPosition += new Vector2(1, 0);
                }
            }
            yield return new WaitForEndOfFrame();
        }
        UpdateView();
        float destPosition = Rects[no].anchoredPosition.x;
        Rects[no].anchoredPosition -= new Vector2(100, 0);
        while (Rects[no].anchoredPosition.x < destPosition)
        {
            Rects[no].anchoredPosition += new Vector2(10, 0);
            cardImg[no].color += new Color(0, 0, 0, 0.1f);
            yield return new WaitForEndOfFrame();
        }
        for(int i = 0; i < enabledCards.Count; i++)
            cardImg[enabledCards[i]].color = new Color(1, 1, 1, 1);
    }
}
