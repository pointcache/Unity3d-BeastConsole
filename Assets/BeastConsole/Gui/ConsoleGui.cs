namespace BeastConsole.GUI {

    using System.Collections;
    using System.Collections.Generic;
    using BeastConsole.Backend;
    using BeastConsole.Backend.Internal;
    using UnityEngine;
    using UnityEngine.UI;

    internal class ConsoleGui : MonoBehaviour {

        internal Options m_options;
        [System.Serializable]
        internal class Options {
            public KeyCode ConsoleKey = KeyCode.BackQuote;
            public float TweenTime = 0.4f;
            public int MaxConsoleLines = 120;
        }

        [SerializeField]
        private GameObject m_entryTemplate;
        [SerializeField]
        private GameObject m_consoleContent;
        [SerializeField]
        private RectTransform m_consoleRoot;
        [SerializeField]
        private InputField m_inputField;
        [SerializeField]
        private Scrollbar m_scrollBar;

        internal bool s_showConsole = false;

        private Queue<ConsoleGuiEntry> m_entries = new Queue<ConsoleGuiEntry>();
        private int m_currentEXECUTIONhistoryIndex = 0;
        private ConsoleBackend m_backend;
        private bool m_consoleShown;
        private bool m_inputTargeted;
        private Vector3 m_posTarget;
        private float m_lerpTime;

        internal void Initialize(ConsoleBackend backend, Options options) {
            if (!gameObject.activeSelf) {
                return;
            }
            this.m_backend = backend;
            this.m_options = options;
            m_consoleRoot.anchorMin = new Vector2(0f, 0.65f);
            m_consoleRoot.anchorMax = new Vector2(1f, 1f);
            m_posTarget = new Vector2(0, 10000);
            m_inputField.onEndEdit.AddListener(delegate { HandleTextInput(m_inputField.text); });
            backend.OnWriteLine += OnWriteLine;
            backend.OnExecutedLine += OnExecutedLine;
            backend.RegisterCommand("clear", "clear the console log",this, Clear);
        }

                /// <summary>
        /// Clears out the console log
        /// </summary>
        /// <example> 
        /// <code>
        /// SmartConsole.Clear();
        /// </code>
        /// </example>
        internal void Clear(string[] parameters) {
            //we dont want to clear our history, instead we clear the screen
            //s_outputHistory.Clear();
            DestroyChildren(m_consoleContent.transform);
        }

        private float Remap(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        private void Update() {
            if (!gameObject.activeSelf) {
                return;
            }
            HandleInput();
            if (s_showConsole) {
                if (!m_consoleShown) {
                    m_inputField.gameObject.SetActive(true);
                    if (!m_inputTargeted) {
                        m_inputField.Select();
                        m_inputField.ActivateInputField();
                        m_inputTargeted = true;
                    }
                    m_posTarget = Vector2.zero;
                    m_lerpTime = 0;
                    m_consoleShown = true;
                }
                else {
                    m_inputField.text = "";
                    m_inputField.gameObject.SetActive(false);
                    m_inputTargeted = false;
                    m_posTarget = new Vector2(0, -m_consoleRoot.rect.y * 2);
                    m_lerpTime = 0;
                    m_scrollBar.value = 0;
                    m_consoleShown = false;
                }
            }
            if (m_inputField.isFocused) {
                if (Input.GetKeyDown(KeyCode.Tab)) {
                    AutoComplete(m_inputField.text);
                }
            }
            s_showConsole = false;
            if (m_lerpTime < m_options.TweenTime - 0.01f) {
                m_lerpTime += Time.deltaTime;
                m_lerpTime = Mathf.Clamp(m_lerpTime, 0f, m_options.TweenTime);
                m_consoleRoot.anchoredPosition = Vector3.Lerp(m_consoleRoot.anchoredPosition, m_posTarget, Remap(m_lerpTime, 0f, m_options.TweenTime, 0f, 1f));
            }
        }

        private void HandleInput() {
            if (Input.GetKeyDown(m_options.ConsoleKey)) {
                s_showConsole = true;
                m_currentEXECUTIONhistoryIndex = m_backend.s_commandHistory.Count - 1;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                if (m_backend.s_commandHistory.Count > 0) {
                    m_currentEXECUTIONhistoryIndex = Mathf.Clamp(m_currentEXECUTIONhistoryIndex, 0, m_backend.s_commandHistory.Count - 1);
                    m_inputField.text = m_backend.s_commandHistory[m_currentEXECUTIONhistoryIndex];
                    m_inputField.caretPosition = m_inputField.text.Length;
                    m_currentEXECUTIONhistoryIndex--;
                }
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace)) {
                delete_back_to_dot();
            }
        }

        internal void OnWriteLine(string message) {
            ConsoleGuiEntry entry = null;
            if (m_entries.Count > m_options.MaxConsoleLines) {
                entry = m_entries.Dequeue().GetComponent<ConsoleGuiEntry>();
                m_entries.Enqueue(entry);
                //OffsetAllEntriesOnce();
                entry.transform.SetAsLastSibling();
            }
            else {
                entry = Instantiate(m_entryTemplate).GetComponent<ConsoleGuiEntry>();
                m_entries.Enqueue(entry);
                entry.transform.SetParent(m_consoleContent.transform, true);
                entry.transform.SetAsLastSibling();
            }
            entry.Clear();
            entry.SetText(message);
            StartCoroutine(SetScrollBarToZero());
            //s_currentCommandHistoryIndex = s_outputHistory.Count - 1;
        }

        private void HandleTextInput(string input) {
            if (input.Length == 0)
                return;
            m_inputField.text = "";
            m_inputField.ActivateInputField();
            m_backend.ExecuteLine(input);
        }

        private void OnExecutedLine(string line) {
            m_currentEXECUTIONhistoryIndex = m_backend.s_commandHistory.Count - 1;

        }

        private IEnumerator SetScrollBarToZero() {
            int i = 0;
            while (i < 2) {
                i++;
                m_scrollBar.value = 0;
                yield return null;
            }
            m_scrollBar.value = 0;
            yield break;
        }

        private void delete_back_to_dot() {
            string text = m_inputField.text;
            if (text.Contains(".")) {
                int index = text.LastIndexOf('.');
                int length = text.Length - index;
                text = text.Remove(index + 1, length - 1);
            }
            else {
                text = "";
            }
            m_inputField.text = text;
        }

        private void AutoComplete(string input) {
            string[] lookup = m_backend.CComParameterSplit(input);
            if (lookup.Length == 0) {
                // don't auto complete if we have typed any parameters so far or nothing at all...
                return;
            }
            Command nearestMatch = m_backend.s_masterDictionary.AutoCompleteLookup(lookup[0]);
            // only complete to the next dot if there is one present in the completion string which
            // we don't already have in the lookup string
            int dotIndex = 0;
            do {
                dotIndex = nearestMatch.m_name.IndexOf(".", dotIndex + 1);
            }
            while ((dotIndex > 0) && (dotIndex < lookup[0].Length));
            string insertion = nearestMatch.m_name;
            if (dotIndex >= 0) {
                insertion = nearestMatch.m_name.Substring(0, dotIndex + 1);
            }
            if (insertion.Length < input.Length) {
                do {
                    if (AutoCompleteTailString("true", input))
                        break;
                    if (AutoCompleteTailString("false", input))
                        break;
                    if (AutoCompleteTailString("True", input))
                        break;
                    if (AutoCompleteTailString("False", input))
                        break;
                    if (AutoCompleteTailString("TRUE", input))
                        break;
                    if (AutoCompleteTailString("FALSE", input))
                        break;
                }
                while (false);
            }
            else if (insertion.Length >= input.Length) // SE - is this really correct?
            {
                m_inputField.text = insertion;
            }
            if (insertion[insertion.Length - 1] != '.')
                m_inputField.text = insertion + " ";
            m_inputField.caretPosition = m_inputField.text.Length;
        }

        private bool AutoCompleteTailString(string tailString, string input) {
            for (int i = 1; i < tailString.Length; ++i) {
                if (input.EndsWith(" " + tailString.Substring(0, i))) {
                    m_inputField.text = input.Substring(0, input.Length - 1) + tailString.Substring(i - 1);
                    return true;
                }
            }
            return false;
        }

        private void DestroyChildren(Transform tr) {
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