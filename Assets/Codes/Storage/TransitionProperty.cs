using UnityEngine;
using System.Collections;
using UnityEngine.Scripting;
using ProtoBuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[ProtoContract()]
public enum Transition
{
    Constant,
    Linear
}

public static class TransitionLib
{
    public static float Lerp(float a, float b, float t, Transition trans)
    {
        return LerpUnclamped(a, b, Mathf.Clamp01(t), trans);
    }
    public static int Lerp(int a, int b, float t, Transition trans)
    {
        return LerpUnclamped(a, b, Mathf.Clamp01(t), trans);
    }
    public static float LerpUnclamped(float a, float b, float t, Transition trans)
    {
        switch (trans)
        {
            case Transition.Constant:
                return t < b ? a : b;
            case Transition.Linear:
                return Mathf.LerpUnclamped(a, b, t);
            default:
                Debug.Log("Unsupported transition property:" + trans);
                return a;
        };
    }

    public static int LerpUnclamped(int a, int b, float t, Transition trans)
    {
        switch (trans)
        {
            case Transition.Constant:
                return t < b ? a : b;
            case Transition.Linear:
                return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, t));
            default:
                Debug.Log("Unsupported transition property:" + trans);
                return a;
        };
    }
}

[Preserve]
[ProtoContract()]
public class TransitionProperty<T> : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public T value { get; set; }

    [ProtoMember(2)]
    public Transition transition { get; set; }

    public TransitionProperty() { }

    public TransitionProperty(T value, Transition transition = Transition.Constant)
    {
        this.value = value;
        this.transition = transition;
    }

    public static implicit operator T(TransitionProperty<T> prop)
    {
        return prop.value;
    }

    public void Set(T rhs, Transition trans)
    {
        value = rhs;
        transition = trans;
    }

    public void Set(T rhs)
    {
        value = rhs;
    }
}

[Preserve]
[ProtoContract()]
public class TransitionColor : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public TransitionProperty<byte> r { get; set; } = new TransitionProperty<byte>();

    [ProtoMember(2)]
    public TransitionProperty<byte> g { get; set; } = new TransitionProperty<byte>();

    [ProtoMember(3)]
    public TransitionProperty<byte> b { get; set; } = new TransitionProperty<byte>();

    [ProtoMember(4)]
    public TransitionProperty<byte> a { get; set; } = new TransitionProperty<byte>();

    public TransitionColor() { }
    public TransitionColor(byte r, byte g, byte b, byte a = 255, Transition transition = Transition.Constant)
    {
        this.r.Set(r, transition);
        this.g.Set(g, transition);
        this.b.Set(b, transition);
        this.a.Set(a, transition);
    }

    public static implicit operator Color(TransitionColor color)
    {
        return new Color(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.a / 255.0f);
    }

    public static TransitionColor Lerp(TransitionColor a, TransitionColor b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    public static TransitionColor LerpUnclamped(TransitionColor a, TransitionColor b, float t)
    {
        TransitionColor ret = new TransitionColor();
        ret.r.Set((byte)TransitionLib.LerpUnclamped(a.r, b.r, t, a.r.transition), a.r.transition);
        ret.g.Set((byte)TransitionLib.LerpUnclamped(a.g, b.g, t, a.g.transition), a.g.transition);
        ret.b.Set((byte)TransitionLib.LerpUnclamped(a.b, b.b, t, a.b.transition), a.b.transition);
        ret.a.Set((byte)TransitionLib.LerpUnclamped(a.a, b.a, t, a.a.transition), a.a.transition);
        return ret;
    }
}

[Preserve]
[ProtoContract()]
public class TransitionVector : IExtensible
{
    private IExtension __pbn__extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [ProtoMember(1)]
    public float x { get; set; }

    [ProtoMember(2)]
    public float y { get; set; }

    [ProtoMember(3)]
    public Transition transition;

    [JsonIgnore]
    public float z { get; set; }

    public TransitionVector() { }
    public TransitionVector(float x, float y, Transition transition = Transition.Linear)
    {
        this.x = x;
        this.y = y;
        this.transition = transition;
    }

    public static implicit operator Vector3(TransitionVector vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }

    public static TransitionVector Lerp(TransitionVector a, TransitionVector b, float t)
    {
        return new TransitionVector
        {
            x = TransitionLib.Lerp(a.x, b.x, t, a.transition),
            y = TransitionLib.Lerp(a.y, b.y, t, a.transition),
            z = TransitionLib.Lerp(a.z, b.z, t, a.transition)
        };
    }
}
