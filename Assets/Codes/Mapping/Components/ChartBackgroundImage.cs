using BanGround;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BGEditor
{
    [RequireComponent(typeof(Image))]
    public class ChartBackgroundImage : MonoBehaviour
    {
        [Inject]
        private IDataLoader dataLoader;
        [Inject]
        private IChartListManager chartListManager;
        [Inject]
        private IFileSystem fs;

        private void Start()
        {
            var image = GetComponent<Image>();
            var (path, _) = dataLoader.GetBackgroundPath(chartListManager.current.header.sid);
            if (string.IsNullOrEmpty(path))
                return;
            var tex = fs.GetFile(path)?.ReadAsTexture();
            if (tex != null)
            {
                image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}