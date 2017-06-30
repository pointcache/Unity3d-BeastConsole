namespace BeastConsole.GUI {

    using System.Collections;
    using System.Collections.Generic;
    using BeastConsole.Backend;
    using BeastConsole.Backend.Internal;
    using UnityEngine;
    using UnityEngine.UI;

    public class ConsoleGui : MonoBehaviour {

        public Options options;
        [System.Serializable]
        public class Options {
            public KeyCode ConsoleKey = KeyCode.BackQuote;
            public float tweenTime = 0.4f;
            public int maxConsoleLines = 120;
        }

        [SerializeField]
        private GameObject entryTemplate;
        [SerializeField]
        private GameObject consoleContent;
        [SerializeField]
        private RectTransform consoleRoot;
        [SerializeField]
        private InputField inputField;
        [SerializeField]
        private Scrollbar scrollBar;

        private Queue<ConsoleGuiEntry> entries = new Queue<ConsoleGuiEntry>();
        private int s_currentEXECUTIONhistoryIndex = 0;
        private ConsoleBackend backend;
        internal bool s_showConsole = false;

        private bool consoleShown;
        private bool inputTargeted;
        private Vector3 posTarget;
        private float lerpTime;


        internal void Initialize(ConsoleBackend backend) {
            if (!gameObject.activeSelf) {
                return;
            }
            this.backend = backend;
            consoleRoot.anchorMin = new Vector2(0f, 0.65f);
            consoleRoot.anchorMax = new Vector2(1f, 1f);
            posTarget = new Vector2(0, 10000);
            inputField.onEndEdit.AddListener(delegate { HandleTextInput(inputField.text); });
            backend.OnWriteLine += OnWriteLine;
            backend.OnExecutedLine += OnExecutedLine;
            backend.RegisterCommand("clear", "clear the console log", Clear);
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
                if (!consoleShown) {
                    inputField.gameObject.SetActive(true);
                    if (!inputTargeted) {
                        inputField.Select();
                        inputField.ActivateInputField();
                        inputTargeted = true;
                    }
                    posTarget = Vector2.zero;
                    lerpTime = 0;
                    consoleShown = true;
                }
                else {
                    inputField.text = "";
                    inputField.gameObject.SetActive(false);
                    inputTargeted = false;
                    posTarget = new Vector2(0, -consoleRoot.rect.y * 2);
                    lerpTime = 0;
                    scrollBar.value = 0;
                    consoleShown = false;
                }
            }
            if (inputField.isFocused) {
                if (Input.GetKeyDown(KeyCode.Tab)) {
                    AutoComplete(inputField.text);
                }
            }
            s_showConsole = false;
            if (lerpTime < options.tweenTime - 0.01f) {
                lerpTime += Time.deltaTime;
                lerpTime = Mathf.Clamp(lerpTime, 0f, options.tweenTime);
                consoleRoot.anchoredPosition = Vector3.Lerp(consoleRoot.anchoredPosition, posTarget, Remap(lerpTime, 0f, options.tweenTime, 0f, 1f));
            }
        }


        private void HandleInput() {
            if (Input.GetKeyDown(options.ConsoleKey)) {
                s_showConsole = true;
                s_currentEXECUTIONhistoryIndex = backend.s_commandHistory.Count - 1;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                if (backend.s_commandHistory.Count > 0) {
                    s_currentEXECUTIONhistoryIndex = Mathf.Clamp(s_currentEXECUTIONhistoryIndex, 0, backend.s_commandHistory.Count - 1);
                    inputField.text = backend.s_commandHistory[s_currentEXECUTIONhistoryIndex];
                    inputField.caretPosition = inputField.text.Length;
                    s_currentEXECUTIONhistoryIndex--;
                }
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace)) {
                delete_back_to_dot();
            }
        }

        public void OnWriteLine(string message) {
            ConsoleGuiEntry entry = null;
            if (entries.Count > options.maxConsoleLines) {
                entry = entries.Dequeue().GetComponent<ConsoleGuiEntry>();
                entries.Enqueue(entry);
                //OffsetAllEntriesOnce();
                entry.transform.SetAsLastSibling();
            }
            else {
                entry = Instantiate(entryTemplate).GetComponent<ConsoleGuiEntry>();
                entries.Enqueue(entry);
                entry.transform.SetParent(consoleContent.transform, true);
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
            inputField.text = "";
            inputField.ActivateInputField();
            backend.ExecuteLine(input);
        }

        private void OnExecutedLine(string line) {
            s_currentEXECUTIONhistoryIndex = backend.s_commandHistory.Count - 1;

        }

        IEnumerator SetScrollBarToZero() {
            int i = 0;
            while (i < 2) {
                i++;
                scrollBar.value = 0;
                yield return null;
            }
            scrollBar.value = 0;
            yield break;
        }

        private void delete_back_to_dot() {
            string text = inputField.text;
            if (text.Contains(".")) {
                int index = text.LastIndexOf('.');
                int length = text.Length - index;
                text = text.Remove(index + 1, length - 1);
            }
            else {
                text = "";
            }
            inputField.text = text;
        }

        private void AutoComplete(string input) {
            string[] lookup = backend.CComParameterSplit(input);
            if (lookup.Length == 0) {
                // don't auto complete if we have typed any parameters so far or nothing at all...
                return;
            }
            Command nearestMatch = backend.s_masterDictionary.AutoCompleteLookup(lookup[0]);
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
                inputField.text = insertion;
            }
            if (insertion[insertion.Length - 1] != '.')
                inputField.text = insertion + " ";
            inputField.caretPosition = inputField.text.Length;
        }


        private bool AutoCompleteTailString(string tailString, string input) {
            for (int i = 1; i < tailString.Length; ++i) {
                if (input.EndsWith(" " + tailString.Substring(0, i))) {
                    inputField.text = input.Substring(0, input.Length - 1) + tailString.Substring(i - 1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Clears out the console log
        /// </summary>
        /// <example> 
        /// <code>
        /// SmartConsole.Clear();
        /// </code>
        /// </example>
        public void Clear(string parameters) {
            //we dont want to clear our history, instead we clear the screen
            //s_outputHistory.Clear();
            DestroyChildren(consoleContent.transform);
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