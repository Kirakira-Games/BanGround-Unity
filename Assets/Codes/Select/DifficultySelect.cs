using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

#pragma warning disable 0649
public class DifficultySelect : MonoBehaviour
{
    public static readonly string[] DIFFICULTY_ABBR = { "EZ", "NM", "HD", "EX", "SP" };
    public GameObject[] cards = new GameObject[5];
    public GameObject[] labels = new GameObject[5];
    Text[] labelText = new Text[5];
    Image[] cardImg = new Image[5];
    RectTransform[] Rects = new RectTransform[5];
    public int[] levels = new int[5];

    [SerializeField] 
    private TextAsset[] voices;
    AudioProvider.ISoundEffect[] processedVoices = new AudioProvider.ISoundEffect[5];


    List<int> enabledCards = new List<int>();
    int selected = 0;
    Text difficultyText;
    Text levelText;
    PlayRecordDisplay recordDisplayer;
    private FixBackground background;

    static KVarRef cl_cursorter = new KVarRef("cl_cursorter");

    void Start()
    {
        //levels = new int[]{ 1,1,18,26,28 };
        for (int i = 0; i < cards.Length; i++)
        {
            cardImg[i] = cards[i].GetComponent<Image>();
            Rects[i] = cards[i].GetComponent<RectTransform>();
            labelText[i] = labels[i].GetComponentInChildren<Text>();
            labelText[i].text = "";
            processedVoices[i] = AudioManager.Instance.PrecacheSE(voices[i].bytes);
        }
        levelText = GameObject.Find("Text_SelectedLevel").GetComponent<Text>();
        levelText.text = "";
        difficultyText = GameObject.Find("Text_SelectedDifficulty").GetComponent<Text>();
        recordDisplayer = GameObject.Find("Left_Panel").GetComponent<PlayRecordDisplay>();
        background = GameObject.Find("KirakiraBackground").GetComponent<FixBackground>();
        lastDifficulty = LiveSetting.actualDifficulty;
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
                labelText[index].text = DIFFICULTY_ABBR[index] + " " + levels[index];
            }
        }
        UpdateView();
    }

    int lastDifficulty;
    //This Called both change button clicked and song changed
    public void UpdateView()
    {
        if(enabledCards.Count == 0)
            return;

        for (int i = 0; i < enabledCards.Count; i++)
        {
            //Rects[enabledCards[i]].anchoredPosition = new Vector2(0, 0); //图层 位置
            Rects[enabledCards[i]].SetAsFirstSibling();
            cardImg[enabledCards[i]].color = new Color(1, 1, 1, 0);
        }
        selected = enabledCards[0]; //最顶上一层
        cardImg[selected].color = Color.white;
        difficultyText.text = Enum.GetName(typeof(Difficulty), selected).ToUpper();
        levelText.text = levels[selected].ToString();

        LiveSetting.actualDifficulty = selected;
        LiveSetting.currentDifficulty = selected;
        if ((Sorter)cl_cursorter == Sorter.ChartDifficulty && lastDifficulty != LiveSetting.actualDifficulty)
            SelectManager.instance.InitSongList(LiveSetting.CurrentHeader.sid);

        lastDifficulty = LiveSetting.actualDifficulty;

        recordDisplayer.DisplayRecord();
        string path = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid).Item1;
        background.UpdateBackground(path);
        //LiveSetting.selectedDifficulty = (Difficulty)enabledCards[0];
        //print(Enum.GetName(typeof(Difficulty), LiveSetting.selectedDifficulty));
    }

    //This called on the change button clicked
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

        processedVoices[selected].PlayOneShot();
    }

}
