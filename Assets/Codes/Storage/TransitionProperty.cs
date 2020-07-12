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
        switch (trans)
        {
            case Transition.Constant:
                return a;
            case Transition.Linear:
                return Mathf.Lerp(a, b, t);
            default:
                Debug.Log("Unsupported transition property:" + trans);
                return a;
        };
    }

    public static int Lerp(int a, int b, int t, Transition trans)
    {
        switch (trans)
        {
            case Transition.Constant:
                return a;
            case Transition.Linear:
                return Mathf.RoundToInt(Mathf.Lerp(a, b, t));
            default:
                Debug.Log("Unsupported transition property:" + trans);
                return a;
        };
    }
}

[Preserve]
[ProtoContract()]
public class TransitionProperty<T>
{
    [ProtoMember(1)]
    public T value;

    [ProtoMember(2)]
    public Transition transition;

    public TransitionProperty() { }

    public TransitionProperty(T value, Transition transition)
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
}

[Preserve]
[ProtoContract()]
public class TransitionColor
{
    [ProtoMember(1)]
    public TransitionProperty<byte> r = new TransitionProperty<byte>();

    [ProtoMember(2)]
    public TransitionProperty<byte> g = new TransitionProperty<byte>();

    [ProtoMember(3)]
    public TransitionProperty<byte> b = new TransitionProperty<byte>();

    [ProtoMember(4)]
    public TransitionProperty<byte> a = new TransitionProperty<byte>();

    public static implicit operator Color(TransitionColor color)
    {
        return new Color(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.a / 255.0f);
    }

    public static TransitionColor Lerp(TransitionColor a, TransitionColor b, float t)
    {
        TransitionColor ret = new TransitionColor();
        ret.r.Set((byte)TransitionLib.Lerp(a.r, b.r, t, a.r.transition), a.r.transition);
        ret.g.Set((byte)TransitionLib.Lerp(a.g, b.g, t, a.g.transition), a.g.transition);
        ret.b.Set((byte)TransitionLib.Lerp(a.b, b.b, t, a.b.transition), a.b.transition);
        ret.a.Set((byte)TransitionLib.Lerp(a.a, b.a, t, a.a.transition), a.a.transition);
        return ret;
    }
}
