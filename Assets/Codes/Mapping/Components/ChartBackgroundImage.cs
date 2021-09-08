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
        private IResourceLoader resourceLoader;
        [Inject]
        private IChartLoader chartLoader;

        private void Start()
        {
            ReloadImage();
        }
        public void ReloadImage(bool forceReload = false)
        {
            var image = GetComponent<Image>();
            var (path, _) = dataLoader.GetBackgroundPath(chartLoader.header.sid);
            if (string.IsNullOrEmpty(path))
                return;
            var tex = resourceLoader.LoadTextureFromFs(path, forceReload);
            if (tex != null)
            {
                image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
