using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

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

// KVar == Kirakira Variable
public class KVar
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Default { get; private set; }

    private KVarFlags m_flags;
    private string m_stringValue = "0";
    private int m_intValue = 0;
    private float m_floatValue = 0.0f;
    private bool m_boolValue = false;

    private Action<object> m_cbValueChanged = null;

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

    public T Get<T>()
    {
        if (IsFlagSet(KVarFlags.StringOnly))
        {
            if (typeof(T).Name != "String")
                throw new ArgumentException("Only string is acceptable!");

            return (T)(object)m_stringValue;
        }

        var type = typeof(T);

        switch (type.Name)
        {
            case "String":
                return (T)(object)m_stringValue;
            case "Int32":
                return (T)(object)m_intValue;
            case "Single":
                return (T)(object)m_floatValue;
            case "Boolean":
                return (T)(object)m_boolValue;
            default:
                throw new ArgumentOutOfRangeException("Type not supported");
        }
    }

    public void Set<T>(T value)
    {
#if !DEBUG
        if (IsFlagSet(KVarFlags.DevelopmentOnly))
            throw new AccessViolationException("KVar is read only!");
#endif

        if (IsFlagSet(KVarFlags.Cheat) && !KVSystem.Instance.CanCheat)
            throw new AccessViolationException("KVar is inwriteable while cheat not enabled only!");

        if (IsFlagSet(KVarFlags.StringOnly))
        {
            if (typeof(T).Name != "String")
                throw new ArgumentException("Only strings are acceptable!");

            m_stringValue = (string)(object)value;
            return;
        }

        var type = typeof(T);

        object lastValue;

        switch (type.Name)
        {
            case "String":
                lastValue = m_stringValue;
                m_stringValue = (string)(object)value;
                break;
            case "Int32":
                lastValue = m_intValue;
                m_intValue = (int)(object)value;
                break;
            case "Single":
                lastValue = m_floatValue;
                m_floatValue = (float)(object)value;
                break;
            case "Boolean":
                lastValue = m_boolValue;
                m_boolValue = (bool)(object)value;
                break;
            default:
                throw new ArgumentOutOfRangeException("Type not supported");
        }

        UpdateValue(value);

        m_cbValueChanged?.Invoke(lastValue);
    }

    public bool IsFlagSet(KVarFlags flag)
    {
        return m_flags.HasFlag(flag);
    }

    /// <summary>
    /// Create a KVar
    /// </summary>
    /// <param name="name">Name of the KVar</param>
    /// <param name="defaultValue">Default Value</param>
    /// <param name="flag">Flags</param>
    /// <param name="help">Help string</param>
    /// <param name="callback">Callback that will be called after value changed</param>
    public KVar(string name, string defaultValue, KVarFlags flag = 0, string help = "", Action<object> callback = null)
    {
        Name = name;
        Description = help;
        Default = defaultValue;

        m_stringValue = defaultValue;
        m_flags = flag;
        m_cbValueChanged = callback;

        if (!IsFlagSet(KVarFlags.StringOnly))
            UpdateValue(m_stringValue);

        KVSystem.Instance.Add(this);
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
    private KVar kVar { get => KVSystem.Instance.Find(m_kvarName); }

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

public class KVSystem
{
    public static KVSystem Instance = new KVSystem();

    // All kvars
    private Dictionary<string, KVar> m_allKvars = new Dictionary<string, KVar>();

    // Wait for assign values, for kvars that does not exists but already had readed value from config
    private Dictionary<string, string> m_wfaValues = new Dictionary<string, string>();

    // g_cheats KVar, for CanCheat check
    private KVar cheat;

    public bool CanCheat
    {
        get
        {
            return cheat.Get<bool>();
        }
    }

    KVSystem()
    {
        // Hack?
        Instance = this;

        // g for Global
        cheat = KVar("g_cheats", "0", KVarFlags.None, "Enable Cheats");
    }

    KVar KVar(string name, string defaultValue, KVarFlags flag = 0, string help = "") => new KVar(name, defaultValue, flag, help);

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

        // kvar is the first part
        var kvar = parts[0];

        // left parts are just value...
        var value = command.Replace(kvar, "").TrimStart(' ');

        // Check if we are good to go
        if (m_allKvars.ContainsKey(kvar))
        {
            var kVar = m_allKvars[kvar];

            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value))
            {
                if(userInput)
                {
                    Debug.Log($"KVar: {kvar} - {kVar.Get<string>()} (Default: {kVar.Default})");

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
        else
        {
            if (userInput)
            {
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value))
                    Debug.Log($"KVar {kvar} not found, but we would keep this value");
                else
                    Debug.Log($"KVar {kvar} not found.");
            }

            m_wfaValues[kvar] = value;
        }
    }

    public void ReloadConfig()
    {
        if (FS.Instance.Exists("config.cfg"))
        {
            string[] cfg = FS.Instance.ReadString("config.cfg").Replace("\r","").Split('\n');

            cfg.All(line => {
                ExecuteLine(line);
                return true;
            });
        }
    }

    public void SaveConfig()
    {
        var cfg = "// generated by Kirakira Games, do not modify\n";

        foreach(var (name, var) in m_allKvars)
        {
            if(var.IsFlagSet(KVarFlags.Archive))
            {
                cfg = $"{cfg}{name} {var.Get<string>()}\n";
            }
            
        }
            
        var path = Path.Combine(DataLoader.DataDir, "config.cfg");

        if (File.Exists(path))
            File.Delete(path);

        File.WriteAllText(path, cfg);
    }

    public bool Add(KVar var)
    {
        string name = var.Name;
        
        if(m_wfaValues.ContainsKey(name))
        {
            var.Set(m_wfaValues[name]);
            m_wfaValues.Remove(name);
        }

        // This will throws if kvar already exists, do check before you create a kvar!
        m_allKvars.Add(name, var);

        return true;
    }

    public KVar Find(string name)
    {
        if (m_allKvars.ContainsKey(name))
            return m_allKvars[name];

        return null;
    }
}