using UnityEngine;
using UnityEngine.UI;

namespace BanGround.Community
{
    public class ProgressBar : MonoBehaviour
    {
        public Image ProgressImage;
        public float Progress
        {
            get => mProgress;
            set
            {
                mProgress = value;
                ProgressImage.transform.localScale.Set(value, 1, 1);
            }
        }

        public void SetColor(Color color)
        {
            ProgressImage.color = color;
        }

        private float mProgress;

        private void Start()
        {
            Progress = 0;
        }
    }
}