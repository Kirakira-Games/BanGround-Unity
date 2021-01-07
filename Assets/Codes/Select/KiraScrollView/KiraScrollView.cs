using Cysharp.Threading.Tasks;
using EasingCore;
using FancyScrollView;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class KiraScrollView : FancyScrollView<int, Context>
{
    [SerializeField] Scroller scroller = default;
    [SerializeField] GameObject cellPrefab = default;

    [Inject]
    private IChartListManager chartListManager;
    [Inject]
    private KiraSongCell.Factory cellFactory;
    [Inject]
    private SelectManager selectManager;

    public bool Moved = false;
    public Action OnMove;

    protected override GameObject CellPrefab => cellPrefab;

    public int SelectedCellIndex => Context.SelectedIndex;

    protected override void Initialize()
    {
        base.Initialize();

        Context.OnCellClicked = SelectCell;

        scroller.OnValueChanged(UpdatePosition);
        scroller.OnSelectionChanged(UpdateSelection);
    }

    private void Update()
    {
        if(Moved)
        {
            Moved = false;
            OnMove?.Invoke();
        }
    }

    protected override void ResizePool(float firstPosition)
    {
        Debug.Assert(CellPrefab != null);
        Debug.Assert(cellContainer != null);

        var addCount = Mathf.CeilToInt((1f - firstPosition) / cellInterval) - pool.Count;
        for (var i = 0; i < addCount; i++)
        {
            var cell = cellFactory.Create();
            cell.transform.parent = cellContainer;

            if (cell == null)
            {
                throw new MissingComponentException(string.Format(
                    "FancyCell<{0}, {1}> component not found in {2}.",
                    typeof(cHeader).FullName, typeof(Context).FullName, CellPrefab.name));
            }

            cell.SetContext(Context);
            cell.Initialize();
            cell.SetVisible(false);
            pool.Add(cell);
        }
    }

    void UpdateSelection(int index)
    {
        if (Context.SelectedIndex == index)
        {
            return;
        }

        Context.SelectedIndex = index;
        selectManager.SelectSong(index);
        Refresh();
    }

    public async void UpdateData(IList<int> items)
    {
        UpdateContents(items);
        scroller.SetTotalCount(items.Count);

        // fix background not correct on first frame.
        await UniTask.DelayFrame(1);
        scroller.ScrollTo(chartListManager.current.index, 0);
    }

    public void SelectCell(int index)
    {
        if(index == Context.SelectedIndex)
        {
            selectManager.OnEnterPressed();
        }
        else if (index < 0 || index >= ItemsSource.Count)
        {
            return;
        }

        UpdateSelection(index);
        scroller.ScrollTo(index, 0.35f, Ease.OutCubic);
    }
}
