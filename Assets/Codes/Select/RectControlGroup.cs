using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Zenject;

[RequireComponent(typeof(ScrollRect))]
public class RectControlGroup : MonoBehaviour
{
    public RectTransform content;

    [Header("Prefab")]
    public GameObject songItemPrefab;

    [Inject]
    private DiContainer _container;
    [Inject]
    private IChartListManager chartList;

    private float scrollHeight;

    public List<RectTransform> rects { get; private set; } = new List<RectTransform>();

    public RectTransform Create()
    {
        var obj = _container.InstantiatePrefab(songItemPrefab, content.transform);
        var trans = obj.GetComponent<RectTransform>();
        rects.Add(trans);
        return trans;
    }

    public void Destroy(RectTransform obj)
    {
        rects.Remove(obj);
        Destroy(obj.gameObject);
    }

    private static bool Intersect(float y1min, float y1max, float y2min, float y2max)
    {
        return y1max >= y2min && y2max >= y1min;
    }

    private void Start()
    {
        scrollHeight = GetComponent<RectTransform>().rect.height;
    }

    private void LateUpdate()
    {
        float ymin = content.localPosition.y;
        float ymax = ymin + scrollHeight;
        int minIndex = rects.Count;
        int maxIndex = 0;
        
        for (int i = 0; i < rects.Count; i++)
        {
            var rect = rects[i];
            bool visible = chartList.current.index == i || Intersect(ymin, ymax, -rect.offsetMax.y, -rect.offsetMin.y);
            if (visible)
            {
                minIndex = Mathf.Min(minIndex, i);
                maxIndex = Mathf.Max(maxIndex, i);
            }
        }

        for (int i = 0; i < rects.Count; i++)
        {
            var rect = rects[i];
            bool visible = i >= minIndex && i <= maxIndex;
            if (visible != rect.gameObject.activeSelf)
            {
                rect.gameObject.SetActive(visible);
            }
        }
    }
}
