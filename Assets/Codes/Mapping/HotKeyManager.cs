using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Linq;
using Zenject;
using UnityEngine.InputSystem.Controls;

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

        private static Keyboard currentKeyboard;

        void Awake()
        {
            mHotKeys = new List<HotKeyCombo>(HotKeys);
            // Add loading blocker
            var allBlockers = blockers.ToList();
            allBlockers.Add(loadingBlocker.gameObject);
            blockers = allBlockers.ToArray();

            currentKeyboard = Keyboard.current;
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
                    KeyControl keyControl = (KeyControl)Keyboard.current[str];
                    if (!keyControl.isPressed)
                        return false;
                    hasDown |= keyControl.wasPressedThisFrame;
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

        static bool GetKey(Key key)
        {
            return currentKeyboard[key].isPressed;
        }

        static bool GetKeyDown(Key key)
        {
            return currentKeyboard[key].wasPressedThisFrame;
        }

        static bool GetKeyUp(Key key)
        {
            return currentKeyboard[key].wasReleasedThisFrame;
        }

        void Update()
        {
            isCtrl = GetKey(Key.LeftCtrl) || GetKey(Key.RightCtrl);
            isShift = GetKey(Key.LeftShift) || GetKey(Key.RightShift);
            isCtrlDown = GetKeyDown(Key.LeftCtrl) || GetKeyDown(Key.RightCtrl);
            isShiftDown = GetKeyDown(Key.LeftShift) || GetKeyDown(Key.RightShift);
            isCtrlUp = GetKeyUp(Key.LeftCtrl) || GetKeyUp(Key.RightCtrl);
            isShiftUp = GetKeyUp(Key.LeftShift) || GetKeyUp(Key.RightShift);
            if (blockers.Any(blocker => blocker.activeInHierarchy))
                return;

            foreach (var combo in mHotKeys)
            {
                if (Activated(combo.Keys))
                    combo.OnCombo.Invoke();
            }
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (!Mathf.Approximately(0, scroll))
            {
                onScroll.Invoke(scroll);
            }
        }
    }
}