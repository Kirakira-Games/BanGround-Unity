using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Linq;
using Zenject;

namespace BGEditor
{
    [Serializable]
    public class HotKeyCombo
    {
        public string Keys;
        public UnityEvent OnCombo;
    }

    public class MouseScrollEvent : UnityEvent<float> { }

    public class HotKeyManager : MonoBehaviour
    {
        [Inject]
        private ILoadingBlocker loadingBlocker;

        public MouseScrollEvent onScroll = new MouseScrollEvent();

        public HotKeyCombo[] HotKeys;

        public GameObject[] blockers;

        private List<HotKeyCombo> mHotKeys;
        public static bool isCtrlUp { get; private set; }
        public static bool isShiftUp { get; private set; }
        public static bool isCtrlDown { get; private set; }
        public static bool isShiftDown { get; private set; }
        public static bool isCtrl { get; private set; }
        public static bool isShift { get; private set; }

        void Awake()
        {
            mHotKeys = new List<HotKeyCombo>(HotKeys);
            // Add loading blocker
            var allBlockers = blockers.ToList();
            allBlockers.Add(loadingBlocker.gameObject);
            blockers = allBlockers.ToArray();
        }

        private HotKeyCombo Find(string keys)
        {
            keys = keys.ToLower();
            return mHotKeys.Find(x => x.Keys.ToLower() == keys);
        }

        public static bool Activated(string keys)
        {
            var keyarray = keys.Split('+');
            bool hasDown = false;
            bool hasCtrl = false;
            bool hasShift = false;
            foreach (var key in keyarray)
            {
                var str = key.ToLower();
                if (str == "ctrl")
                {
                    hasCtrl = true;
                    if (!isCtrl)
                        return false;
                    hasDown |= isCtrlDown;
                }
                else if (str == "shift")
                {
                    hasShift = true;
                    if (!isShift)
                        return false;
                    hasDown |= isShiftDown;
                }
                else
                {
                    if (!Input.GetKey(str))
                        return false;
                    hasDown |= Input.GetKeyDown(str);
                }
            }
            if (!hasDown)
                return false;
            if (hasShift != isShift || hasCtrl != isCtrl)
                return false;
            return true;
        }

        public void AddHotKey(string keys, UnityAction action)
        {
            var combo = Find(keys);
            if (combo == null)
            {
                combo = new HotKeyCombo
                {
                    Keys = keys,
                    OnCombo = new UnityEvent()
                };
                mHotKeys.Add(combo);
            }
            combo.OnCombo.AddListener(action);
        }

        public void RemoveHotKey(string keys, UnityAction action)
        {
            var combo = Find(keys);
            if (combo == null)
                return;
            combo.OnCombo.RemoveListener(action);
        }

        void Update()
        {
            isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            isCtrlDown = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
            isShiftDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            isCtrlUp = Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl);
            isShiftUp = Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift);
            if (blockers.Any(blocker => blocker.activeInHierarchy))
                return;

            foreach (var combo in mHotKeys)
            {
                if (Activated(combo.Keys))
                    combo.OnCombo.Invoke();
            }
            float scroll = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(0, scroll))
            {
                onScroll.Invoke(scroll);
            }
        }
    }
}