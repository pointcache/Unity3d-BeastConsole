namespace BeastConsole.GUI
{
    using System.Collections.Generic;
#pragma warning disable 0649
    using System.Linq;
    using System.Text;
    using BeastConsole.Backend;
    using BeastConsole.Backend.Internal;
    using UnityEngine;

    internal class ConsoleGui
    {
        public static event System.Action<bool> OnStateChanged = delegate { };

        internal Options m_options;
        [System.Serializable]
        internal class Options
        {
            public KeyCode ConsoleKey = KeyCode.BackQuote;
            public float TweenTime = 0.4f;
            public int MaxConsoleLines = 120;
            public float LinePadding = 10;
            public bool LogHandler = true;
            public GUISkin skin;

            public Colors colors = new Colors();

            [System.Serializable]
            public class Colors
            {
                public Color error = new Color(1, 1, 1, 1);
                public Color warning = new Color(1, 1, 1, 1);
                public Color log = new Color(1, 1, 1, 1);
                public Color command = new Color(1, 1, 1, 1);
                public Color suggestionGreyed = new Color(1, 1, 1, 1);
            }
        }

        private GUISkin skin;

        private ConsoleBackend m_backend;
        private bool m_consoleShown;
        private bool drawConsole;
        private bool consoleWasOpened;
        private bool InputToggleConsole = false;
        private bool GUIToggleConsole = false;
        private bool moveToEnd = false;

        private int consoleSize;
        private float ConsoleHeight
        {
            get
            {
                switch (consoleSize)
                {
                    case 1:
                    return Screen.height / 3F;
                    case 2:
                    return Screen.height / 2F;
                    case 3:
                    return (Screen.height / 2F) + (Screen.height / 4F);

                }

                return Screen.height / 3F;
            }
        }


        private float m_lerpTime;
        private Rect rect_console;
        private float console_targetPosition;
        private float console_currentPosition;
        private Vector2 consoleScrollPosition;
        private float ClosedPosition => -(ConsoleHeight + 20F);
        private Rect inputFieldRect => new Rect(5, rect_console.y + rect_console.height + 5, 400, skin.textField.CalcHeight(new GUIContent("Input"), 50));
        private int currentSuggestionIndex = -1;
        private string currentSuggestion;
        private int currentCommandHistoryIndex = -1;

        private string console_input;

        private const string INPUT_FIELD_NAME = "ifield";
        private GUIStyle suggestionStyle;
        private GUIStyle suggestionActiveStyle;
        private Texture2D img_box;
        private StringBuilder sb = new StringBuilder();

        private string greycolorstr;

        private struct MsgData
        {
            public string msg;
            public int count;

            public MsgData(string str)
            {
                msg = str;
                count = 0;
            }
        }

        private List<MsgData> msgHistory = new List<MsgData>();

        internal ConsoleGui(ConsoleBackend backend, Options options)
        {
            skin = options.skin;

            greycolorstr = ConsoleUtility.ToHex(options.colors.suggestionGreyed);
            suggestionStyle = skin.customStyles.Where(x => x.name == "suggestion").FirstOrDefault();
            suggestionActiveStyle = skin.customStyles.Where(x => x.name == "suggestionActive").FirstOrDefault();
            img_box = skin.customStyles.Where(x => x.name == "img_box").FirstOrDefault().normal.background;
            m_backend = backend;
            m_options = options;

            m_backend.OnWriteLine += OnWriteLine;
            SetSize(PlayerPrefs.GetInt("beastconsole.size"));
            console_currentPosition = ClosedPosition;
            console_targetPosition = ClosedPosition;


            m_backend.RegisterVariable<int>(SetSize, this, "console.size", "Set the size of the console, 1/2/3");
            m_backend.RegisterCommand("clr", "clear the console log", this, Clear);
        }

        private void SetSize(int size)
        {
            consoleSize = Mathf.Clamp(size, 1, 3);
            PlayerPrefs.SetInt("beastconsole.size", size);
        }

        internal void Update()
        {
            rect_console = new Rect(0, 0, Screen.width, ConsoleHeight);
            consoleWasOpened = false;

            InputToggleConsole = Input.GetKeyDown(m_options.ConsoleKey);

            if (InputToggleConsole || GUIToggleConsole)
            {
                GUIToggleConsole = false;
                InputToggleConsole = false;

                // Do Open
                if (!m_consoleShown)
                {
                    console_targetPosition = 0F;

                    if (OnStateChanged != null)
                        OnStateChanged(true);
                    m_lerpTime = 0;
                    m_consoleShown = true;
                    console_input = "";
                    consoleWasOpened = true;

                    ScrollToBottom();
                }
                else
                {

                    m_lerpTime = 0;

                    console_targetPosition = ClosedPosition;

                    m_consoleShown = false;
                    if (OnStateChanged != null)
                        OnStateChanged(false);
                }
            }

            if (m_lerpTime < m_options.TweenTime - 0.01f)
            {
                m_lerpTime += Time.deltaTime;
                m_lerpTime = Mathf.Clamp(m_lerpTime, 0f, m_options.TweenTime);
                console_currentPosition = Mathf.Lerp(
                    console_currentPosition,
                    console_targetPosition,
                    Remap(m_lerpTime, 0f, m_options.TweenTime, 0f, 1f));

                rect_console = new Rect(0, console_currentPosition, rect_console.width, rect_console.height);

                drawConsole = !Mathf.Approximately(console_currentPosition, ClosedPosition);

            }
        }

        internal void OnGUI()
        {
            if (!drawConsole)
                return;

            GUI.skin = skin;
            DrawConsole();
            if (m_consoleShown)
                ControlInputField();
        }

        private void DrawConsole()
        {
            GUI.Box(rect_console, "Beast console");
            if (m_consoleShown)
                DrawHistory();
        }

        private void ControlInputField()
        {
            Event e = Event.current;


            if (!consoleWasOpened && e.type == EventType.KeyDown && e.keyCode == m_options.ConsoleKey)
            {
                GUI.FocusControl(null);
                GUIToggleConsole = true;
                e.Use();
                return;
            }

            if (e.type == EventType.KeyDown)
            {
                if (GUI.GetNameOfFocusedControl() == INPUT_FIELD_NAME)
                {
                    if (e.keyCode == KeyCode.Return)
                    {
                        e.Use();
                        if (currentSuggestionIndex == -1)
                        {
                            if (!string.IsNullOrEmpty(console_input))
                            {
                                try
                                {
                                    HandleInput(console_input);
                                }
                                finally
                                {
                                    console_input = "";
                                    ScrollToBottom();
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(currentSuggestion))
                            {
                                try
                                {
                                    HandleInput(currentSuggestion);
                                }
                                finally
                                {
                                    currentSuggestion = null;
                                    currentSuggestionIndex = -1;
                                    console_input = "";
                                    ScrollToBottom();
                                }
                            }
                        }
                    }
                    else if (e.keyCode == KeyCode.Tab)
                    {
                        e.Use();
                        if (currentSuggestionIndex == -1)
                        {
                            AutoComplete(console_input);
                            moveToEnd = true;
                        }
                        else
                        {
                            console_input = currentSuggestion;
                            moveToEnd = true;
                        }
                    }
                    else if (e.keyCode == KeyCode.DownArrow)
                    {
                        e.Use();
                        if (currentCommandHistoryIndex == -1)
                        {
                            currentSuggestionIndex++;
                            currentCommandHistoryIndex = -1;
                        }
                        else if (currentCommandHistoryIndex > -1)
                        {
                            currentCommandHistoryIndex--;
                            SetCmdHistoryItem();
                            moveToEnd = true;
                        }
                    }
                    else if (e.keyCode == KeyCode.UpArrow)
                    {
                        e.Use();
                        if (currentSuggestionIndex != -1)
                        {
                            currentSuggestionIndex--;
                            currentCommandHistoryIndex = -1;
                        }
                        else
                        {
                            currentCommandHistoryIndex++;
                            SetCmdHistoryItem();
                            moveToEnd = true;
                        }
                    }
                    else if (e.isKey)
                    {
                        currentSuggestionIndex = -1;
                        currentCommandHistoryIndex = -1;
                    }
                }
            }


            DrawInputField();
            DrawAutoCompleteSuggestions(console_input);

            if (consoleWasOpened)
            {
                GUI.FocusControl(INPUT_FIELD_NAME);
            }

        }

        private void SetCmdHistoryItem()
        {
            var cmdhis = m_backend.m_commandHistory;
            var count = cmdhis.Count;
            currentCommandHistoryIndex = Mathf.Clamp(currentCommandHistoryIndex, -1, count - 1);
            if (count == 0 || currentCommandHistoryIndex < 0)
                return;

            console_input = cmdhis[cmdhis.Count - currentCommandHistoryIndex - 1];
        }

        private void DrawInputField()
        {
            GUI.SetNextControlName(INPUT_FIELD_NAME);
            Rect inputField = inputFieldRect;
            Vector2 size = skin.textField.CalcSize(new GUIContent(console_input));

            if (inputField.width < size.x)
            {
                inputField.width = size.x + 10F;
            }

            console_input = GUI.TextField(inputField, console_input);
            if (moveToEnd)
            {
                TextEditor txt = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                txt.text = console_input;
                txt.MoveLineEnd();
                moveToEnd = false;
            }
        }

        private void OnWriteLine(string str)
        {
            int count = msgHistory.Count;
            if (count != 0)
            {
                var lastMsgData = msgHistory[msgHistory.Count - 1];
                if (lastMsgData.msg == str)
                {
                    lastMsgData.count++;
                    msgHistory[msgHistory.Count - 1] = lastMsgData;
                }
                else
                {
                    msgHistory.Add(new MsgData(str));
                }
            }
            else
            {
                msgHistory.Add(new MsgData(str));
            }

            ScrollToBottom();
        }

        private void DrawAutoCompleteSuggestions(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            var results = m_backend.m_commandsTrie.GetByPrefix(str);
            var count = results.Count();

            if (currentSuggestionIndex > count - 1)
                currentSuggestionIndex = 0;

            var inputrect = InputFieldBottom();
            int num = 0;
            foreach (var item in results)
            {
                int length = ConsoleUtility.WrapInColor(greycolorstr, console_input, out string result);
                sb.Clear();
                sb.Append(result);
                sb.Append(item.Value);
                sb.Remove(length, console_input.Length);
                string value = sb.ToString();

                Vector2 size = suggestionStyle.CalcSize(new GUIContent(value));
                Rect pos = new Rect(inputrect.x, inputrect.y + size.y * num, size.x, size.y);

                string description = null;
                if (m_backend.m_masterDictionary.TryGetValue(item.Value, out Command cmd))
                {
                    description = cmd.m_description;
                }

                if (currentSuggestionIndex == num)
                {
                    currentSuggestion = item.Value;
                    GUI.Label(pos, item.Value, suggestionActiveStyle);
                }
                else
                {
                    GUI.Label(pos, value, suggestionStyle);
                }

                if (!string.IsNullOrEmpty(description))
                {
                    var descriptionSize = suggestionStyle.CalcSize(new GUIContent(description));

                    if (currentSuggestionIndex == num)
                    {
                        GUI.Label(new Rect(pos.x + pos.width + 5F, pos.y, descriptionSize.x, descriptionSize.y), description, suggestionStyle);
                    }
                    else
                    {
                        GUI.color = new Color(1, 1, 1, 0.5F);
                        GUI.Label(new Rect(pos.x + pos.width + 5F, pos.y, descriptionSize.x, descriptionSize.y), description, suggestionStyle);
                        GUI.color = new Color(1, 1, 1, 1);
                    }
                }

                num++;
            }
        }

        private void DrawHistory()
        {
            var list = msgHistory;
            int count = list.Count;

            float totalHeight = GetHistoryContentHeight();

            Rect historyRect = rect_console;

            Rect viewRect = new Rect(historyRect.x, historyRect.y, historyRect.width - 10, totalHeight);



            consoleScrollPosition = GUI.BeginScrollView(historyRect, consoleScrollPosition, viewRect);
            {
                float currentYPos = (historyRect.height > viewRect.height ? historyRect.height : viewRect.height);

                for (int i = count - 1; i >= 0; i--)
                {
                    var msgData = list[i];
                    string msg = (msgData.count > 0) ? msgData.msg + "   x" + msgData.count : msgData.msg;
                    float height = CalcHeightForLine(msg);
                    var rect = new Rect(0, currentYPos -= height, viewRect.width, height);
                    if (i % 2 == 0)
                    {
                        GUI.DrawTexture(new Rect(0F, rect.y, Screen.width, rect.height), img_box);
                    }

                    GUI.Label(rect, msg);
                }
            }
            GUI.EndScrollView();

        }


        private void ScrollToBottom()
        {
            consoleScrollPosition = new Vector2(0, GetHistoryContentHeight());
        }

        private float CalcHeightForLine(string line)
        {
            return skin.label.CalcHeight(new GUIContent(line), Screen.width);
        }

        private Rect InputFieldBottom()
        {
            Rect rect = inputFieldRect;
            Vector2 size = skin.textField.CalcSize(new GUIContent(console_input));

            return new Rect(rect.x, rect.y + size.y + 5F, 0, 0);
        }

        private Rect CaretPosition()
        {
            Rect rect = inputFieldRect;
            Vector2 size = skin.textField.CalcSize(new GUIContent(console_input));

            return new Rect(rect.x + size.x, rect.y + size.y + 5F, 0, 0);
        }

        private float GetHistoryContentHeight()
        {
            float totalHeight = 0F;
            var list = m_backend.m_outputHistory;
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var cmd = list[i];
                totalHeight += CalcHeightForLine(cmd);
            }
            return totalHeight;
        }

        private void HandleInput(string text)
        {
            m_backend.ExecuteLine(text);
        }




        public static Rect RectWithPadding(Rect rect, int padding)
        {
            return new Rect(rect.x + padding, rect.y + padding, rect.width - padding - padding, rect.height - padding - padding);
        }

        private float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }




        private void AutoComplete(string input)
        {
            string[] lookup = m_backend.CComParameterSplit(input);
            if (lookup.Length == 0)
            {
                // don't auto complete if we have typed any parameters so far or nothing at all...
                return;
            }
            Command nearestMatch = m_backend.m_masterDictionary.AutoCompleteLookup(lookup[0]);
            // only complete to the next dot if there is one present in the completion string which
            // we don't already have in the lookup string
            int dotIndex = 0;
            do
            {
                dotIndex = nearestMatch.m_name.IndexOf(".", dotIndex + 1);
            }
            while ((dotIndex > 0) && (dotIndex < lookup[0].Length));
            string insertion = nearestMatch.m_name;
            if (dotIndex >= 0)
            {
                insertion = nearestMatch.m_name.Substring(0, dotIndex + 1);
            }
            if (insertion.Length < input.Length)
            {
                //do
                //{
                //    if (AutoCompleteTailString("true", input))
                //        break;
                //    if (AutoCompleteTailString("false", input))
                //        break;
                //    if (AutoCompleteTailString("True", input))
                //        break;
                //    if (AutoCompleteTailString("False", input))
                //        break;
                //    if (AutoCompleteTailString("TRUE", input))
                //        break;
                //    if (AutoCompleteTailString("FALSE", input))
                //        break;
                //}
                //while (false);
            }
            else if (insertion.Length >= input.Length) // SE - is this really correct?
            {
                console_input = insertion;
            }
            if (insertion[insertion.Length - 1] != '.')
                console_input = insertion;
        }

        /// <summary>
        /// Clears out the console log
        /// </summary>
        /// <example> 
        /// <code>
        /// SmartConsole.Clear();
        /// </code>
        /// </example>
        internal void Clear(string[] parameters)
        {
            //we dont want to clear our history, instead we clear the screen
            msgHistory.Clear();
        }
    }
}