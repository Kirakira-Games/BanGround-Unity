using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using BanGround;

public class KiraSongCell : FancyCell<cHeader, Context>
{
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
        mHeader song = SelectManager_old.instance.dataLoader.GetMusicHeader(chart.mid);

        smallText.text = song.title;

        string path = SelectManager_old.instance.dataLoader.GetBackgroundPath(chart.sid).Item1;
        UpdateBackground(path);
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

    public void UpdateBackground(string path)
    {
        if (path == null || !SelectManager_old.instance.fs.FileExists(path))
        {
            largeImage.sprite = null;
            return;
        }
        else
        {
            var tex = SelectManager_old.instance.fs.GetFile(path).ReadAsTexture();
            largeImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
