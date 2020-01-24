#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

////TODO: add the ability for the debugger to just generate input on the device according to the controls it finds; good for testing

////TODO: add commands to event trace (also clickable)

////TODO: add diff-to-previous-event ability to event window

////FIXME: doesn't survive domain reload correctly

////FIXME: the repaint triggered from IInputStateCallbackReceiver somehow comes with a significant delay

////TODO: Add "Remote:" field in list that also has a button for local devices that allows to mirror them and their input
////      into connected players

////TODO: this window should help diagnose problems in the event stream (e.g. ignored state events and why they were ignored)

////TODO: add toggle to that switches to displaying raw control values

////TODO: allow adding visualizers (or automatically add them in cases) to control that show value over time (using InputStateHistory)

////TODO: show default states of controls

////TODO: provide ability to save and load event traces; also ability to record directly to a file
////TODO: provide ability to scrub back and forth through history

namespace UnityEngine.InputSystem.Editor
{
    // Shows status and activity of a single input device in a separate window.
    // Can also be used to alter the state of a device by making up state events.
    internal sealed class InputDeviceDebuggerWindow : EditorWindow, ISerializationCallbackReceiver, IDisposable
    {
        internal const int kMaxNumEventsInTrace = 64;

        internal static InlinedArray<Action<InputDevice>> s_OnToolbarGUIActions;

        public static event Action<InputDevice> onToolbarGUI
        {
            add => s_OnToolbarGUIActions.Append(value);
            remove => s_OnToolbarGUIActions.Remove(value);
        }

        public static void CreateOrShowExisting(InputDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            // See if we have an existing window for the device and if so pop it
            // in front.
            if (s_OpenDebuggerWindows != null)
            {
                for (var i = 0; i < s_OpenDebuggerWindows.Count; ++i)
                {
                    var existingWindow = s_OpenDebuggerWindows[i];
                    if (existingWindow.m_DeviceId == device.deviceId)
                    {
                        existingWindow.Show();
                        existingWindow.Focus();
                        return;
                    }
                }
            }

            // No, so create a new one.
            var window = CreateInstance<InputDeviceDebuggerWindow>();
            window.InitializeWith(device);
            window.minSize = new Vector2(270, 300);
            window.Show();
            window.titleContent = new GUIContent(device.name);
        }

        internal void OnDestroy()
        {
            if (m_Device != null)
            {
                RemoveFromList();

                m_EventTrace?.Dispose();
                m_EventTrace = null;

                InputSystem.onDeviceChange -= OnDeviceChange;
                InputState.onChange -= OnDeviceStateChange;
            }
        }

        public void Dispose()
        {
            m_EventTrace?.Dispose();
        }

        internal void OnGUI()
        {
            // Find device again if we've gone through a domain reload.
            if (m_Device == null)
            {
                m_Device = InputSystem.GetDeviceById(m_DeviceId);

                if (m_Device == null)
                {
                    EditorGUILayout.HelpBox(Styles.notFoundHelpText, MessageType.Warning);
                    return;
                }

                InitializeWith(m_Device);
            }

            ////FIXME: with ExpandHeight(false), editor still expands height for some reason....
            EditorGUILayout.BeginVertical("OL Box", GUILayout.Height(170));// GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("Name", m_Device.name);
            EditorGUILayout.LabelField("Layout", m_Device.layout);
            EditorGUILayout.LabelField("Type", m_Device.GetType().Name);
            if (!string.IsNullOrEmpty(m_Device.description.interfaceName))
                EditorGUILayout.LabelField("Interface", m_Device.description.interfaceName);
            if (!string.IsNullOrEmpty(m_Device.description.product))
                EditorGUILayout.LabelField("Product", m_Device.description.product);
            if (!string.IsNullOrEmpty(m_Device.description.manufacturer))
                EditorGUILayout.LabelField("Manufacturer", m_Device.description.manufacturer);
            if (!string.IsNullOrEmpty(m_Device.description.serial))
                EditorGUILayout.LabelField("Serial Number", m_Device.description.serial);
            EditorGUILayout.LabelField("Device ID", m_DeviceIdString);
            if (!string.IsNullOrEmpty(m_DeviceUsagesString))
                EditorGUILayout.LabelField("Usages", m_DeviceUsagesString);
            if (!string.IsNullOrEmpty(m_DeviceFlagsString))
                EditorGUILayout.LabelField("Flags", m_DeviceFlagsString);
            if (m_Device is Keyboard)
                EditorGUILayout.LabelField("Keyboard Layout", ((Keyboard)m_Device).keyboardLayout);
            EditorGUILayout.EndVertical();

            DrawControlTree();
            DrawEventList();
        }

        private void DrawControlTree()
        {
            var updateTypeToShow = InputSystem.s_Manager.defaultUpdateType;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"Controls ({updateTypeToShow} State)", GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            // Allow plugins to add toolbar buttons.
            for (var i = 0; i < s_OnToolbarGUIActions.length; ++i)
                s_OnToolbarGUIActions[i](m_Device);

            if (GUILayout.Button(Contents.stateContent, EditorStyles.toolbarButton))
            {
                var window = CreateInstance<InputStateWindow>();
                window.InitializeWithControl(m_Device);
                window.Show();
            }

            GUILayout.EndHorizontal();

            if (m_NeedControlValueRefresh)
            {
                RefreshControlTreeValues();
                m_NeedControlValueRefresh = false;
            }

            ////REVIEW: I'm not sure tree view needs a scroll view or whether it does that automatically
            m_ControlTreeScrollPosition = EditorGUILayout.BeginScrollView(m_ControlTreeScrollPosition);
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_ControlTree.OnGUI(rect);
            EditorGUILayout.EndScrollView();
        }

        private void DrawEventList()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Events", GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(Contents.clearContent, EditorStyles.toolbarButton))
            {
                m_EventTrace.Clear();
                m_EventTree.Reload();
            }

            var eventTraceDisabledNow = GUILayout.Toggle(!m_EventTraceDisabled, Contents.pauseContent, EditorStyles.toolbarButton);
            if (eventTraceDisabledNow != m_EventTraceDisabled)
            {
                m_EventTraceDisabled = eventTraceDisabledNow;
                if (eventTraceDisabledNow)
                    m_EventTrace.Disable();
                else
                    m_EventTrace.Enable();
            }

            GUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_EventTree.OnGUI(rect);
        }

        ////FIXME: some of the state in here doesn't get refreshed when it's changed on the device
        private void InitializeWith(InputDevice device)
        {
            m_Device = device;
            m_DeviceId = device.deviceId;
            m_DeviceIdString = device.deviceId.ToString();
            m_DeviceUsagesString = string.Join(", ", device.usages.Select(x => x.ToString()).ToArray());

            var flags = new List<string>();
            if ((m_Device.m_DeviceFlags & InputDevice.DeviceFlags.Native) == InputDevice.DeviceFlags.Native)
                flags.Add("Native");
            if ((m_Device.m_DeviceFlags & InputDevice.DeviceFlags.Remote) == InputDevice.DeviceFlags.Remote)
                flags.Add("Remote");
            if ((m_Device.m_DeviceFlags & InputDevice.DeviceFlags.UpdateBeforeRender) == InputDevice.DeviceFlags.UpdateBeforeRender)
                flags.Add("UpdateBeforeRender");
            if ((m_Device.m_DeviceFlags & InputDevice.DeviceFlags.HasStateCallbacks) == InputDevice.DeviceFlags.HasStateCallbacks)
                flags.Add("HasStateCallbacks");
            if ((m_Device.m_DeviceFlags & InputDevice.DeviceFlags.Disabled) == InputDevice.DeviceFlags.Disabled)
                flags.Add("Disabled");
            m_DeviceFlagsString = string.Join(", ", flags.ToArray());

            // Set up event trace. The default trace size of 1mb fits a ton of events and will
            // likely bog down the UI if we try to display that many events. Instead, come up
            // with a more reasonable sized based on the state size of the device.
            if (m_EventTrace == null)
                m_EventTrace = new InputEventTrace((int)device.stateBlock.alignedSizeInBytes * kMaxNumEventsInTrace) {deviceId = device.deviceId};
            m_EventTrace.onEvent += _ =>
            {
                ////FIXME: this is very inefficient
                m_EventTree.Reload();
            };
            if (!m_EventTraceDisabled)
                m_EventTrace.Enable();

            // Set up event tree.
            m_EventTree = InputEventTreeView.Create(m_Device, m_EventTrace, ref m_EventTreeState, ref m_EventTreeHeaderState);

            // Set up control tree.
            m_ControlTree = InputControlTreeView.Create(m_Device, 1, ref m_ControlTreeState, ref m_ControlTreeHeaderState);
            m_ControlTree.Reload();
            m_ControlTree.ExpandAll();

            AddToList();

            InputSystem.onDeviceChange += OnDeviceChange;
            InputState.onChange += OnDeviceStateChange;
        }

        private void RefreshControlTreeValues()
        {
            var updateTypeToShow = InputSystem.s_Manager.defaultUpdateType;
            var currentUpdateType = InputState.currentUpdateType;

            InputStateBuffers.SwitchTo(InputSystem.s_Manager.m_StateBuffers, updateTypeToShow);
            m_ControlTree.RefreshControlValues();
            InputStateBuffers.SwitchTo(InputSystem.s_Manager.m_StateBuffers, currentUpdateType);
        }

        // We will lose our device on domain reload and then look it back up the first
        // time we hit a repaint after a reload. By that time, the input system should have
        // fully come back to life as well.
        [NonSerialized] private InputDevice m_Device;
        [NonSerialized] private string m_DeviceIdString;
        [NonSerialized] private string m_DeviceUsagesString;
        [NonSerialized] private string m_DeviceFlagsString;
        [NonSerialized] private InputControlTreeView m_ControlTree;
        [NonSerialized] private InputEventTreeView m_EventTree;
        [NonSerialized] private bool m_NeedControlValueRefresh;

        [SerializeField] private int m_DeviceId = InputDevice.InvalidDeviceId;
        [SerializeField] private TreeViewState m_ControlTreeState;
        [SerializeField] private TreeViewState m_EventTreeState;
        [SerializeField] private MultiColumnHeaderState m_ControlTreeHeaderState;
        [SerializeField] private MultiColumnHeaderState m_EventTreeHeaderState;
        [SerializeField] private Vector2 m_ControlTreeScrollPosition;
        [SerializeField] private InputEventTrace m_EventTrace;
        [SerializeField] private bool m_EventTraceDisabled;

        private static List<InputDeviceDebuggerWindow> s_OpenDebuggerWindows;

        private void AddToList()
        {
            if (s_OpenDebuggerWindows == null)
                s_OpenDebuggerWindows = new List<InputDeviceDebuggerWindow>();
            if (!s_OpenDebuggerWindows.Contains(this))
                s_OpenDebuggerWindows.Add(this);
        }

        private void RemoveFromList()
        {
            s_OpenDebuggerWindows?.Remove(this);
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device.deviceId != m_DeviceId)
                return;

            if (change == InputDeviceChange.Removed)
            {
                Close();
            }
            else
            {
                Repaint();
            }
        }

        private void OnDeviceStateChange(InputDevice device, InputEventPtr eventPtr)
        {
            ////REVIEW: Ideally we would defer the refresh until we repaint. That way, we would not refresh on every single
            ////        state change but rather only once for a repaint. However, for some reason, if we move the refresh
            ////        into OnGUI, something in Unity blows up and takes forever. It seems that we are invalidating some
            ////        cached material data over and over and over so that OnGUI suddenly becomes crazy expensive.

            ////FIXME: Reading values here means we won't be showing the effect of EditorWindowSpaceProcessor correctly. In the
            ////       input update, there is no current EditorWindow so no window to be relative to. However, even if we read the
            ////       values in OnGUI(), the result would always be relative to the debugger window (that'd probably be fine).

            if (InputState.currentUpdateType != InputSystem.s_Manager.defaultUpdateType)
                return;
            m_ControlTree?.RefreshControlValues();
            Repaint();
        }

        private static class Styles
        {
            public static string notFoundHelpText = "Device could not be found.";
        }

        private static class Contents
        {
            public static GUIContent clearContent = new GUIContent("Clear");
            public static GUIContent pauseContent = new GUIContent("Pause");
            public static GUIContent stateContent = new GUIContent("State");
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            AddToList();
        }
    }
}

#endif // UNITY_EDITOR
