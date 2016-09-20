using UnityEngine;
using System;
using System.Collections.Generic;

public static class SLog
{
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
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.Log(message);
        else 
        SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
    }

    public static void print(object message, UnityEngine.Object obj)
    {
        
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.Log(message, obj);
        else
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
    }

    public static void error(object message)
    {
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.LogError(message);
        else
        SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);

    }

    public static void error(object message, UnityEngine.Object obj)
    {
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.LogError(message, obj);
        else
        SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);

    }

    public static void warning(object message)
    {
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.LogWarning(message);
        else
        SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);


    }

    public static void warning(object message, UnityEngine.Object obj)
    {
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.LogWarning(message, obj);
        else
        SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);


    }


    public static void confirm(object message)
    {
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.Log(message);
        else
        SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);

    }

    public static void confirm(object message, UnityEngine.Object obj)
    {
        if ( CFG.output_unity_log == null || CFG.output_unity_log) Debug.Log(message, obj);
        else
        SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);

    }
}
