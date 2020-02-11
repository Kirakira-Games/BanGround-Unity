using UnityEngine;
using System.Collections;
using Un4seen.Bass;
using System;

public class ModBase
{
    public virtual float ScoreMultiplier => 1.0f;
    public virtual Type[] IncompatibleMods => Array.Empty<Type>();
}

public class AudioMod : ModBase
{
    public virtual float SpeedCompensation => 1.0f;
    public virtual void ApplyMod(BassMemStream stream){}
}

public class DoubleMod : AudioMod
{
    public static DoubleMod Instanse = new DoubleMod();

    public override Type[] IncompatibleMods => new Type[] { typeof(HalfMod) };

    public override float SpeedCompensation => 1.5f;
    public override float ScoreMultiplier => 1.5f;

    public override void ApplyMod(BassMemStream stream)
    {
        Bass.BASS_ChannelSetAttribute(stream.ID, BASSAttribute.BASS_ATTRIB_TEMPO, 50.0f);
    }
}

public class HalfMod : AudioMod
{
    public static HalfMod Instanse = new HalfMod();

    public override Type[] IncompatibleMods => new Type[] { typeof(DoubleMod) };

    public override float SpeedCompensation => 0.75f;
    public override float ScoreMultiplier => 0.75f;

    public override void ApplyMod(BassMemStream stream)
    {
        Bass.BASS_ChannelSetAttribute(stream.ID, BASSAttribute.BASS_ATTRIB_TEMPO, -25.0f);
    }
}