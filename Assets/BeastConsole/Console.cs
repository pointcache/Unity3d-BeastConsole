namespace BeastConsole {

    using UnityEngine;
    using System;
    using UnityEngine.EventSystems;
    using BeastConsole.GUI;
    using BeastConsole.Backend;
    using System.Collections.Generic;

    public class Console : MonoBehaviour {
        private static Console _instance;
        public static Console instance
        {
            get {
                if (!_instance)
                    _instance = GameObject.FindObjectOfType<Console>();
                return _instance;
            }
        }

        [SerializeField]
        internal ConsoleGui.Options consoleOptions;

        private GameObject m_consoleRoot;
        private ConsoleGui m_gui;
        private ConsoleBackend m_backend;


        private void Awake() {
            var evsys = GameObject.FindObjectOfType<EventSystem>();
            if (!evsys) {
                Debug.LogError("UnityEvent System not found in scene, manually add it.");
                Debug.Break();
            }
            DestroyChildren(transform);
            GameObject prefab = Resources.Load<GameObject>("BeastConsole/ConsoleGui");
            m_consoleRoot = GameObject.Instantiate(prefab);
            m_consoleRoot.transform.SetParent(transform);

            m_backend = new ConsoleBackend();
            ConsoleGui gui = m_consoleRoot.GetComponentInChildren<ConsoleGui>();
            gui.Initialize(m_backend, consoleOptions);
        }

        public static void RegisterCommand(string name, string description, object owner, Action<string[]> callback) {
            instance.m_backend.RegisterCommand(name, description, owner, callback);
        }

        public static void UnregisterCommand(string name, object owner) {
            if (instance != null && instance.m_backend != null)
                instance.m_backend.RemoveCommandIfExists(name, owner);
        }
        public static void RegisterVariable<T>(string name, string description, Action<T> setter, object owner) {
            instance.m_backend.RegisterVariable<T>(setter, owner, name, description);
        }

        public static void UnregisterVariable<T>(string name, object owner) {
            if (instance != null && instance.m_backend != null)
                instance.m_backend.UnregisterVariable<T>(name, owner);
        }

        /// <summary>
        /// Directly execute command
        /// </summary>
        /// <param name="line"></param>
        public static void ExecuteLine(string line) {
            if (instance != null && instance.m_backend != null)
                instance.m_backend.ExecuteLine(line);
        }

        private void OnDisable() {
            Destroy(m_consoleRoot.gameObject);
        }

        /// <summary>
        /// Supports rich text.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message) {
            instance.m_backend.WriteLine(message);
        }

        //Breadth-first search
        internal static Transform FindDeepChild(Transform aParent, string aName) {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent) {
                result = FindDeepChild(child, aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        internal static void DestroyChildren(Transform tr) {
            List<Transform> list = new List<Transform>();
            foreach (Transform child in tr) {
                list.Add(child);
            }
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                GameObject.Destroy(list[i].gameObject);
            }
        }
    }
}