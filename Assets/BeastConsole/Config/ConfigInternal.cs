using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// Object to expose options in unity editor
/// </summary>
/// 
public static partial class CFG
{
    //All initialization of variables
    static CFG()
    {
        reg_GFX();
        reg_PLAYER();
        reg_GAME();
        reg_MISC();
        reg_UI();
        reg_AUDIO();
        reg_CONTROLS();
    }

        /// <summary>
    /// Used to create a variable outside of CFG static vars
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="group"></param>
    /// <param name="name"></param>
    /// <param name="initval"></param>
    public static Variable<T> NEW_EXTERNAL_VAR<T>(string group, string name, T initval) where T : new()
    {
        var VAR = new Variable<T>(group, name, "", false);
        VAR.SetSilent(initval);
        return VAR;
    }

    public const string AudioGroup = "sound";
    public const string GraphicsGroup = "gfx";
    public const string AppGroup = "app";
    public const string ControlsGroup = "ctrl";
    public const string PlayerGroup = "player";
    public const string UiGroup = "ui";
    
    static Dictionary<string, VariableBase> vars = new Dictionary<string, VariableBase>();
    static Dictionary<string, List<VariableBase>> groups = new Dictionary<string, List<VariableBase>>();
    static public Dictionary<string, List<SettingsVar>> groupsSettings = new Dictionary<string, List<SettingsVar>>();
    
    public class VariableBase
    {
        public string name;
        public string description;
        public string group;
        /// <summary>
        /// This allows to access the value as object from base type, for flexibility
        /// </summary>
        public object baseValue;
        public bool console;
        public Type type;

        /// <summary>
        /// Hack to allow subscribing to variables of unknown type
        /// Does the same as "Subscribe()"
        /// </summary>
        public Action OnChangedBase;

        public override string ToString()
        {
            return baseValue.ToString();
        }
    }

    public class Variable<T> : VariableBase where T : new()
    {
        public event Action<T> OnChanged;
        public T value;

        public Variable(string _group, string _name, string _description, bool _console)
        {
            group = _group;
            if (group != "")
                name = _group + "." + name;
            description = _description;
            if (vars.ContainsKey(name))
            {
                Debug.LogError("InternalConfig: Attempt to register existing variable, aborting.");
                Debug.Break();
            }
            type = typeof(T);
            console = _console;
            value = new T();
            baseValue = value;
            if (console)
                SmartConsole.CreateVariable(this);
            
            
            if (!groups.ContainsKey(group))
            {
                List<VariableBase> list = new List<VariableBase>();
                groups.Add(group, list);
                list.Add(this);
            }
            else
            {
                groups[group].Add(this);
            }
            vars.Add(name, this);
        }


        public Variable<T> Set(T val)
        {
            value = val;
            baseValue = val;
            if (OnChanged != null)
                OnChanged(val);
            if (OnChangedBase != null)
                OnChangedBase();
            return this;
        }
        /// <summary>
        /// Will not raise notification
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public Variable<T> SetSilent(T val)
        {
            value = val;
            baseValue = val;
            return this;
        }

        public static implicit operator T(Variable<T> var)
        {
            return var.value;
        }

        public Variable<T> SetFromString(string value)
        {
            Set((T)System.Convert.ChangeType(value, typeof(T)));
            return this;
        }

        public string GetToString()
        {
            return value.ToString();
        }


        /// <summary>
        /// Will raise event for all subscribers
        /// </summary>
        public Variable<T> Update()
        {
            Set(value);
            return this;
        }
    }

    public static VariableBase GetVar(string name)
    {
        if (vars.ContainsKey(name))
            return vars[name];
        else
            return null;
    }

    public static void SetVar<T>(string name, T val) where T : new()
    {

        if (!vars.ContainsKey(name))
        {
            Debug.LogError("InternalConfig: trying to set unregistered variable " + name);
            return;
        }

        Variable<T> _var = vars[name] as Variable<T>;
        _var.Set(val);
    }

    public static void SetVar<T>(string name, string val) where T : new()
    {
        if (!vars.ContainsKey(name))
        {
            Debug.LogError("InternalConfig: trying to set unregistered variable " + name);
            return;
        }

        Variable<T> _var = vars[name] as Variable<T>;
        _var.SetFromString(val);
    }

    public class SettingsVar
    {
        public VariableBase mainVar;
        public float min, max;
        public ControlType controlType { get; private set; }
        public enum ControlType
        {
            Fslider,
            Islider,
            toggle,
            input,
            keypress,
            dropdown
        }

        public SettingsVar(VariableBase varB)
        {
            mainVar = varB;

            if (varB.type == typeof(float))
            { controlType = ControlType.Fslider; }
            else if (varB.type == typeof(int))
            { controlType = ControlType.Islider; }
            else if (varB.type == typeof(bool))
            { controlType = ControlType.toggle; }
        }
    }


    static void NewSettingsVar(VariableBase varb, float min, float max)
    {
        string group = varb.group;
        SettingsVar svar = null;
        if (!groupsSettings.ContainsKey(group))
        {
            List<SettingsVar> list = new List<SettingsVar>();
            groupsSettings.Add(group, list);
            svar = new SettingsVar(varb);
            list.Add(svar);
        }
        else
        {
            svar = new SettingsVar(varb);
            groupsSettings[group].Add(svar);
        }
        svar.min = min;
        svar.max = max;
    }

}