using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLifeGraph : MonoBehaviour
{
    public Color color;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DrawLines());
    }
    
    IEnumerator DrawLines()
    {
        yield return new WaitForSeconds(2f);
        for(int i = 0;i<LifeController.lifePerSecond.Count;i++)
        {
            System.Type[] compoints = new System.Type[] { typeof(RectTransform), typeof(Image) };
            GameObject gmobj = Instantiate(new GameObject(i + " value", compoints),transform);
            gmobj.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 2 * LifeController.lifePerSecond[i]);
            gmobj.GetComponent<Image>().color = color;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
