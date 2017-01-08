using UnityEngine;
using System.Collections;
using FullSerializer;
using System.IO;
using System;

public static class SerializationHelper
{
    public static bool Serialize(object obj, string path, bool beautify)
    {
        fsSerializer _serializer = new fsSerializer();
        fsData data;
        _serializer.TrySerialize(obj, out data).AssertSuccessWithoutWarnings();
        StreamWriter sw = new StreamWriter(path);
        switch (beautify)
        {
            case true:
                sw.Write(fsJsonPrinter.PrettyJson(data));
                break;
            case false:
                sw.Write(fsJsonPrinter.CompressedJson(data));
                break;
        }

        sw.Close();
        return true;
    }

    public static object Deserialize(Type type, string serializedState)
    {
        fsSerializer _serializer = new fsSerializer();
        // step 1: parse the JSON data
        fsData data = fsJsonParser.Parse(serializedState);

        // step 2: deserialize the data
        object deserialized = null;
        _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

        return deserialized;
    }

    public static T Load<T>(string path)
    {
        StreamReader sr = new StreamReader(path);
        string data = sr.ReadToEnd();
        sr.Close();
        return (T)Deserialize(typeof(T), data);
    }

    public static T LoadFromString<T>(string json)
    {
        return (T)Deserialize(typeof(T), json);
    }
}