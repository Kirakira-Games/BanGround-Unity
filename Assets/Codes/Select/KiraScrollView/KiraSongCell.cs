using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class KiraSongCell : FancyCell<cHeader, Context>
{
    // null?
    [Inject]
    private IDataLoader dataLoader;

    [SerializeField] Animator animator = default;
    [SerializeField] Button button = default;

    [SerializeField] Text smallText;
    [SerializeField] Image smallImage;
    [SerializeField] Image largeImage;

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

    public override void UpdateContent(cHeader chart)
    {
        mHeader song = dataLoader.GetMusicHeader(chart.mid);

        smallText.text = song.title;
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

    public class Factory : PlaceholderFactory<KiraSongCell> {}
}
