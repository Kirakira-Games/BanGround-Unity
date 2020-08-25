using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
//using UnityEditor.UIElements;
using UnityEngine;
using Zenject;
using FS = System.IO.KiraFilesystem;

[Flags]
public enum KVarFlags
{
    None = 1,
    // Only setable if DEBUG build
    DevelopmentOnly = 2,
    // Hide for user
    Hidden = 4,
    // Require g_cheats 1 for editing
    Cheat = 8,
    // Will save to config.cfg
    Archive = 16,
    // Only string
    StringOnly = 32
}

public class KonCommandBase
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
}

// Kirakira Command
public class Kommand : KonCommandBase
{
    IKVSystem _kvSystem;

    private Action<string[]> command;

    public void Invoke(string[] args = null) => command(args);

    public class KommandInfo
    {
        public string Name;
        public string Help;
        public Action<string[]> Command;
    }

    public static KommandInfo C(string name, string help, Action<string[]> command) => new KommandInfo
    {
        Name = name,
        Help = help,
        Command = command
    };

    public static KommandInfo C(string name, string help, Action command) => new KommandInfo
    {
        Name = name,
        Help = help,
        Command = _ => command()
    };

    public Kommand(IKVSystem kvSystem) 
    {
        _kvSystem = kvSystem;
    }

    public static Action<InjectContext, object> OnInit(KommandInfo info)
    {
        return (_, obj) =>
        {
            if (obj is ValidationMarker)
                return;
            var me = obj as Kommand;

            me.Name = info.Name;
            me.Description = info.Help;
            me.command = info.Command;

            me._kvSystem.Add(me);
        };
    }
}

// Kirakira Variable
public class KVar : KonCommandBase
{
    IKVSystem _kvSystem;

    public string Default { get; private set; }

    private KVarFlags m_flags;
    private string m_stringValue = "0";
    private int m_intValue = 0;
    private float m_floatValue = 0.0f;
    private bool m_boolValue = false;

    private Action<object> m_cbValueChanged = null;
    private Action<object, IKVSystem> m_cbValueChangedAlt = null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateValue(object o)
    {
        if (o is string)
        {
            int.TryParse(m_stringValue, out m_intValue);
            float.TryParse(m_stringValue, out m_floatValue);
            m_boolValue = Convert.ToBoolean(m_intValue);
        }
        else if (o is int)
        {
            m_stringValue = m_intValue.ToString();
            m_floatValue = m_intValue;
            m_boolValue = Convert.ToBoolean(m_intValue);
        }
        else if (o is float)
        {
            m_stringValue = m_floatValue.ToString();
            m_intValue = Convert.ToInt32(Math.Floor(m_floatValue));
            m_boolValue = Convert.ToBoolean(m_intValue);
        }
        else if (o is bool)
        {
            m_intValue = Convert.ToInt32(m_boolValue);
            m_floatValue = m_intValue;
            m_stringValue = m_intValue.ToString();
        }
    }

    readonly static Type stringType = typeof(string);
    readonly static Type intType = typeof(int);
    readonly static Type boolType = typeof(bool);
    readonly static Type floatType = typeof(float);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get<T>()
    {
        if (IsFlagSet(KVarFlags.StringOnly))
        {
            if (typeof(T) != typeof(string))
                throw new ArgumentException("Only string is acceptable!");
            return (T)(object)m_stringValue;
        }

        var type = typeof(T);

        if (type == stringType)
            return (T)(object)m_stringValue;
        else if (type == intType)
            return (T)(object)m_intValue;
        else if (type == floatType)
            return (T)(object)m_floatValue;
        else if (type == boolType)
            return (T)(object)m_boolValue;
        else
            throw new ArgumentOutOfRangeException("Type not supported");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set<T>(T value)
    {
#if !DEBUG
        if (IsFlagSet(KVarFlags.DevelopmentOnly))
            throw new AccessViolationException("KVar is read only!");
#endif

        if (IsFlagSet(KVarFlags.Cheat) && !_kvSystem.CanCheat)
            throw new AccessViolationException("KVar is inwriteable while cheat not enabled only!");

        if (IsFlagSet(KVarFlags.StringOnly))
        {
            if (!(value is string))
                throw new ArgumentException("Only strings are acceptable!");

            string lv = m_stringValue;

            m_stringValue = (string)(object)value;

            m_cbValueChanged?.Invoke(lv);
            m_cbValueChangedAlt?.Invoke(lv, _kvSystem);
            return;
        }

        object lastValue;

        if(value is string)
        {
            lastValue = m_stringValue;
            m_stringValue = (string)(object)value;
        }
        else if(value is int)
        {
            lastValue = m_intValue;
            m_intValue = (int)(object)value;
        }
        else if(value is float)
        {
            lastValue = m_floatValue;
            m_floatValue = (float)(object)value;
        }
        else if(value is bool)
        {
            lastValue = m_boolValue;
            m_boolValue = (bool)(object)value;
        }
        else
        {
            throw new ArgumentOutOfRangeException("Type not supported");
        }

        UpdateValue(value);
        m_cbValueChanged?.Invoke(lastValue);
        m_cbValueChangedAlt?.Invoke(lastValue, _kvSystem);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFlagSet(KVarFlags flag)
    {
        return m_flags.HasFlag(flag);
    }

    public class KVarInfo
    {
        public string Name;
        public string DefaultValue;
        public KVarFlags Flag = 0;
        public string Help = "";
        public Action<object> Callback = null;
        public Action<object, IKVSystem> CallbackAlt = null;
    }

    public static KVarInfo C(string name, string defaultValue, KVarFlags flag = 0, string help = "", Action<object> callback = null) => new KVarInfo { Name = name, DefaultValue = defaultValue, Flag = flag, Help = help, Callback = callback };
    public static KVarInfo C(string name, string defaultValue, KVarFlags flag, string help, Action<object, IKVSystem> callback) => new KVarInfo { Name = name, DefaultValue = defaultValue, Flag = flag, Help = help, CallbackAlt = callback };

    public KVar(IKVSystem kvSystem) 
    {
        _kvSystem = kvSystem;
    }

    public static Action<InjectContext, object> OnInit(KVarInfo info)
    {
        return (_, obj) =>
        {
            if (obj is ValidationMarker)
                return;
            var me = obj as KVar;

            me.Name = info.Name;
            me.Description = info.Help;
            me.Default = info.DefaultValue;
            me.m_stringValue = info.DefaultValue;
            me.m_flags = info.Flag;
            me.m_cbValueChanged = info.Callback;
            me.m_cbValueChangedAlt = info.CallbackAlt;

            if (!me.IsFlagSet(KVarFlags.StringOnly))
                me.UpdateValue(me.m_stringValue);

            me._kvSystem.Add(me);
        };
    }

    public static implicit operator string(KVar kVar) => kVar.Get<string>();
    public static implicit operator int(KVar kVar) => kVar.Get<int>();
    public static implicit operator float(KVar kVar) => kVar.Get<float>();
    public static implicit operator bool(KVar kVar) => kVar.Get<bool>();

    // ---------------------------------
    public static explicit operator SEStyle(KVar kVar) => (SEStyle)kVar.Get<int>();
    public static explicit operator NoteStyle(KVar kVar) => (NoteStyle)kVar.Get<int>();
    public static explicit operator Sorter(KVar kVar) => (Sorter)kVar.Get<int>();
}

public class KVarRef
{
    private string m_kvarName;
    private KVar kvarCache = null;
    private KVar kVar 
    { 
        get 
        { 
            if (kvarCache == null) 
                kvarCache = KVSystem.Instance.Find(m_kvarName); 

            return kvarCache; 
        } 
    }

    public T Get<T>()
    {
        return kVar.Get<T>();
    }

    public void Set<T>(T value)
    {
        kVar.Set(value);
    }

    public bool IsFlagSet(KVarFlags flag) => kVar.IsFlagSet(flag);

    public KVarRef(string name)
    {
        m_kvarName = name;
    }

    public static implicit operator string(KVarRef kVar) => kVar.Get<string>();
    public static implicit operator int(KVarRef kVar) => kVar.Get<int>();
    public static implicit operator float(KVarRef kVar) => kVar.Get<float>();
    public static implicit operator bool(KVarRef kVar) => kVar.Get<bool>();

    // ---------------------------------
    public static explicit operator SEStyle(KVarRef kVar) => (SEStyle)kVar.Get<int>();
    public static explicit operator NoteStyle(KVarRef kVar) => (NoteStyle)kVar.Get<int>();
    public static explicit operator Sorter(KVarRef kVar) => (Sorter)kVar.Get<int>();
}

public class KVSystem : IKVSystem
{
    public static KVSystem Instance;

    private bool isReloadingConfig = false;

    // All commands
    private Dictionary<string, KonCommandBase> m_allCmds = new Dictionary<string, KonCommandBase>();

    // Wait for assign values, for kvars that does not exists but already had readed value from config
    private Dictionary<string, string> m_wfaValues = new Dictionary<string, string>();

    // g_cheats KVar, for CanCheat check
    private KVarRef cheat = new KVarRef("g_cheats");

    public bool CanCheat => cheat.Get<bool>();

    KVSystem()
    {
        Instance = this;  // TODO: Remove
    }

    public unsafe static string[] CommandLineToArgs(string str)
    {
        var stack = new Stack<char>();

        fixed (char* pszCmd = str)
        {
            var pszCmda_cpy = pszCmd - 1;

            while (++pszCmda_cpy < pszCmd + str.Length)
            {
                if (*pszCmda_cpy == '"' || *pszCmda_cpy == '\'')
                {
                    if (stack.Count > 0 && stack.Peek() == *pszCmda_cpy)
                        stack.Pop();
                    else
                        stack.Push(*pszCmda_cpy);
                }

                if (*pszCmda_cpy == ' ' && stack.Count == 0)
                    *pszCmda_cpy = '\n';
            }
        }

        var output = str.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < output.Length; i++)
            output[i] = output[i].Trim('\'', '"');

        return output;
    }

    public void ExecuteLine(string line, bool userInput = false)
    {
        string command = line;

        // Check for comments
        var commentPosition = line.IndexOf("//");

        if (commentPosition != -1)
            command = line.Substring(0, commentPosition);

        // Don't exec if the command is empty
        if (string.IsNullOrEmpty(command) || string.IsNullOrWhiteSpace(command))
            return;

        // Trim command
        command.TrimStart(' ');

        // Remove quotes
        command.Replace("\"", "");

        // Split by whitespace
        var parts = command.Split(' ');

        // cmd name is the first part
        var cmdName = parts[0];

        // left parts are just value/params...
        var value = command.Replace(cmdName, "").TrimStart(' ');

        // Check if we are good to go
        if (m_allCmds.ContainsKey(cmdName))
        {
            var cmd = m_allCmds[cmdName];

            if (cmd is KVar kVar)
            {
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value))
                {
                    if (userInput)
                    {
                        Debug.Log($"KVar: {cmdName} - {kVar.Get<string>()} (Default: {kVar.Default})");

                        if (!string.IsNullOrEmpty(kVar.Description))
                            Debug.Log(kVar.Description);
                    }

                    return;
                }

                if (kVar.IsFlagSet(KVarFlags.Hidden)
#if !DEBUG
             || kVar.IsFlagSet(KVarFlags.DevelopmentOnly)
#endif
                )
                {
                    // Shhhh! looks like it's hidden, just skip it.
                    return;
                }

                // Set it
                kVar.Set(value);
            }
            else if(cmd is Kommand kmd)
            {
                string[] args = CommandLineToArgs(value);
                kmd.Invoke(args);
            }
        }
        else
        {
            if (userInput)
            {
                Debug.Log($"KonCommand {cmdName} not found.");
            }

            m_wfaValues[cmdName] = value;
        }
    }

    public void ReloadConfig() 
    {
        isReloadingConfig = true;
        ExecuteLine("exec config.cfg");
        isReloadingConfig = false;
    }

    public void SaveConfig()
    {
        if (isReloadingConfig)
            return; // Fix by findstr;

        var cfg = "// generated by Kirakira Games, do not modify\n";

        foreach(var (name, cmd) in m_allCmds)
        {
            if (cmd is KVar var)
            {
                if (var.IsFlagSet(KVarFlags.Archive))
                {
                    cfg = $"{cfg}{name} {var.Get<string>()}\n";
                }
            }
        }

        var path = Path.Combine(DataLoader.DataDir, "config.cfg");

        if (File.Exists(path))
            File.Delete(path);

        File.WriteAllText(path, cfg);
    }

    public void Add(KonCommandBase cmd)
    {
        string name = cmd.Name;

        if (cmd is KVar var)
        {
            if (m_wfaValues.ContainsKey(name))
            {
                var.Set(m_wfaValues[name]);
                m_wfaValues.Remove(name);
            }
        }

        // This will throws if kvar already exists, do check before you create a kvar!
        m_allCmds.Add(name, cmd);
    }

    public KVar Find(string name)
    {
        if (m_allCmds.ContainsKey(name))
            if (m_allCmds[name] is KVar var)
                return var;

        return null;
    }

    public IEnumerator<KonCommandBase> GetEnumerator()
    {
        foreach(var (name, cmd) in m_allCmds)
        {
            yield return cmd;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}