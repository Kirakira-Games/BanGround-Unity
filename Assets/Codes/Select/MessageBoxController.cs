using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxController : MonoBehaviour
{
    public static MessageBoxController Instance { get; private set; }

    private Sprite[] msgSprite;
    private Button msgBtn;
    private Image msgImg;
    private LocalizedText msgText;
    private Animator animator;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        msgSprite = Resources.LoadAll<Sprite>("V2Assets/MsgBox");
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

    /// <summary>
    /// level: 0-info 1-ok 2-error
    /// </summary>
    /// <param name="level"></param>
    /// <param name="content"></param>
    public void ShowMsg(int level, string content, bool autoClose = true)
    {
        //Set content
        msgImg.sprite = msgSprite[level];
        msgText.text = content;
        msgText.Localizify(content);

        //Show Box
        animator.Play("MsgIn");

        //AutoClose
        if (autoClose) StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(3);
        animator.Play("MsgOut");
    }
}
