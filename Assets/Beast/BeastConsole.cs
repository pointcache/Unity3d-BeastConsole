
namespace BeastConsole
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using System.Collections;

    public class BeastConsole : MonoBehaviour
    {
        string cfgpath { get { return Application.dataPath + "/" + configOptions.config_path; } }


        #region SINGLETON
        private static BeastConsole _instance;
        public static BeastConsole instance
        {
            get
            {
                if (!_instance) _instance = GameObject.FindObjectOfType<BeastConsole>();
                return _instance;
            }
        }
        #endregion

        [Header("Console")]
        public SmartConsole.Options consoleOptions;
        [Header("Config")]
        public ConfigOptions configOptions;
        GameObject consoleRoot;
        void OnEnable()
        {
            var evsys = GameObject.FindObjectOfType<EventSystem>();
            if (!evsys)
            {
                Debug.LogError("UnityEvent System not found in scene, manually add it.");
                Debug.Break();
            }
            GameObject prefab = Resources.Load<GameObject>("BeastConsole/BeastConsole");
            consoleRoot = GameObject.Instantiate(prefab);
            consoleRoot.transform.SetParent(transform);
            SmartConsole.options = consoleOptions;
            SmartConsole.entryTemplate = Resources.Load<GameObject>("BeastConsole/ConsoleEntry");
            SmartConsole.consoleContent = consoleRoot.transform.FindDeepChild("Content").gameObject;
            SmartConsole.consoleRoot = consoleRoot.transform.FindDeepChild("Root").GetComponent<RectTransform>();
            SmartConsole.inputField = consoleRoot.transform.FindDeepChild("InputField").GetComponent<InputField>();
            SmartConsole.scrollBar = consoleRoot.transform.FindDeepChild("Scrollbar Vertical").GetComponent<Scrollbar>();
            consoleRoot.AddComponent<SmartConsole>();
        }

        private void OnDisable()
        {
            SmartConsole.Destroy();
            Destroy(consoleRoot.gameObject);
        }

        [Serializable]
        public class ConfigOptions
        {
            public string config_path = "game.cfg";
        }

        public void SaveConfigs()
        {
            ConfigSystem.Save(cfgpath);
        }

        public void LoadConfigs()
        {
            ConfigSystem.Load(cfgpath);
        }
    }
    public static class TransformDeepChildExtension
    {
        //Breadth-first search
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }


    }
}