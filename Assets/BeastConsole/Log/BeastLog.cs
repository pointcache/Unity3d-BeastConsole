using UnityEngine;
using System;
using System.Collections.Generic;
public static class BeastLog
{
    const bool output_unity_log = true;
    const bool output_beast_log = false;
    const string idcol = "<color=#1676d0>>id:";
    const string objnamecol = "<color=white>: ";
    public static string id(this object obj)
    {
        return idcol + obj.GetHashCode() + "</color>";
    }
    public static string log(this object obj)
    {
        return objnamecol + obj.ToString() + "</color>" + idcol + obj.GetHashCode() + "</color>";
    }
    public static void print(object message)
    {
        if (output_unity_log) Debug.Log(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
    }
    public static void print(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.Log(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
    }
    public static void error(object message)
    {
        if (output_unity_log) Debug.LogError(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);
    }
    public static void error(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.LogError(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);
    }
    public static void warning(object message)
    {
        if (output_unity_log) Debug.LogWarning(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);
    }
    public static void warning(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.LogWarning(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);
    }
    public static void confirm(object message)
    {
        if (output_unity_log) Debug.Log(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);
    }
    public static void confirm(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.Log(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);
    }
}
