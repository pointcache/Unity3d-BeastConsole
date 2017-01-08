
namespace BeastConsole
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;




    public static class BeastLog
    {
        const bool output_unity_log = true;
#if !UNITY_EDITOR
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
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
#endif
        }
        public static void Log(object message, UnityEngine.Object obj)
        {
            if (output_unity_log) Debug.Log(message, obj);
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.log);
#endif
        }
        public static void Error(object message)
        {
            if (output_unity_log) Debug.LogError(message);
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);
#endif
        }
        public static void Error(object message, UnityEngine.Object obj)
        {
            if (output_unity_log) Debug.LogError(message, obj);
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.error);
#endif
        }
        public static void Warning(object message)
        {
            if (output_unity_log) Debug.LogWarning(message);
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);
#endif
        }
        public static void Warning(object message, UnityEngine.Object obj)
        {
            if (output_unity_log) Debug.LogWarning(message, obj);
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.warning);
#endif
        }
        public static void Success(object message)
        {
            if (output_unity_log) Debug.Log(message);
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);
#endif
        }
        public static void Success(object message, UnityEngine.Object obj)
        {
            if (output_unity_log) Debug.Log(message, obj);
#if !UNITY_EDITOR
            SmartConsole.Log(message.ToString(), SmartConsole.myLogType.confirmation);
#endif
        }
    }
}