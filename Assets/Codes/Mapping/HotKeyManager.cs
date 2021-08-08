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
        public Key[] Combo;

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
        public static bool isCtrlUp => currentKeyboard.ctrlKey.wasReleasedThisFrame;
        public static bool isShiftUp => currentKeyboard.shiftKey.wasReleasedThisFrame;
        public static bool isCtrlDown => currentKeyboard.ctrlKey.wasPressedThisFrame;
        public static bool isShiftDown => currentKeyboard.shiftKey.wasPressedThisFrame;
        public static bool isCtrl => currentKeyboard.ctrlKey.isPressed;
        public static bool isShift => currentKeyboard.shiftKey.isPressed;

        private static Keyboard currentKeyboard => Keyboard.current;

        void Awake()
        {
            mHotKeys = new List<HotKeyCombo>(HotKeys);
            // Add loading blocker
            var allBlockers = blockers.ToList();
            allBlockers.Add(loadingBlocker.gameObject);
            blockers = allBlockers.ToArray();
        }

        private HotKeyCombo Find(Key[] keys)
        {
            return mHotKeys.Find(x => Enumerable.SequenceEqual(x.Combo, keys));
        }

        public static bool Activated(Key[] keys)
        {
            bool hasDown = false;
            bool hasCtrl = false;
            bool hasShift = false;
            foreach (var key in keys)
            {
                if (key == Key.LeftCtrl || key == Key.RightCtrl)
                {
                    hasCtrl = true;
                    if (!isCtrl)
                        return false;
                    hasDown |= isCtrlDown;
                }
                else if (key == Key.LeftShift || key == Key.RightShift)
                {
                    hasShift = true;
                    if (!isShift)
                        return false;
                    hasDown |= isShiftDown;
                }
                else
                {
                    KeyControl keyControl = Keyboard.current[key];
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

        public void AddHotKey(Key[] keys, UnityAction action)
        {
            var combo = Find(keys);
            if (combo == null)
            {
                combo = new HotKeyCombo
                {
                    Combo = keys,
                    OnCombo = new UnityEvent()
                };
                mHotKeys.Add(combo);
            }
            combo.OnCombo.AddListener(action);
        }

        public void RemoveHotKey(Key[] keys, UnityAction action)
        {
            var combo = Find(keys);
            if (combo == null)
                return;
            combo.OnCombo.RemoveListener(action);
        }

        void Update()
        {
            if (blockers.Any(blocker => blocker.activeInHierarchy))
                return;

            if (currentKeyboard != null)
            {
                foreach (var combo in mHotKeys)
                {
                    if (Activated(combo.Combo))
                        combo.OnCombo.Invoke();
                }
            }

            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (!NoteUtility.Approximately(0, scroll))
                {
                    onScroll.Invoke(scroll);
                }
            }
        }
    }
}