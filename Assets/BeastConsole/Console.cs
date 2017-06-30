namespace BeastConsole
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using System.Collections;
    using BeastConsole.GUI;
    using BeastConsole.Backend;

    public class Console : MonoBehaviour
    {
        private static Console _instance;
        public static Console instance
        {
            get
            {
                if (!_instance) _instance = GameObject.FindObjectOfType<Console>();
                return _instance;
            }
        }

        [Header("Console")]
        public ConsoleGui.Options consoleOptions;

        private GameObject consoleRoot;
        private ConsoleGui gui;
        private ConsoleBackend backend;

        private void Awake()
        {
            var evsys = GameObject.FindObjectOfType<EventSystem>();
            if (!evsys)
            {
                Debug.LogError("UnityEvent System not found in scene, manually add it.");
                Debug.Break();
            }
            GameObject prefab = Resources.Load<GameObject>("BeastConsole/ConsoleGui");
            consoleRoot = GameObject.Instantiate(prefab);
            consoleRoot.transform.SetParent(transform);

            backend = new ConsoleBackend();
            ConsoleGui gui = consoleRoot.GetComponentInChildren<ConsoleGui>();
            gui.Initialize(backend);
        }
       
        private void OnDisable()
        {
            Destroy(consoleRoot.gameObject);
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