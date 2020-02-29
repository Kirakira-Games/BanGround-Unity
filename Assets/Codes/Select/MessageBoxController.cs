using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class MessageQueueItem
{
    public LogLevel level;
    public string content;
    public bool autoClose;
}

public class MessageBoxController : MonoBehaviour
{
    //public static MessageBoxController Instance { get; private set; }

    private Sprite[] msgSprite;
    private Button msgBtn;
    private Image msgImg;
    private LocalizedText msgText;
    private Animator animator;

    private static Queue<MessageQueueItem> msgQueue = new Queue<MessageQueueItem>();

    private void Awake()
    {
        //Instance = this;
        //msgQueue = new Queue<MessageQueueItem>();
    }

    void Start()
    {
        msgSprite = Resources.LoadAll<Sprite>("UI/MsgBox");
        animator = GetComponent<Animator>();
        msgBtn = GetComponent<Button>();
        msgImg = GetComponent<Image>();
        msgText = GetComponentInChildren<LocalizedText>();

        msgBtn.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            animator.Play("MsgOut");
        });
    }

    private void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Outed"))
        {
            if (msgQueue.Count > 0)
            {
                ShowMsg(msgQueue.Dequeue());
            }
        }
    }

    /// <summary>
    /// level: 0-info 1-ok 2-error
    /// </summary>
    /// <param name="level"></param>
    /// <param name="content"></param>
    public static void ShowMsg(LogLevel level, string content, bool autoClose = true)
    {
        msgQueue.Enqueue(new MessageQueueItem
        {
            level = level,
            content = content,
            autoClose = autoClose
        });
    }

    private void ShowMsg(MessageQueueItem item)
    {
        int level = (int)item.level;
        //Set content
        msgImg.sprite = msgSprite[level];
        msgText.text = item.content;
        msgText.Localizify(item.content);

        //Show Box
        animator.Play("MsgIn");

        //AutoClose
        if (item.autoClose) StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(3);
        animator.Play("MsgOut");
    }
}

public enum LogLevel
{
    INFO,OK,ERROR
}