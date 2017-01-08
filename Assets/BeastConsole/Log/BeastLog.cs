using UnityEngine;
using System;
using System.Collections.Generic;
public static class BeastLog
{
    const bool output_unity_log = true;
#if UNITY_EDITOR
    const bool output_beast_log = false;
#else
    const bool output_beast_log = true;
#endif
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
    public static void Log(object message)
    {
        if (output_unity_log) Debug.Log(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
    }
    public static void Log(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.Log(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
    }
    public static void Error(object message)
    {
        if (output_unity_log) Debug.LogError(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);
    }
    public static void Error(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.LogError(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);
    }
    public static void Warning(object message)
    {
        if (output_unity_log) Debug.LogWarning(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);
    }
    public static void Warning(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.LogWarning(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);
    }
    public static void Success(object message)
    {
        if (output_unity_log) Debug.Log(message);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);
    }
    public static void Success(object message, UnityEngine.Object obj)
    {
        if (output_unity_log) Debug.Log(message, obj);
        if (output_beast_log)
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);
    }
}
