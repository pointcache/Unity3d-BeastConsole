

namespace BeastConsole
{

    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using System.Collections.Generic;
    using rvar;

    public static class ConfigSystem
    {
        static Dictionary<string, IrVar> current = new Dictionary<string, IrVar>();
        static Dictionary<Type, List<string>> cached = new Dictionary<Type, List<string>>();

        public static void Save(string path)
        {
            SerializationHelper.Serialize(current, path, true);
        }

        public static void Load(string path)
        {
            var dict = SerializationHelper.Load<Dictionary<string, IrVar>>(path);

            foreach (var item in current)
            {
                IrVar cur = item.Value;

                IrVar loaded = null;
                dict.TryGetValue(item.Key, out loaded);
                if (loaded == null)
                    continue;
                cur.setValue(loaded.getValue());
            }
        }

        public static void RegisterConfig(ConfigBase cfg)
        {
            Type t = cfg.GetType();
            var fields = t.GetFields();
            if (fields.Length > 0)
            {
                List<string> cache = null;

                cached.TryGetValue(t, out cache);

                if (cache == null)
                {
                    cache = new List<string>();
                    cached.Add(t, cache);
                }

                foreach (var f in fields)
                {
                    if (f.FieldType.BaseType.BaseType == typeof(rVar))
                    {
                        var attr = f.GetCustomAttributes(typeof(ConfigVarAttribute), false);
                        if (attr.Length == 1)
                        {
                            ConfigVarAttribute cattr = attr[0] as ConfigVarAttribute;

                            if (current.ContainsKey(cattr.Name))
                            {
                                Debug.LogError("<color=red> You can't have config variables with same name. </color>");
                                Debug.LogError("Variable in class " + t.Name + " with name " + cattr.Name);
                                continue;
                            }
                            IrVar ivar = f.GetValue(cfg) as IrVar;
                            current.Add(cattr.Name, ivar);
                            cache.Add(cattr.Name);

                            if (cattr.Console)
                            {
                                ivar.registerInConsole(cattr.Name, cattr.Description);
                            }
                        }
                    }
                }
            }
        }

        public static void UnregisterConfig(ConfigBase cfg)
        {
            Type t = cfg.GetType();

            List<string> cache = null;
            cached.TryGetValue(t, out cache);

            foreach (var s in cache)
            {
                IrVar rv = null;
                current.TryGetValue(s, out rv);
                if (rv != null)
                {
                    rv.unregisterInConsole(s);
                    current.Remove(s);
                }
                else
                    Debug.Log("When unregistering config, config variable " + s + " was not found is the list of tracked variables.");
            }
        }
    }

    public class ConfigVarAttribute : Attribute
    {
        public string Name;
        public string Description;
        public bool Console;

        public ConfigVarAttribute(string name)
        {
            Name = name;
            Description = "nondescript";
            Console = true;
        }

        public ConfigVarAttribute(string name, bool console)
        {
            Name = name;
            Description = "nondescript";
            Console = console;
        }

        public ConfigVarAttribute(string name, string description)
        {
            Name = name;
            Description = description;
            Console = true;
        }
        public ConfigVarAttribute(string name, string description, bool console)
        {
            Name = name;
            Description = description;
            Console = console;
        }


    }
}