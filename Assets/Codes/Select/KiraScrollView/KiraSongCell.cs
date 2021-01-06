﻿using BanGround;
using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class KiraSongCell : FancyCell<int, Context>
{
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IFileSystem fs;

    [SerializeField] 
    Animator animator = default;
    [SerializeField] 
    Button button = default;
    [SerializeField]
    CanvasGroup group = default;

    [SerializeField] 
    Text smallText;
    [SerializeField] 
    Image smallImage;

    RectTransform rectTransform => transform as RectTransform;

    private int myIndex = -1;
    private int lastSid = 0;

    public static float MostCenterdCellButShiftedPosition = -1;
    public static float MostCenterdCellPosition = -1;
    public static int MostCenterdCellIndex = -1;

    // GameObject が非アクティブになると Animator がリセットされてしまうため
    // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
    float currentPosition = 0;

    static class AnimatorHash
    {
        public static readonly int Scroll = Animator.StringToHash("scroll");
    }

    void OnEnable() => UpdatePosition(currentPosition);

    void Start()
    {
        button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
    }

    public override void UpdateContent(int index)
    {
        myIndex = index;
        var chart = dataLoader.chartList[index];

        if (chart.sid == lastSid)
            return;

        lastSid = chart.sid;

        mHeader song = dataLoader.GetMusicHeader(chart.mid);
        smallText.text = song.title;
    }

    private void Update()
    {
        if (Mathf.Abs(smallImage.rectTransform.anchorMin.y - 0.5f) <= Mathf.Abs(MostCenterdCellPosition - 0.5f))
        {
            MostCenterdCellIndex = myIndex;
            MostCenterdCellButShiftedPosition = MostCenterdCellButShiftedPosition == -1 ? smallImage.rectTransform.anchorMin.y : MostCenterdCellPosition;
            MostCenterdCellPosition = smallImage.rectTransform.anchorMin.y;
        }
    }

    public override void UpdatePosition(float position)
    {
        currentPosition = position;

        if (animator.isActiveAndEnabled)
        {
            animator.Play(AnimatorHash.Scroll, -1, position);
        }

        animator.speed = 0;
    }

    public class Factory : PlaceholderFactory<KiraSongCell> { }
}
