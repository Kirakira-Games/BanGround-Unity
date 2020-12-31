using UnityEngine;
using System.Collections;
using UnityEngine.Scripting;
using ProtoBuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

using Math = UnityEngine.Mathf;

[ProtoContract()]
public enum Transition
{
    Constant,
    Linear,
    QuadraticEaseIn,
    QuadraticEaseOut,
    QuadraticEaseInOut,
    CubicEaseIn,
    CubicEaseOut,
    CubicEaseInOut,
    QuarticEaseIn,
    QuarticEaseOut,
    QuarticEaseInOut,
    QuinticEaseIn,
    QuinticEaseOut,
    QuinticEaseInOut,
    SineEaseIn,
    SineEaseOut,
    SineEaseInOut,
    CircularEaseIn,
    CircularEaseOut,
    CircularEaseInOut,
    ExponentialEaseIn,
    ExponentialEaseOut,
    ExponentialEaseInOut,
    ElasticEaseIn,
    ElasticEaseOut,
    ElasticEaseInOut,
    BackEaseIn,
    BackEaseOut,
    BackEaseInOut,
    BounceEaseIn,
    BounceEaseOut,
    BounceEaseInOut
}

public static class TransitionLib
{
    public static float Lerp(float a, float b, float t, Transition trans)
        => LerpUnclamped(a, b, Math.Clamp01(t), trans);

    public static int Lerp(int a, int b, float t, Transition trans)
        => LerpUnclamped(a, b, Math.Clamp01(t), trans);

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t, Transition trans)
        => Vector3.LerpUnclamped(a, b, Easings.interpolationFuncs[(int)trans](t));

    public static float LerpUnclamped(float a, float b, float t, Transition trans)
        => Math.LerpUnclamped(a, b, Easings.interpolationFuncs[(int)trans](t));

    public static int LerpUnclamped(int a, int b, float t, Transition trans)
        => Math.RoundToInt(Math.LerpUnclamped(a, b, Easings.interpolationFuncs[(int)trans](t)));

    public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t, Transition trans)
        => Vector3.LerpUnclamped(a, b, Easings.interpolationFuncs[(int)trans](t));

    static public class Easings
    {
        /// <summary>
        /// Constant Pi.
        /// </summary>
        private const float PI = Math.PI;

        /// <summary>
        /// Constant Pi / 2.
        /// </summary>
        private const float HALFPI = Math.PI / 2.0f;

        public static readonly Func<float, float>[] interpolationFuncs =
        {
            Constant,
            Linear,
            QuadraticEaseIn,
            QuadraticEaseOut,
            QuadraticEaseInOut,
            CubicEaseIn,
            CubicEaseOut,
            CubicEaseInOut,
            QuarticEaseIn,
            QuarticEaseOut,
            QuarticEaseInOut,
            QuinticEaseIn,
            QuinticEaseOut,
            QuinticEaseInOut,
            SineEaseIn,
            SineEaseOut,
            SineEaseInOut,
            CircularEaseIn,
            CircularEaseOut,
            CircularEaseInOut,
            ExponentialEaseIn,
            ExponentialEaseOut,
            ExponentialEaseInOut,
            ElasticEaseIn,
            ElasticEaseOut,
            ElasticEaseInOut,
            BackEaseIn,
            BackEaseOut,
            BackEaseInOut,
            BounceEaseIn,
            BounceEaseOut,
            BounceEaseInOut
        };

        /// <summary>
        /// Zero or One
        /// </summary>
        static public float Constant(float p)
        {
            return p < 1.0f ? 0.0f : 1.0f;
        }

        /// <summary>
        /// Modeled after the line y = x
        /// </summary>
        static public float Linear(float p)
        {
            return p;
        }

        /// <summary>
        /// Modeled after the parabola y = x^2
        /// </summary>
        static public float QuadraticEaseIn(float p)
        {
            return p * p;
        }

        /// <summary>
        /// Modeled after the parabola y = -x^2 + 2x
        /// </summary>
        static public float QuadraticEaseOut(float p)
        {
            return -(p * (p - 2));
        }

        /// <summary>
        /// Modeled after the piecewise quadratic
        /// y = (1/2)((2x)^2)             ; [0, 0.5)
        /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
        /// </summary>
        static public float QuadraticEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 2 * p * p;
            }
            else
            {
                return (-2 * p * p) + (4 * p) - 1;
            }
        }

        /// <summary>
        /// Modeled after the cubic y = x^3
        /// </summary>
        static public float CubicEaseIn(float p)
        {
            return p * p * p;
        }

        /// <summary>
        /// Modeled after the cubic y = (x - 1)^3 + 1
        /// </summary>
        static public float CubicEaseOut(float p)
        {
            float f = (p - 1);
            return f * f * f + 1;
        }

        /// <summary>	
        /// Modeled after the piecewise cubic
        /// y = (1/2)((2x)^3)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
        /// </summary>
        static public float CubicEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 4 * p * p * p;
            }
            else
            {
                float f = ((2 * p) - 2);
                return 0.5f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quartic x^4
        /// </summary>
        static public float QuarticEaseIn(float p)
        {
            return p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quartic y = 1 - (x - 1)^4
        /// </summary>
        static public float QuarticEaseOut(float p)
        {
            float f = (p - 1);
            return f * f * f * (1 - p) + 1;
        }

        /// <summary>
        // Modeled after the piecewise quartic
        // y = (1/2)((2x)^4)        ; [0, 0.5)
        // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        /// </summary>
        static public float QuarticEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 8 * p * p * p * p;
            }
            else
            {
                float f = (p - 1);
                return -8 * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quintic y = x^5
        /// </summary>
        static public float QuinticEaseIn(float p)
        {
            return p * p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quintic y = (x - 1)^5 + 1
        /// </summary>
        static public float QuinticEaseOut(float p)
        {
            float f = (p - 1);
            return f * f * f * f * f + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quintic
        /// y = (1/2)((2x)^5)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
        /// </summary>
        static public float QuinticEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 16 * p * p * p * p * p;
            }
            else
            {
                float f = ((2 * p) - 2);
                return 0.5f * f * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave
        /// </summary>
        static public float SineEaseIn(float p)
        {
            return Math.Sin((p - 1) * HALFPI) + 1;
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave (different phase)
        /// </summary>
        static public float SineEaseOut(float p)
        {
            return Math.Sin(p * HALFPI);
        }

        /// <summary>
        /// Modeled after half sine wave
        /// </summary>
        static public float SineEaseInOut(float p)
        {
            return 0.5f * (1 - Math.Cos(p * PI));
        }

        /// <summary>
        /// Modeled after shifted quadrant IV of unit circle
        /// </summary>
        static public float CircularEaseIn(float p)
        {
            return 1 - Math.Sqrt(1 - (p * p));
        }

        /// <summary>
        /// Modeled after shifted quadrant II of unit circle
        /// </summary>
        static public float CircularEaseOut(float p)
        {
            return Math.Sqrt((2 - p) * p);
        }

        /// <summary>	
        /// Modeled after the piecewise circular function
        /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
        /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        static public float CircularEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * (1 - Math.Sqrt(1 - 4 * (p * p)));
            }
            else
            {
                return 0.5f * (Math.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
            }
        }

        /// <summary>
        /// Modeled after the exponential function y = 2^(10(x - 1))
        /// </summary>
        static public float ExponentialEaseIn(float p)
        {
            return (p == 0.0f) ? p : Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the exponential function y = -2^(-10x) + 1
        /// </summary>
        static public float ExponentialEaseOut(float p)
        {
            return (p == 1.0f) ? p : 1 - Math.Pow(2, -10 * p);
        }

        /// <summary>
        /// Modeled after the piecewise exponential
        /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
        /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
        /// </summary>
        static public float ExponentialEaseInOut(float p)
        {
            if (p == 0.0 || p == 1.0) return p;

            if (p < 0.5f)
            {
                return 0.5f * Math.Pow(2, (20 * p) - 10);
            }
            else
            {
                return -0.5f * Math.Pow(2, (-20 * p) + 10) + 1;
            }
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
        /// </summary>
        static public float ElasticEaseIn(float p)
        {
            return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
        /// </summary>
        static public float ElasticEaseOut(float p)
        {
            return Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise exponentially-damped sine wave:
        /// y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
        /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
        /// </summary>
        static public float ElasticEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * ((2 * p) - 1));
            }
            else
            {
                return 0.5f * (Math.Sin(-13 * HALFPI * ((2 * p - 1) + 1)) * Math.Pow(2, -10 * (2 * p - 1)) + 2);
            }
        }

        /// <summary>
        /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
        /// </summary>
        static public float BackEaseIn(float p)
        {
            return p * p * p - p * Math.Sin(p * PI);
        }

        /// <summary>
        /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
        /// </summary>	
        static public float BackEaseOut(float p)
        {
            float f = (1 - p);
            return 1 - (f * f * f - f * Math.Sin(f * PI));
        }

        /// <summary>
        /// Modeled after the piecewise overshooting cubic function:
        /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
        /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
        /// </summary>
        static public float BackEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                float f = 2 * p;
                return 0.5f * (f * f * f - f * Math.Sin(f * PI));
            }
            else
            {
                float f = (1 - (2 * p - 1));
                return 0.5f * (1 - (f * f * f - f * Math.Sin(f * PI))) + 0.5f;
            }
        }

        /// <summary>
        /// </summary>
        static public float BounceEaseIn(float p)
        {
            return 1 - BounceEaseOut(1 - p);
        }

        /// <summary>
        /// </summary>
        static public float BounceEaseOut(float p)
        {
            if (p < 4 / 11.0f)
            {
                return (121 * p * p) / 16.0f;
            }
            else if (p < 8 / 11.0f)
            {
                return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
            }
            else if (p < 9 / 10.0f)
            {
                return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
            }
            else
            {
                return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
            }
        }

        /// <summary>
        /// </summary>
        static public float BounceEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * BounceEaseIn(p * 2);
            }
            else
            {
                return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
            }
        }
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

    public static float LerpFloat(TransitionProperty<float> a, TransitionProperty<float> b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpFloatUnclamped(a, b, t);
    }

    public static float LerpFloatUnclamped(TransitionProperty<float> a, TransitionProperty<float> b, float t)
    {
        return TransitionLib.LerpUnclamped(a, b, t, a.transition);
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

    public static string ColoredString(string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
    }

    public TransitionColor Copy()
    {
        return new TransitionColor(r, g, b, a, transition);
    }

    public override string ToString()
    {
        return $"({r},{g},{b},{a} [{transition}])";
    }

    public void Set(Color c)
    {
        Set((byte)Mathf.Lerp(0, 255, c.r),
            (byte)Mathf.Lerp(0, 255, c.g),
            (byte)Mathf.Lerp(0, 255, c.b),
            (byte)Mathf.Lerp(0, 255, c.a)
        );
    }
    public void Set(Color c, Transition transition)
    {
        Set((byte)Mathf.Lerp(0, 255, c.r),
            (byte)Mathf.Lerp(0, 255, c.g),
            (byte)Mathf.Lerp(0, 255, c.b),
            (byte)Mathf.Lerp(0, 255, c.a),
            transition
        );
    }

    public void Set(byte r, byte g, byte b, byte a = 255)
    {
        Set(r, g, b, a, transition);
    }

    public void Set(byte r, byte g, byte b, byte a, Transition transition)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
        this.transition = transition;
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

    public static Vector3 LerpVector(TransitionVector a, TransitionVector b, float t)
    {
        t = Math.Clamp01(t);
        return LerpVectorUnclamped(a, b, t);
    }

    public static Vector3 LerpVectorUnclamped(TransitionVector a, TransitionVector b, float t)
    {
        t = Math.Clamp01(t);
        return new Vector3(
            TransitionProperty<float>.LerpFloatUnclamped(a.x, b.x, t),
            TransitionProperty<float>.LerpFloatUnclamped(a.y, b.y, t),
            Math.LerpUnclamped(a.z, b.z, t)
        );
    }

    public static TransitionVector Lerp(TransitionVector a, TransitionVector b, float t)
    {
        t = Math.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    public static TransitionVector LerpUnclamped(TransitionVector a, TransitionVector b, float t)
    {
        return new TransitionVector
        {
            x = TransitionProperty<float>.LerpUnclamped(a.x, b.x, t),
            y = TransitionProperty<float>.LerpUnclamped(a.y, b.y, t),
            z = Math.LerpUnclamped(a.z, b.z, t)
        };
    }

    public override string ToString()
    {
        return $"({x.value} [{x.transition}], {y.value}[{y.transition}], {z})";
    }
}
