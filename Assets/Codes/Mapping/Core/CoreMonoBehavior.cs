using UnityEngine;

namespace BGEditor
{
    public class CoreMonoBehavior : MonoBehaviour
    {
        private ChartCore mCore;
        public ChartCore Core => mCore == null ? mCore = ChartCore.Instance : mCore;
    }
}
