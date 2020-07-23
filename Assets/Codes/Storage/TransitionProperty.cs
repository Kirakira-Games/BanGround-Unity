using UnityEngine;
using System.Collections;
using UnityEngine.Scripting;
using ProtoBuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

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
                return t < 1 ? a : b;
            case Transition.Linear:
                return Mathf.LerpUnclamped(a, b, t);
            default:
                Debug.LogWarning("Unsupported transition property:" + trans);
                return a;
        };
    }

    public static int LerpUnclamped(int a, int b, float t, Transition trans)
    {
        switch (trans)
        {
            case Transition.Constant:
                return t < 1 ? a : b;
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

    public TransitionProperty<T> Copy()
    {
        return new TransitionProperty<T>(value, transition);
    }

    public static TransitionProperty<float> Lerp(TransitionProperty<float> a, TransitionProperty<float> b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    public static TransitionProperty<float> LerpUnclamped(TransitionProperty<float> a, TransitionProperty<float> b, float t)
    {
        return new TransitionProperty<float>(TransitionLib.LerpUnclamped(a, b, t, a.transition), a.transition);
    }

    public static TransitionProperty<int> Lerp(TransitionProperty<int> a, TransitionProperty<int> b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    public static TransitionProperty<int> LerpUnclamped(TransitionProperty<int> a, TransitionProperty<int> b, float t)
    {
        return new TransitionProperty<int>(TransitionLib.LerpUnclamped(a, b, t, a.transition), a.transition);
    }

    public override string ToString()
    {
        return $"{value} [{transition}]";
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
    public byte r;

    [ProtoMember(2)]
    public byte g;

    [ProtoMember(3)]
    public byte b;

    [ProtoMember(4)]
    public byte a;

    [ProtoMember(5)]
    public Transition transition;

    public TransitionColor() { }
    public TransitionColor(byte r, byte g, byte b, byte a = 255, Transition transition = Transition.Constant)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
        this.transition = transition;
    }

    public static implicit operator Color(TransitionColor color)
    {
        return new Color(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f);
    }

    public static TransitionColor Lerp(TransitionColor a, TransitionColor b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    public static TransitionColor LerpUnclamped(TransitionColor a, TransitionColor b, float t)
    {
        TransitionColor ret = new TransitionColor
        {
            r = (byte)TransitionLib.LerpUnclamped(a.r, b.r, t, a.transition),
            g = (byte)TransitionLib.LerpUnclamped(a.g, b.g, t, a.transition),
            b = (byte)TransitionLib.LerpUnclamped(a.b, b.b, t, a.transition),
            a = (byte)TransitionLib.LerpUnclamped(a.a, b.a, t, a.transition),
            transition = a.transition
        };
        return ret;
    }

    public TransitionColor Copy()
    {
        return new TransitionColor(r, g, b, a, transition);
    }

    public override string ToString()
    {
        return $"({r},{g},{b},{a} [{transition}])";
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
    public TransitionProperty<float> x { get; set; }

    [ProtoMember(2)]
    public TransitionProperty<float> y { get; set; }

    [JsonIgnore]
    public float z { get; set; }

    public TransitionVector() { }
    public TransitionVector(float x, float y, Transition transX = Transition.Linear, Transition transY = Transition.Linear)
    {
        this.x = new TransitionProperty<float>(x, transX);
        this.y = new TransitionProperty<float>(y, transY);
    }

    public TransitionVector Copy()
    {
        return new TransitionVector
        {
            x = x.Copy(),
            y = y.Copy(),
            z = z
        };
    }

    public static implicit operator Vector3(TransitionVector vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }

    public static TransitionVector Lerp(TransitionVector a, TransitionVector b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    public static TransitionVector LerpUnclamped(TransitionVector a, TransitionVector b, float t)
    {
        return new TransitionVector
        {
            x = TransitionProperty<float>.LerpUnclamped(a.x, b.x, t),
            y = TransitionProperty<float>.LerpUnclamped(a.y, b.y, t),
            z = Mathf.LerpUnclamped(a.z, b.z, t)
        };
    }

    public override string ToString()
    {
        return $"({x.value} [{x.transition}], {y.value}[{y.transition}], {z})";
    }
}
