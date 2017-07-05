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
        [SerializeField]
        private GameObject m_helpBox;

        private bool m_beastConsoleHelp
        {
            get { return PlayerPrefs.GetInt("BC_BeastConsoleShowHelp") == 1 ? true : false; }
            set {
                m_helpBox.SetActive(value);
                PlayerPrefs.SetInt("BC_BeastConsoleShowHelp", value ? 1 : 0);
            }
        }

        internal bool s_showConsole = false;
        internal static bool NavigationAllowed
        {
            get {
                return m_eventSystem.sendNavigationEvents;
            }
            set {
                m_eventSystem.sendNavigationEvents = value;
            }
        }

        private Queue<ConsoleGuiEntry> m_entries = new Queue<ConsoleGuiEntry>();
        private int m_currentEXECUTIONhistoryIndex = 0;
        private ConsoleBackend m_backend;
        private bool m_consoleShown;
        private bool m_inputTargeted;
        private Vector3 m_posTarget;
        private float m_lerpTime;
        private bool m_darkLine;
        private bool m_inputactive;
        private static EventSystem m_eventSystem;
        private GameObject m_selected;
        private ConsoleGuiEntry m_lastEntry;


        internal void Initialize(ConsoleBackend backend, Options options) {
            if (!gameObject.activeSelf) {
                return;
            }

            m_eventSystem = GameObject.FindObjectOfType<EventSystem>();
            this.m_backend = backend;
            this.m_options = options;
            m_consoleRoot.anchorMin = new Vector2(0f, 0.65f);
            m_consoleRoot.anchorMax = new Vector2(1f, 1f);
            m_posTarget = new Vector2(0, 10000);
            m_inputField.onValueChanged.AddListener(DrawAutoCompleteSuggestions);
            m_inputField.onEndEdit.AddListener(HandleInput);
            m_backend.OnWriteLine += OnWriteLine;
            m_backend.OnExecutedCommand += OnExecutedLine;
            m_backend.RegisterCommand("clear", "clear the console log", this, Clear);
            Console.DestroyChildren(m_consoleContent.transform);
            m_historyRoot.gameObject.SetActive(false);
            m_beastConsoleHelp = m_beastConsoleHelp;

            Console.AddVariable<bool>("console.showHelp", "Shows the info box in the console", x => m_beastConsoleHelp = x, this);
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
            SetupNavigation(m_autocompleteRoot);
        }

        private void SetupNavigation(Transform transform) {
            int index = 0;
            if (transform.childCount > 1) {
                foreach (Transform tr in transform) {
                    var button = tr.GetComponent<Selectable>();
                    var nav = button.navigation;
                    if (index == 0) {
                        nav.selectOnDown = transform.GetChild(1).GetComponent<Selectable>();
                    }
                    else
                        if (index == transform.childCount - 1) {
                        nav.selectOnUp = transform.GetChild(transform.childCount - 2).GetComponent<Selectable>();
                    }
                    else {
                        nav.selectOnDown = transform.GetChild(index + 1).GetComponent<Selectable>();
                        nav.selectOnUp = transform.GetChild(index - 1).GetComponent<Selectable>();
                    }

                    button.navigation = nav;
                    index++;
                }
            }
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
                    NavigationAllowed = false;

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
                    NavigationAllowed = true;
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
        }

        private void HandleInput() {

            m_selected = m_eventSystem.currentSelectedGameObject;
            m_inputactive = m_inputField.gameObject == m_selected && m_inputField.isFocused;

            if (Input.GetKeyDown(m_options.ConsoleKey)) {
                s_showConsole = true;
                m_currentEXECUTIONhistoryIndex = m_backend.m_commandHistory.Count - 1;
                m_inputField.text = "";
            }
            else
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                if (m_inputactive) {
                    DrawHistory();
                    if (m_historyRoot.childCount > 0)
                        m_eventSystem.SetSelectedGameObject(m_historyRoot.GetChild(m_historyRoot.childCount - 1).gameObject);
                }
                else {
                    if (m_selected) {
                        var selectable = m_selected.GetComponent<Selectable>();
                        if (selectable.navigation.selectOnUp != null)
                            m_eventSystem.SetSelectedGameObject(selectable.navigation.selectOnUp.gameObject);
                    }
                }
            }

            else
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace)) {
                delete_back_to_dot();
            }

            else
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                if (m_inputactive) {
                    if (m_inputField.text != string.Empty) {
                        EventSystem.current.SetSelectedGameObject(SelectAutoComplete());
                    }
                }
                else {
                    if (m_selected) {
                        var selectable = m_selected.GetComponent<Selectable>();
                        if (selectable.navigation.selectOnDown != null)
                            m_eventSystem.SetSelectedGameObject(selectable.navigation.selectOnDown.gameObject);
                    }
                }
            }
            else
                if (Input.GetKeyDown(KeyCode.Return)) {
                if (m_inputactive) {

                }
                else {
                    sendSubmitEvent();
                }
            }
            else
                if (Input.GetKey(KeyCode.LeftArrow)) {
                if (m_inputactive) {

                }
                else {
                    SelectInput();
                }
            }
            else
                if (Input.GetKey(KeyCode.RightArrow)) {
                if (m_inputactive) {

                }
                else {
                    sendSubmitEvent();
                }
            }
            else
                if (Input.GetKey(KeyCode.Tab)) {
                if (m_inputactive) {

                }
                else {
                    sendSubmitEvent();
                }
            }
            else
            if (Input.anyKeyDown) {
                if (m_inputactive) {

                }
                else {
                    SelectInput();
                }
            }
        }

        private void sendSubmitEvent() {
            if (m_selected) {
                var guibase = m_selected.GetComponent<GuiBase>();
                if (guibase != null)
                    ((ISubmitHandler)guibase).OnSubmit(null);
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

            SetupNavigation(m_historyRoot);

        }

        internal void SelectInput() {
            m_inputField.Select();
            m_inputField.ActivateInputField();
            m_inputField.selectionAnchorPosition = 0;
            m_inputField.selectionFocusPosition = 0;
            m_inputField.MoveTextEnd(false);
            //m_inputField.ActivateInputField();
        }

        internal void OnWriteLine(string message) {
            ConsoleGuiEntry entry = null;
            if (m_entries.Count > m_options.MaxConsoleLines) {
                entry = m_entries.Dequeue().GetComponent<ConsoleGuiEntry>();
                m_entries.Enqueue(entry);
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
            m_lastEntry = entry;
            StartCoroutine(SetScrollBarToZero());
        }

        private void OnExecutedLine(string line, Command command) {
            if (m_lastEntry.Text == line)
                m_lastEntry.SetCommand(command);
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

        private void OnDisable() {
            NavigationAllowed = true;
        }

        internal void SetInputText(string text) {
            m_inputField.text = text;
        }

        private T GetInterface<T>(GameObject inObj) where T : class {
            if (!typeof(T).IsInterface) {
                Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
                return null;
            }
            var objs = inObj.GetComponents<Component>();
            return objs.OfType<T>().FirstOrDefault();
        }
    }
}