using UnityEngine;
using System;
using System.Collections.Generic;
using FullSerializer;
using System.Linq;

[Serializable]
public class rVar
{
}

public interface IrVar
{
    object getValue();
    void setValue(object val);
    void registerInConsole(string name, string description);
    void unregisterInConsole(string name);
}

[Serializable]
public class rVar<T> : rVar, IrVar
{
    [SerializeField]
    [HideInInspector]
    T value;
    [fsProperty]
    public T Value
    {
        get { return value; }
        set
        {
            this.value = value;
            OnChanged(value);
        }
    }
    public rVar()
    {
    }
    public rVar(T initval)
    {
        Value = initval;
    }
    public event Action<T> OnChanged = delegate { };

    public override string ToString()
    {
        return value.ToString();
    }

    public object getValue()
    {
        return Value;
    }

    public void setValue(object val)
    {
        Value = (T)val;
    }

    public void registerInConsole(string name, string description)
    {
        SmartConsole.RegisterVariable(this, name, description);
    }

    public void unregisterInConsole(string name)
    {
        SmartConsole.UnregisterVariable(name);
    }

    public rVar<T> SetFromString(string value)
    {
        Value = ((T)System.Convert.ChangeType(value, typeof(T)));
        return this;
    }
}

[Serializable]
public class r_int : rVar<int>
{
    public r_int() : base() { }
    public r_int(int initialValue) : base(initialValue) { }
    public static implicit operator int(r_int var)
    {
        return var.Value;
    }
}
[Serializable]
public class r_float : rVar<float>
{
    public r_float() : base() { }
    public r_float(float initialValue) : base(initialValue) { }
    public static implicit operator float(r_float var) { return var.Value; }
}

[Serializable]
public class r_string : rVar<string>
{
    public r_string() : base() { }
    public r_string(string initialValue) : base(initialValue) { }
    public static implicit operator string(r_string var)
    {
        return var.Value;
    }

}
[Serializable]
public class r_double : rVar<double>
{
    public r_double() : base() { }
    public r_double(double initialValue) : base(initialValue) { }
    public static implicit operator double(r_double var)
    {
        return var.Value;
    }
}
[Serializable]
public class r_bool : rVar<bool>
{
    public r_bool() : base() { }
    public r_bool(bool initialValue) : base(initialValue) { }
    public static implicit operator bool(r_bool var)
    {
        return var.Value;
    }
}

[Serializable]
public class r_KeyCode : rVar<KeyCode>
{
    public r_KeyCode() : base() { }
    public r_KeyCode(KeyCode initialValue) : base(initialValue) { }
    public static implicit operator KeyCode(r_KeyCode var)
    {
        return var.Value;
    }
}

[Serializable]
public class r_Color : rVar<Color>
{
    public r_Color() : base() { }
    public r_Color(Color initialValue) : base(initialValue) { }
    public static implicit operator Color(r_Color var)
    {
        return var.Value;
    }
}
