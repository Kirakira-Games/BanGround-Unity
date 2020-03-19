using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DifficultySelect : MonoBehaviour
{
    public GameObject[] cards = new GameObject[5];
    public GameObject[] labels = new GameObject[5];
    Text[] labelText = new Text[5];
    Image[] cardImg = new Image[5];
    RectTransform[] Rects = new RectTransform[5];
    public int[] levels = new int[5];
    List<int> enabledCards = new List<int>();
    int selected = 0;
    Text difficultyText;
    Text levelText;
    SelectManager selectManager;
    private FixBackground background;


    void Start()
    {
        //levels = new int[]{ 1,1,18,26,28 };
        for (int i = 0; i < cards.Length; i++)
        {
            cardImg[i] = cards[i].GetComponent<Image>();
            Rects[i] = cards[i].GetComponent<RectTransform>();
            labelText[i] = labels[i].GetComponentInChildren<Text>();
            labelText[i].text = "";
        }
        levelText = GameObject.Find("Text_SelectedLevel").GetComponent<Text>();
        levelText.text = "";
        difficultyText = GameObject.Find("Text_SelectedDifficulty").GetComponent<Text>();
        
        selectManager = GameObject.Find("SelectManager").GetComponent<SelectManager>();
        //OnSongChange();
        background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
    }
    
    public void OnSongChange()
    {
        enabledCards = new List<int>();
        for (int i = 0; i < cards.Length; i++) //找出需要被激活的卡片
        {
            int index = (i + LiveSetting.currentDifficulty) % cards.Length;
            if (levels[index] < 0)//没有此难度的话
            {
                cards[index].SetActive(false);
                labels[index].SetActive(false);
            }
            else
            {
                enabledCards.Add(index);
                cards[index].SetActive(true);
                labels[index].SetActive(true);
                labelText[index].text = Enum.GetName(typeof(Difficulty), index).ToUpper() + " " + levels[index];
            }
        }
        UpdateView();
    }

    public void UpdateView()
    {
        for (int i = 0; i < enabledCards.Count; i++)
        {
            //Rects[enabledCards[i]].anchoredPosition = new Vector2(0, 0); //图层 位置
            Rects[enabledCards[i]].SetAsFirstSibling();
            cardImg[enabledCards[i]].color = new Color(1,1,1,0);
        }
        selected = enabledCards[0]; //最顶上一层
        cardImg[selected].color = Color.white;
        difficultyText.text = Enum.GetName(typeof(Difficulty), selected).ToUpper();
        levelText.text = levels[selected].ToString();

        LiveSetting.actualDifficulty = selected;
        selectManager.DisplayRecord();

        string path = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid);
        background.UpdateBackground(path);
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
       /* while (cardImg[no].color.a > 0)
        {
            cardImg[no].color -= new Color(0, 0, 0, 0.1f);
            yield return new WaitForEndOfFrame();
        }
        //cards[no].SetActive(false);
        */
        //cardImg[no].color = Color.clear;

        UpdateView();
        yield return new WaitForEndOfFrame();
        /*cardImg[selected].color = new Color(1, 1, 1, 0);
        while (cardImg[selected].color.a < 1)
        {
            cardImg[selected].color += new Color(0, 0, 0, 0.1f);
            yield return new WaitForEndOfFrame();
        }
        cardImg[selected].color = Color.white;*/
        LiveSetting.currentDifficulty = selected;
    }
}
