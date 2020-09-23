using BanGround;
using UnityEngine;
using Zenject;

public class FixBackground : MonoBehaviour
{
    protected SpriteRenderer render;
    protected Sprite defaultSprite;
    protected Camera mainCam;
    protected Vector2 camSize;

    float camHeight;
    float camWidth;

    protected virtual void Start()
    {
        /*
            Field of View / 2 == arctan(camheight / (2 * distance))
         */

        mainCam = Camera.main;
        camHeight = Mathf.Tan(mainCam.fieldOfView / 2 * Mathf.Deg2Rad) * 2 * 10.35f;
        camWidth = mainCam.aspect * camHeight;
        //camSize = new Vector2(mainCam.aspect * camHeight, camHeight);

        render = GetComponent<SpriteRenderer>();
        defaultSprite = render.sprite;

        UpdateScale();
    }

    protected virtual void UpdateScale()
    {
        Vector2 spriteSize = render.sprite.bounds.size;
        float scale = Mathf.Max(camWidth / spriteSize.x, camHeight / spriteSize.y);
        //transform.localScale = new Vector3(camSize.x / spriteSize.x, camSize.y / spriteSize.y, 1);
        transform.localScale = Vector3.one * scale;
    }

    [Inject]
    IFileSystem fs;

    public void UpdateBackground(string path)
    {
        if (path == null || !fs.FileExists(path))
        {
            render.sprite = defaultSprite;
            UpdateScale();
            return;
        }

        GetAndSetBG(path);
    }

    protected void GetAndSetBG(string path)
    {
        var tex = fs.GetFile(path).ReadAsTexture();
        render.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        UpdateScale();
    }
}
