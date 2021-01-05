using EasingCore;
using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KiraScrollView : FancyScrollView<cHeader, Context>
{
    [SerializeField] Scroller scroller = default;
    [SerializeField] GameObject cellPrefab = default;

    protected override GameObject CellPrefab => cellPrefab;

    protected override void Initialize()
    {
        base.Initialize();

        Context.OnCellClicked = SelectCell;

        scroller.OnValueChanged(UpdatePosition);
        scroller.OnSelectionChanged(UpdateSelection);
    }

    void UpdateSelection(int index)
    {
        if (Context.SelectedIndex == index)
        {
            return;
        }

        SelectManager_old.instance.chartListManager.SelectChartByIndex(index);
        Context.SelectedIndex = index;
        Refresh();
    }

    public void UpdateData(IList<cHeader> items)
    {
        UpdateContents(items);
        scroller.SetTotalCount(items.Count);
    }

    public void SelectCell(int index)
    {
        if (index < 0 || index >= ItemsSource.Count)
        {
            Debug.LogWarning("Selection out of boundary!");
            return;
        }

        if(index == Context.SelectedIndex)
        {
            Debug.Log("Enter Ingame Scene");
            //SelectManager_old.instance.OnEnterPressed();
            return;
        }

        UpdateSelection(index);
        scroller.ScrollTo(index, 0.35f, Ease.OutCubic);
    }
}
