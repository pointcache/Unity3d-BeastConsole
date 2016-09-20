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
    
    public interface DynaVarAccess
    {
        void Initialize();
        string GetToString();

    }
    static Dictionary<string, VariableBase> vars = new Dictionary<string, VariableBase>();
    static Dictionary<string, List<VariableBase>> groups = new Dictionary<string, List<VariableBase>>();
    static public Dictionary<string, List<SettingsVar>> groupsSettings = new Dictionary<string, List<SettingsVar>>();
    static List<DynaVarAccess> initializationList = new List<DynaVarAccess>();


    public class VariableBase
    {
        protected string name;
        public string Fullname;
        public string NiceName;
        public string Group;
        public object Value;
        public bool StartupInitialization;
        public bool Console;
        public Type type;

        /// <summary>
        /// Hack to allow subscribing to variables of unknown type
        /// Does the same as "Subscribe()"
        /// </summary>
        public Action OnChangedBase;

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class Variable<T> : VariableBase, DynaVarAccess where T : new()
    {
        Action<T> OnChanged;
        private T value;
        public T initVal;

        public Variable(string name)
        {
            Initialise(name, "", new T(), null);
        }

        public Variable(string name, T initialValue)
        {
            Initialise(name, "", initialValue, null);
        }

        public Variable(string name, string niceName, T initialValue, Action<T> callback)
        {
            Initialise(name, niceName, initialValue, callback);
        }

        public Variable<T> Set(T val)
        {
            value = val;
            Value = val;
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
            Value = val;
            return this;
        }

        public static implicit operator T(Variable<T> var)
        {
            return var.value;
        }

        private void Initialise(string name, string niceName, T initalValue, Action<T> callback)
        {
            type = typeof(T);
            Fullname = name;
            base.name = name;
            NiceName = niceName;
            value = initalValue;
            OnChanged = callback;
            initVal = initalValue;
            Value = initalValue;
        }

        public Variable<T> SetFromString(string value)
        {
            Set((T)System.Convert.ChangeType(value, typeof(T)));
            return this;
        }

        public void Initialize()
        {
           Set(initVal);
        }

        public string GetToString()
        {
            return value.ToString();
        }

        public Variable<T> ClearSubscribers()
        {
            OnChanged = null;
            if (Console)
                SmartConsole.ClearVarCallbacks<T>(name);
            return this;
        }

        public Variable<T> Subscribe(Action<T> callback)
        {
            OnChanged += callback;
            if(Console)
                SmartConsole.AddVarCallback(name, callback);
            return this;
        }

        public Variable<T> Unsubscribe(Action<T> callback)
        {
            OnChanged -= callback;
            if (Console)
                SmartConsole.RemoveVarCallback(name, callback);
            return this;
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

    public static Variable<T> NewVar<T>(string group, string name, string niceName, string description, Action<T> callback, bool StartupInit, bool console) where T : new()
    {
        if (group != "")
            name = group + "." + name;
        
        if (vars.ContainsKey(name))
        {
            Debug.LogError("InternalConfig: Attempt to register existing variable, aborting.");
            Debug.Break();
            return null;
        }
        Variable<T> val = new Variable<T>(name, niceName,  callback);

        if (console)
            SmartConsole.CreateVariable(name, description,  callback);
        val.Console = console;
        val.Group = group;
        if (!groups.ContainsKey(group))
        {
            List<VariableBase> list = new List<VariableBase>();
            groups.Add(group, list);
            list.Add(val);
        }
        else
        {
            groups[group].Add(val);
        }
        vars.Add(name, val);
        if (StartupInit)
        {
            val.StartupInitialization = StartupInit;
            initializationList.Add(val);
        }

        return val;
    }


    static void NewSettingsVar(VariableBase varb, float min, float max)
    {
        string group = varb.Group;
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

    public static void StartupInitialization()
    {
        int c = initializationList.Count;
        for (int i = 0; i < c; i++)
        {
            initializationList[i].Initialize();
        }
    }
}