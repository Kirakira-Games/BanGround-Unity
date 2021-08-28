using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Difficulty = V2.Difficulty;

#pragma warning disable 0649
public class DifficultySelect : MonoBehaviour
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private RankTable rankTable;

    [Inject(Id = "cl_cursorter")]
    private KVar cl_cursorter;
    [Inject(Id = "cl_lastdiff")]
    private KVar cl_lastdiff;

    public static readonly string[] DIFFICULTY_ABBR = { "", "", "", "", "" };
    public GameObject[] cards = new GameObject[5];
    public GameObject[] labels = new GameObject[5];
    private Text[] labelText = new Text[5];
    private Image[] cardImg = new Image[5];
    private RectTransform[] Rects = new RectTransform[5];
    private List<int> levels => chartListManager.current.header.difficultyLevel;

    [SerializeField] 
    TextAsset[] voices;
    private AudioProvider.ISoundEffect[] processedVoices = new AudioProvider.ISoundEffect[5];


    private List<int> enabledCards = new List<int>();
    private int selected => enabledCards[0];
    private Text difficultyText;
    private Text levelText;
    private PlayRecordDisplay recordDisplayer;

    async void Start()
    {
        //levels = new int[]{ 1,1,18,26,28 };
        levelText = GameObject.Find("Text_SelectedLevel").GetComponent<Text>();
        levelText.text = "";
        difficultyText = GameObject.Find("Text_SelectedDifficulty").GetComponent<Text>();
        recordDisplayer = GameObject.Find("Left_Panel").GetComponent<PlayRecordDisplay>();

        for (int i = 0; i < cards.Length; i++)
        {
            cardImg[i] = cards[i].GetComponent<Image>();
            Rects[i] = cards[i].GetComponent<RectTransform>();
            labelText[i] = labels[i].GetComponentInChildren<Text>();
            labelText[i].text = "";
            processedVoices[i] = await audioManager.PrecacheSE(voices[i].bytes);
        }

        // Register listeners
        chartListManager.onChartListUpdated.AddListener(OnSongChange);
        chartListManager.onSelectedChartUpdated.AddListener(OnSongChange);
        chartListManager.onDifficultyUpdated.AddListener(UpdateView);
    }

    #region View
    public void OnSongChange()
    {
        enabledCards = new List<int>();
        for (int i = 0; i < cards.Length; i++) //找出需要被激活的卡片
        {
            int index = (i + cl_lastdiff) % cards.Length;
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
        cardImg[selected].color = Color.white;
        difficultyText.text = Enum.GetName(typeof(Difficulty), selected).ToUpper();
        levelText.text = levels[selected].ToString();

        rankTable.UpdateCurrentChart(chartListManager.current.header.sid, chartListManager.current.difficulty);
        recordDisplayer.DisplayRecord();
    }
    #endregion

    #region Controller
    //This called on the change button clicked
    public void OnSwipeNext()
    {
        if (enabledCards.Count <= 1)
            return;

        var firstCard = enabledCards[0];
        enabledCards.RemoveAt(0);
        enabledCards.Add(firstCard);

        chartListManager.SelectDifficulty((Difficulty)selected);

        if ((Sorter)cl_cursorter == Sorter.ChartDifficulty)
        {
            chartListManager.SortChart();
            return;
        }

        StartCoroutine(SwipeOutAnimation());
    }

    IEnumerator SwipeOutAnimation()
    {
        yield return new WaitForEndOfFrame();

        processedVoices[selected].PlayOneShot();
    }
    #endregion

    private void OnDestroy()
    {
        chartListManager.onChartListUpdated.RemoveListener(OnSongChange);
        chartListManager.onSelectedChartUpdated.RemoveListener(OnSongChange);
        chartListManager.onDifficultyUpdated.RemoveListener(UpdateView);
    }
}
