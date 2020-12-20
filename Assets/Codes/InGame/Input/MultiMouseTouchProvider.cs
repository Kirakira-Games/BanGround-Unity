using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MultiMouseTouchProvider : InputManagerTouchProvider
{
    [DllImport("LibRawInput")]
    private static extern bool init();

    [DllImport("LibRawInput")]
    private static extern bool kill();

    [DllImport("LibRawInput")]
    private static extern IntPtr poll();

    public const byte RE_DEVICE_CONNECT = 0;
    public const byte RE_MOUSE = 2;
    public const byte RE_DEVICE_DISCONNECT = 1;
    public string getEventName(byte id)
    {
        switch (id)
        {
            case RE_DEVICE_CONNECT: return "RE_DEVICE_CONNECT";
            case RE_DEVICE_DISCONNECT: return "RE_DEVICE_DISCONNECT";
            case RE_MOUSE: return "RE_MOUSE";
        }
        return "UNKNOWN(" + id + ")";
    }

    public float defaultMiceSensitivity = 1f;
    public float accelerationThreshold = 40;
    public float accelerationMultiplier = 2;
    public int screenBorderPixels = 16;

    [StructLayout(LayoutKind.Sequential)]
    public struct RawInputEvent
    {
        public int devHandle;
        public int x, y, wheel;
        public byte press;
        public byte release;
        public byte type;
    }

#pragma warning disable 0649
    public class MousePointer
    {
        public GameObject obj;
        public Vector2 position;
        public int deviceID;
        public float sensitivity;

        public int iPress;
        public bool prevButton;
        public bool button => iPress > 0;
    }

    Dictionary<int, MousePointer> pointersByDeviceId = new Dictionary<int, MousePointer>();
    Transform transform;
    private GameObject cursor;

    public MultiMouseTouchProvider()
    {
        init();
        cursor = GameObject.Find("CSample");
        cursor.SetActive(false);
        transform = GameObject.Find("MouseCanvas").transform;
    }

    ~MultiMouseTouchProvider()
    {
        kill();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    MousePointer AddCursor(int deviceId)
    {
        MousePointer mp = null;

        Debug.Log("Adding DeviceID " + deviceId);
        mp = new MousePointer();
        mp.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        mp.obj = GameObject.Instantiate(cursor, transform) as GameObject;
        mp.obj.SetActive(true);
        var rt = mp.obj.GetComponent<RectTransform>();
        rt.position = mp.position;

        return mp;
    }

    void DeleteCursor(int deviceId)
    {
        var mp = pointersByDeviceId[deviceId];
        pointersByDeviceId.Remove(mp.deviceID);
        GameObject.Destroy(mp.obj);
    }

    MousePointer CreateOrGetPoint(int deviceId)
    {
        MousePointer pointer = null;
        if(!pointersByDeviceId.TryGetValue(deviceId, out pointer))
        {
            pointer = pointersByDeviceId[deviceId] = AddCursor(deviceId);
        }

        return pointer;
    }

    void UpdateMouseStatus()
    {
        IntPtr data = poll();
        int numEvents = Marshal.ReadInt32(data);

        for (int i = 0; i < numEvents; ++i)
        {
            var ev = new RawInputEvent();
            long offset = data.ToInt64() + sizeof(int) + i * Marshal.SizeOf(ev);
            ev.devHandle = Marshal.ReadInt32(new IntPtr(offset + 0));
            ev.x = Marshal.ReadInt32(new IntPtr(offset + 4));
            ev.y = Marshal.ReadInt32(new IntPtr(offset + 8));
            ev.wheel = Marshal.ReadInt32(new IntPtr(offset + 12));
            ev.press = Marshal.ReadByte(new IntPtr(offset + 16));
            ev.release = Marshal.ReadByte(new IntPtr(offset + 17));
            ev.type = Marshal.ReadByte(new IntPtr(offset + 18));
            //Debug.Log(getEventName(ev.type) + ":  H=" + ev.devHandle + ";  (" + ev.x + ";" + ev.y + ")  Down=" + ev.press + " Up=" + ev.release);

            if (ev.type == RE_DEVICE_DISCONNECT) 
                DeleteCursor(ev.devHandle);
            else if (ev.type == RE_MOUSE)
            {
                MousePointer pointer = CreateOrGetPoint(ev.devHandle);
                float dx = ev.x * defaultMiceSensitivity;
                float dy = ev.y * defaultMiceSensitivity;

                if (Mathf.Abs(dx) > accelerationThreshold) 
                    dx *= accelerationMultiplier;
                if (Mathf.Abs(dy) > accelerationThreshold) 
                    dy *= accelerationMultiplier;

                pointer.position = new Vector2(
                    Mathf.Clamp(pointer.position.x + dx, 0, Screen.width),
                    Mathf.Clamp(pointer.position.y - dy, 0, Screen.height));

                pointer.prevButton = pointer.button;

                pointer.iPress += ev.press;
                pointer.iPress -= ev.release;
                //Debug.Log($"p:{ev.press},r:{ev.release},ip:{pointer.iPress},down:{pointer.button},last:{pointer.prevButton}");

                //pointer.button = pointer.iPress > 0;

                RectTransform rt = pointer.obj.GetComponent<RectTransform>();
                rt.position = pointer.position;
            }
        }
    }

    public override KirakiraTouchState[][] GetTouches()
    {
        var states = base.GetTouches()[0].ToList();

        UpdateMouseStatus();

        var enumerator = pointersByDeviceId.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current.Value;
            var id = enumerator.Current.Key;

            if (current.button)
            {
                var ray = NoteController.mainCamera.ScreenPointToRay(current.position);
                var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;
                var phase = KirakiraTouchPhase.Ongoing;

                if (current.button != current.prevButton)
                {
                    phase = KirakiraTouchPhase.Began;
                }

                var touch = new KirakiraTouchState
                {
                    touchId = id,
                    screenPos = current.position,
                    pos = pos,
                    time = NoteController.judgeTime,
                    phase = phase
                };
                //Debug.Log(touch.ToString());
                states.Add(touch);
            }
            else
            {
                if (current.button != current.prevButton)
                {
                    var ray = NoteController.mainCamera.ScreenPointToRay(current.position);
                    var pos = NoteUtility.JudgePlane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : KirakiraTouch.INVALID_POSITION;

                    states.Add(new KirakiraTouchState
                    {
                        touchId = id,
                        screenPos = current.position,
                        pos = pos,
                        time = NoteController.judgeTime,
                        phase = KirakiraTouchPhase.Ended
                    });
                }
            }
        }

        return new KirakiraTouchState[][] { states.ToArray() };
    }
}
