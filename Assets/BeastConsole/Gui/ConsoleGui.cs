namespace BeastConsole.GUI {

    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using BeastConsole.Backend;
    using BeastConsole.Backend.Internal;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    internal class ConsoleGui : MonoBehaviour {

        internal Options m_options;
        [System.Serializable]
        internal class Options {
            public KeyCode ConsoleKey = KeyCode.BackQuote;
            public float TweenTime = 0.4f;
            public int MaxConsoleLines = 120;
            public float LinePadding = 10;
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
        [SerializeField]
        private Transform m_autocompleteRoot;
        [SerializeField]
        private Transform m_autocompleteEntryTemplate;
        [SerializeField]
        private Transform m_historyRoot;
        [SerializeField]
        private GameObject m_historyEntryTemplate;

        internal bool s_showConsole = false;

        private Queue<ConsoleGuiEntry> m_entries = new Queue<ConsoleGuiEntry>();
        private int m_currentEXECUTIONhistoryIndex = 0;
        private ConsoleBackend m_backend;
        private bool m_consoleShown;
        private bool m_inputTargeted;
        private Vector3 m_posTarget;
        private float m_lerpTime;
        private bool m_darkLine;


        internal void Initialize(ConsoleBackend backend, Options options) {
            if (!gameObject.activeSelf) {
                return;
            }
            this.m_backend = backend;
            this.m_options = options;
            m_consoleRoot.anchorMin = new Vector2(0f, 0.65f);
            m_consoleRoot.anchorMax = new Vector2(1f, 1f);
            m_posTarget = new Vector2(0, 10000);
            m_inputField.onValueChanged.AddListener(DrawAutoCompleteSuggestions);
            m_inputField.onEndEdit.AddListener(HandleInput);
            m_backend.OnWriteLine += OnWriteLine;
            m_backend.OnExecutedLine += OnExecutedLine;
            m_backend.RegisterCommand("clear", "clear the console log", this, Clear);
            Console.DestroyChildren(m_consoleContent.transform);
            m_historyRoot.gameObject.SetActive(false);

        }

        private void OnEnable() {
            StartCoroutine(SetScrollBarToZero());
        }

        private void HandleInput(string text) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                if (text.Length == 0)
                    return;
                m_inputField.text = "";
                m_inputField.ActivateInputField();
                m_backend.ExecuteLine(text);
            }
        }

        private void HandleTextInput(string input) {

        }

        private void DrawAutoCompleteSuggestions(string str) {
            if (string.IsNullOrEmpty(str)) {
                if (m_autocompleteRoot.childCount > 0)
                    Console.DestroyChildren(m_autocompleteRoot);
                return;
            }

            Console.DestroyChildren(m_autocompleteRoot);
            var results = m_backend.m_commandsTrie.GetByPrefix(str);

            foreach (var item in results) {
                var go = Instantiate(m_autocompleteEntryTemplate, m_autocompleteRoot, false);
                go.GetComponentInChildren<AutoCompleteGuiEntry>().Initialize(item.Value, this);
                var button = go.GetComponent<Button>();
            }
            int index = 0;
            if (m_autocompleteRoot.childCount > 1) {
                foreach (Transform tr in m_autocompleteRoot) {
                    var button = tr.GetComponent<Button>();
                    var nav = button.navigation;
                    if (index == 0) {
                        nav.selectOnDown = m_autocompleteRoot.GetChild(1).GetComponent<Button>();
                    }
                    else
                        if (index == m_autocompleteRoot.childCount - 1) {
                        nav.selectOnUp = m_autocompleteRoot.GetChild(m_autocompleteRoot.childCount - 2).GetComponent<Button>();
                    }
                    else {
                        nav.selectOnDown = m_autocompleteRoot.GetChild(index + 1).GetComponent<Button>();
                        nav.selectOnUp = m_autocompleteRoot.GetChild(index - 1).GetComponent<Button>();

                    }

                    button.navigation = nav;
                    index++;
                }
            }
            //m_autocompleteRoot.position = GetLocalCaretPosition();
        }

        private Vector2 GetLocalCaretPosition() {
            TextGenerator gen = m_inputField.textComponent.cachedTextGenerator;
            UICharInfo charInfo = gen.characters[m_inputField.caretPosition];
            float x = (charInfo.cursorPos.x + charInfo.charWidth) / m_inputField.textComponent.pixelsPerUnit;
            float y = (charInfo.cursorPos.y) / m_inputField.textComponent.pixelsPerUnit;
            return new Vector2(x, y);
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
            Console.DestroyChildren(m_consoleContent.transform);
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
                    m_consoleRoot.gameObject.SetActive(true);
                    m_inputField.gameObject.SetActive(true);
                    m_autocompleteRoot.gameObject.SetActive(true);
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
                    m_autocompleteRoot.gameObject.SetActive(false);
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
                if (Vector3.Distance(m_consoleRoot.anchoredPosition, new Vector2(0, -m_consoleRoot.rect.y * 2)) < 0.01f)
                    m_consoleRoot.gameObject.SetActive(false);
            }

            if (m_historyRoot.gameObject.activeSelf && !HistoryGuiEntry.s_selected) {
                m_historyRoot.gameObject.SetActive(false);
                SelectInput();

            }
            //if (AutoCompleteGuiEntry.s_selectedCount == 0) {
            //    SelectInput();
            //}
        }

        private void HandleInput() {

            if (Input.GetKeyDown(m_options.ConsoleKey)) {
                s_showConsole = true;
                m_currentEXECUTIONhistoryIndex = m_backend.m_commandHistory.Count - 1;
                m_inputField.text = "";
            }
            else
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                GameObject sel = EventSystem.current.currentSelectedGameObject;
                if (m_inputField.gameObject == sel) {
                    DrawHistory();
                    if (m_historyRoot.childCount > 0)
                        EventSystem.current.SetSelectedGameObject(m_historyRoot.GetChild(m_historyRoot.childCount - 1).gameObject);
                }
                else {


                }

                //if (m_inputField.isFocused && m_backend.m_commandHistory.Count > 0) {
                //    m_currentEXECUTIONhistoryIndex = Mathf.Clamp(m_currentEXECUTIONhistoryIndex, 0, m_backend.m_commandHistory.Count - 1);
                //    m_inputField.text = m_backend.m_commandHistory[m_currentEXECUTIONhistoryIndex];
                //    m_inputField.caretPosition = m_inputField.text.Length;
                //    m_currentEXECUTIONhistoryIndex--;
                //}
            }

            else
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace)) {
                delete_back_to_dot();
            }

            //else
            //if (Input.GetKeyDown(KeyCode.Return)) {
            //    GameObject sel = EventSystem.current.currentSelectedGameObject;
            //    if (m_inputField.gameObject == sel && m_inputField.isFocused)
            //        HandleTextInput(m_inputField.text);
            //    else {
            //        //if (sel) {
            //        //    if (sel.GetComponent<AutoCompleteGuiEntry>()) {
            //        //        m_inputField.text = sel.GetComponentInChildren<Text>().text + " ";
            //        //        SelectInput();
            //        //        m_inputField.ForceLabelUpdate();
            //        //        m_inputField.selectionFocusPosition = 0;
            //        //        m_inputField.caretPosition = m_inputField.text.Length;
            //        //    }
            //        //}
            //    }
            //}

            else
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                GameObject sel = EventSystem.current.currentSelectedGameObject;
                if (m_inputField.gameObject == sel) {
                    if (m_inputField.text != string.Empty) {
                        //SelectInput();
                        EventSystem.current.SetSelectedGameObject(SelectAutoComplete());
                    }
                }
            }

            else
            if (Input.anyKeyDown) {
                SelectInput();
            }
        }

        private GameObject SelectAutoComplete() {
            if (m_autocompleteRoot.childCount > 0) {
                return m_autocompleteRoot.GetChild(0).gameObject;
            }
            else
                return null;
        }

        private void DrawHistory() {

            m_historyRoot.gameObject.SetActive(true);

            Console.DestroyChildren(m_historyRoot);

            var list = m_backend.m_commandHistory;

            int liststart = Mathf.Clamp(list.Count - 20, 0, list.Count);

            for (int i = liststart; i < list.Count; i++) {
                var go = Instantiate(m_historyEntryTemplate, m_historyRoot, false);
                go.GetComponentInChildren<HistoryGuiEntry>().Initialize(list[i], this);
            }
        }

        internal void SelectInput() {
            EventSystem.current.SetSelectedGameObject(m_inputField.gameObject);
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
            entry.Initialize(message, m_options.LinePadding, m_darkLine);
            m_darkLine = !m_darkLine;
            StartCoroutine(SetScrollBarToZero());
        }



        private void OnExecutedLine(string line) {
            m_currentEXECUTIONhistoryIndex = m_backend.m_commandHistory.Count - 1;

        }

        private IEnumerator SetScrollBarToZero() {
            int i = 0;
            while (i < 4) {
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
            Command nearestMatch = m_backend.m_masterDictionary.AutoCompleteLookup(lookup[0]);
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

        private void OnDestroy() {
            StopAllCoroutines();

        }

        internal void SetInputText(string text) {
            m_inputField.text = text;
        }
    }
}