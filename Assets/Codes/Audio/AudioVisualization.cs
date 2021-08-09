using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

class AudioVisualization : MonoBehaviour
{
    [Inject]
    IAudioManager am;

    public Image image;

    private void Start()
    {
        image = GetComponent<Image>();

        var tex = am.GetFFTTexture();
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, 1), new Vector2(1f, 1f));
    }
}
