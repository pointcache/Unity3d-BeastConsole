
// Copyright (c) 2014 Cranium Software

// SmartConsole
//
// A Quake style debug console where you can add variables with the
// CreateVariable functions and commands with the RegisterCommand functions
// the variables wrap their underlying types, the commands are delegates
// taking a string with the line of console input and returning void

// TODO:
// * sort out spammy history and 'return' key handling on mobile platforms
// * improve cvar interface
// * allow history to scroll
// * improve autocomplete
// * allow executing console script from file

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using System;



// SE: broadly patterned after the debug console implementation from GLToy...
// https://code.google.com/p/gltoy/source/browse/trunk/GLToy/Independent/Core/Console/GLToy_Console.h

/// <summary>
/// A Quake style debug console - should be added to an otherwise empty game object and have a font set in the inspector
/// </summary>
public class SmartConsole : MonoBehaviour
{
    public delegate void ConsoleCommandFunction(string parameters);

    private static SmartConsole _singleton;
    public static SmartConsole singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = GameObject.FindObjectOfType<SmartConsole>();
            }
            return _singleton;
        }
    }


    // control the general layout here

    //	private static Vector3 k_position = new Vector3( 0.01f, 0.65f, 0.0f );
    //	private static Vector3 k_fullPosition = new Vector3( 0.01f, 0.05f, 0.0f );
    //	private static Vector3 k_hidePosition = new Vector3( 0.01f, 1.1f, 0.0f );
    //	private static Vector3 k_scale = new Vector3( 0.5f, 0.5f, 1.0f );

    // SE: annoying having to leak this out publicly - basically to facilitate the weird and wonderful cvar implementation
    /// <summary>
    /// A class representing a console command - WARNING: this is only exposed as a hack!
    /// </summary>
    public class Command
    {
        public ConsoleCommandFunction m_callback = null;
        public string m_name = null;
        public string m_paramsExample = "";
        public string m_help = "(no description)";
    };


    public static Options options;
    [System.Serializable]
    public class Options
    {   // allow to specify font (because we need one imported)
        /// <summary>
        /// The font used to render the console
        /// </summary>
        public KeyCode ConsoleKey;
        public float tweenTime = 0.4f;
        public int maxConsoleLines = 120;
         
    }

    public static GameObject entryTemplate;
    public static GameObject consoleContent;
    public static RectTransform consoleRoot;
    public static InputField inputField;
    public static Scrollbar scrollBar;

    private static Queue<SmartConsoleEntry> entries = new Queue<SmartConsoleEntry>();

    // SE - this is a bit elaborate, needed to provide a way to do this
    // without relying on memory addresses or pointers... which has resulted in
    // a little blob of bloat and overhead for something that should be trivial... :/

    /// <summary>
    /// A class representing a console variable
    /// </summary>
    public class Variable<T> : Command where T : new()
    {
        CFG.Variable<T> configVar;

        public Variable(CFG.Variable<T> var)
        {
            configVar = var;
            m_name = var.name;
            m_help = var.description;
            m_callback = CommandFunction;
        }
        public void Set(T val) // SE: I don't seem to know enough C# to provide a user friendly assignment operator solution
        {
            configVar.Set(val);
        }

        public static implicit operator T(Variable<T> var)
        {
            return var.configVar;
        }

        private static void CommandFunction(string parameters)
        {
            string[] split = CVarParameterSplit(parameters);
            if ((split.Length != 0) && s_variableDictionary.ContainsKey(split[0]))
            {
                Variable<T> variable = s_variableDictionary[split[0]] as Variable<T>;
                string conjunction = " is set to ";
                if (split.Length == 2)
                {
                    variable.SetFromString(split[1]);
                    conjunction = " has been set to ";
                }

                WriteLine(variable.configVar.name + conjunction + variable.configVar.value);
            }
        }

        private void SetFromString(string value)
        {
            if (typeof(T) == typeof(bool))
            {
                if (value == "0")
                {
                    Set((T)System.Convert.ChangeType("false", typeof(T)));
                }
                else if (value == "1")
                {
                    Set((T)System.Convert.ChangeType("true", typeof(T)));
                }
                else
                    Set((T)System.Convert.ChangeType(value, typeof(T)));
            }
            else
                Set((T)System.Convert.ChangeType(value, typeof(T)));
        }


    };


    void Awake()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        Initialise(this);

        consoleRoot.anchorMin = new Vector2(0f, 0.65f);
        consoleRoot.anchorMax = new Vector2(1f, 1f);
        
    }
    bool consoleShown;
    bool inputTargeted;
    void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }


        HandleInput();

        if (s_showConsole)
        {
            if (!consoleShown)
            {
                inputField.gameObject.SetActive(true);
                if (!inputTargeted)
                {
                    inputField.Select();
                    inputField.ActivateInputField();
                    inputTargeted = true;
                }

                consoleRoot.DOAnchorPos(Vector3.zero, options.tweenTime);


                consoleShown = true;
                CFG.consoleOpened.Set(true);
            }
            else
            {
                inputField.text = "";
                inputField.gameObject.SetActive(false);
                inputTargeted = false;

                consoleRoot.DOAnchorPos(new Vector2(0, -consoleRoot.rect.y * 2), options.tweenTime);
                scrollBar.value = 0;
                consoleShown = false;
                CFG.consoleOpened.Set(false);
            }


        }

        if (inputField.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                AutoComplete(inputField.text);
            }
        }
        s_showConsole = false;
    }

    static void delete_back_to_dot()
    {
        string text = inputField.text;

        if (text.Contains("."))
        {
            int index = text.LastIndexOf('.');
            int length = text.Length - index;
            text = text.Remove(index + 1, length - 1);
        }
        else
        {
            text = "";
        }
        inputField.text = text;
    }

    /// <summary>
    /// Clears out the console log
    /// </summary>
    /// <example> 
    /// <code>
    /// SmartConsole.Clear();
    /// </code>
    /// </example>
    public static void Clear()
    {
        s_outputHistory.Clear();
    }

    /// <summary>
    /// Write a message to the debug console (only - not the log)
    /// </summary>
    /// <param name="message">
    /// The message to display
    /// </param>
    /// <example>
    /// <code>
    /// SmartConsole.Print( "Hello world!" );
    /// </code>
    /// </example>
    public static void Print(string message)
    {
        WriteLine(message);
    }

    /// <summary>
    /// Write a message to the debug console (only - not the log)
    /// </summary>
    /// <param name="message">
    /// The message to display
    /// </param>
    /// <example>
    /// <code>
    /// SmartConsole.WriteLine( "Hello world!" );
    /// </code>
    /// </example>
    public static void WriteLine(string message)
    {
        SmartConsoleEntry entry = null;
        if (entries.Count > options.maxConsoleLines)
        {
            entry = entries.Dequeue().GetComponent<SmartConsoleEntry>();

            entries.Enqueue(entry);
            //OffsetAllEntriesOnce();
            entry.transform.SetAsLastSibling();
        }
        else
        {
            entry = Instantiate(entryTemplate).GetComponent<SmartConsoleEntry>();
            entries.Enqueue(entry);
            entry.transform.SetParent(consoleContent.transform, true);
            entry.transform.SetAsLastSibling();
        }

        entry.Clear();
        entry.SetText(message);

        singleton.StartCoroutine(SetScrollBarToZero());

        s_outputHistory.Add(DeNewLine(message));
        //s_currentCommandHistoryIndex = s_outputHistory.Count - 1;

    }

    static IEnumerator SetScrollBarToZero()
    {
        int i = 0;
        while (i < 2)
        {
            i++;
            scrollBar.value = 0;
            yield return null;
        }
        scrollBar.value = 0;
        yield break;
    }

    /// <summary>
    /// Execute a string as if it were a single line of input to the console
    /// </summary>
    public static void ExecuteLine(string inputLine)
    {

        string[] words = CComParameterSplit(inputLine);
        if (words.Length > 0)
        {
            if (s_masterDictionary.ContainsKey(words[0]))
            {
                WriteLine("<b>=> </b><color=lime>" + inputLine + "</color>");
                s_masterDictionary[words[0]].m_callback(inputLine);
            }
            else
            {
                WriteLine("<color=red>Unrecognised command or variable name: " + words[0] + "</color>");
            }
            s_commandHistory.Add(inputLine);
            s_currentEXECUTIONhistoryIndex = s_commandHistory.Count - 1;
        }
    }

    // public static void ExecuteFile( string path ) {} //...

    public static void RemoveCommandIfExists(string name)
    {
        s_commandDictionary.Remove(name);
        s_masterDictionary.Remove(name);
    }

    /// <summary>
    /// Register a console command with an example of usage and a help description
    /// e.g. SmartConsole.RegisterCommand( "echo", "echo <string>", "writes <string> to the console log", SmartConsole.Echo );
    /// </summary>
    public static void RegisterCommand(string name, string exampleUsage, string helpDescription, ConsoleCommandFunction callback)
    {
        Command command = new Command();
        command.m_name = name;
        command.m_paramsExample = exampleUsage;
        command.m_help = helpDescription;
        command.m_callback = callback;

        s_commandDictionary.Add(name, command);
        s_masterDictionary.Add(name, command);
    }

    /// <summary>
    /// Register a console command with a help description
    /// e.g. SmartConsole.RegisterCommand( "help", "displays help information for console command where available", SmartConsole.Help );
    /// </summary>
    public static void RegisterCommand(string name, string helpDescription, ConsoleCommandFunction callback)
    {
        RegisterCommand(name, "", helpDescription, callback);
    }

    /// <summary>
    /// Register a console command
    /// e.g. SmartConsole.RegisterCommand( "foo", Foo );
    /// </summary>
    public static void RegisterCommand(string name, ConsoleCommandFunction callback)
    {
        RegisterCommand(name, "", "(no description)", callback);
    }

    public static void RegisterVariable<T>(CFG.Variable<T> var) where T : new()
    {
        if (s_variableDictionary.ContainsKey(var.name))
        {
            Debug.LogError("Tried to add already existing console variable!");
            return ;
        }

        Variable<T> returnValue = new Variable<T>(var);
        s_variableDictionary.Add(var.name, returnValue);
        s_masterDictionary.Add(var.name, returnValue);
    }


    /// <summary>
    /// Destroy a console variable (so its name can be reused)
    /// </summary>
    public static void UnregisterVariable<T>(Variable<T> variable) where T : new()
    {
        s_variableDictionary.Remove(variable.m_name);
        s_masterDictionary.Remove(variable.m_name);
    }

    // --- commands
    private static void Help(string parameters)
    {
        // try and lay it out nicely...
        const int nameLength = 25;
        const int exampleLength = 35;
        foreach (Command command in s_commandDictionary.Values)
        {
            string outputString = command.m_name;
            for (int i = command.m_name.Length; i < nameLength; ++i)
            {
                outputString += " ";
            }

            if (command.m_paramsExample.Length > 0)
            {
                outputString += " example: " + command.m_paramsExample;
            }
            else
            {
                outputString += "          ";
            }

            for (int i = command.m_paramsExample.Length; i < exampleLength; ++i)
            {
                outputString += " ";
            }

            WriteLine(outputString + command.m_help);
        }
    }

    private static void Echo(string parameters)
    {
        string outputMessage = "";
        string[] split = CComParameterSplit(parameters);
        for (int i = 1; i < split.Length; ++i)
        {
            outputMessage += split[i] + " ";
        }

        if (outputMessage.EndsWith(" "))
        {
            outputMessage.Substring(0, outputMessage.Length - 1);
        }

        WriteLine(outputMessage);
    }

    private static void Clear(string parameters)
    {
        Clear();
    }

    private static void LastExceptionCallStack(string parameters)
    {
        DumpCallStack(s_lastExceptionCallStack);
    }

    private static void LastErrorCallStack(string parameters)
    {
        DumpCallStack(s_lastErrorCallStack);
    }

    private static void LastWarningCallStack(string parameters)
    {
        DumpCallStack(s_lastWarningCallStack);
    }

    private static void Quit(string parameters)
    {
        Application.Quit();

    }


    private static void ListCvars(string parameters)
    {
        // try and lay it out nicely...
        const int nameLength = 50;
        foreach (Command variable in s_variableDictionary.Values)
        {
            string outputString = variable.m_name;
            for (int i = variable.m_name.Length; i < nameLength; ++i)
            {
                outputString += " ";
            }

            WriteLine(outputString + variable.m_help);
        }
    }

    // --- internals

    private static void Initialise(SmartConsole instance)
    {
        // run this only once...
        if (s_textInput != null)
        {
            return;
        }
#if UNITY_EDITOR
        Application.logMessageReceived += LogHandler;
#endif
        InitialiseCommands();
        inputField.onEndEdit.AddListener(delegate { HandleTextInput(inputField.text); });
    }

    static int s_currentEXECUTIONhistoryIndex = 0;
    //static int s_currentCommandHistoryIndex = 0;
    private static void HandleInput()
    {
        if (Input.GetKeyDown(options.ConsoleKey))
        {

            s_showConsole = true;
            s_currentEXECUTIONhistoryIndex = s_commandHistory.Count - 1;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (s_commandHistory.Count > 0)
            {
                s_currentEXECUTIONhistoryIndex = Mathf.Clamp(s_currentEXECUTIONhistoryIndex, 0, s_commandHistory.Count - 1);
                inputField.text = s_commandHistory[s_currentEXECUTIONhistoryIndex];
                inputField.caretPosition = inputField.text.Length;
                s_currentEXECUTIONhistoryIndex--;
            }
        }
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace))
        {
            delete_back_to_dot();
        }
    }

    private static void InitialiseCommands()
    {
        RegisterCommand("clear", "clear the console log", Clear);
        RegisterCommand("cls", "clear the console log (alias for Clear)", Clear);
        RegisterCommand("echo", "echo <string>", "writes <string> to the console log (alias for echo)", Echo);
        RegisterCommand("help", "displays help information for console command where available", Help);
        RegisterCommand("list", "lists all currently registered console variables", ListCvars);
        RegisterCommand("print", "print <string>", "writes <string> to the console log", Echo);
        RegisterCommand("quit", "quit the game (not sure this works with iOS/Android)", Quit);
        RegisterCommand("callstack.warning", "display the call stack for the last warning message", LastWarningCallStack);
        RegisterCommand("callstack.error", "display the call stack for the last error message", LastErrorCallStack);
        RegisterCommand("callstack.exception", "display the call stack for the last exception message", LastExceptionCallStack);
    }

    private static void HandleTextInput(string input)
    {
        if (input.Length == 0)
            return;
        inputField.text = "";
        inputField.ActivateInputField();
        ExecuteCurrentLine(input);
    }

    private static void ExecuteCurrentLine(string line)
    {
        ExecuteLine(line);
    }

    private static void AutoComplete(string input)
    {
        string[] lookup = CComParameterSplit(input);
        if (lookup.Length == 0)
        {
            // don't auto complete if we have typed any parameters so far or nothing at all...
            return;
        }

        Command nearestMatch = s_masterDictionary.AutoCompleteLookup(lookup[0]);

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
            do
            {
                if (AutoCompleteTailString("true", input)) break;
                if (AutoCompleteTailString("false", input)) break;
                if (AutoCompleteTailString("True", input)) break;
                if (AutoCompleteTailString("False", input)) break;
                if (AutoCompleteTailString("TRUE", input)) break;
                if (AutoCompleteTailString("FALSE", input)) break;
            }
            while (false);
        }
        else if (insertion.Length >= input.Length) // SE - is this really correct?
        {
            inputField.text = insertion;
        }
        if(insertion[insertion.Length -1] != '.')
            inputField.text = insertion + " ";
        inputField.caretPosition = inputField.text.Length;
    }

    private static bool AutoCompleteTailString(string tailString, string input)
    {
        for (int i = 1; i < tailString.Length; ++i)
        {
            if (input.EndsWith(" " + tailString.Substring(0, i)))
            {
                inputField.text = input.Substring(0, input.Length - 1) + tailString.Substring(i - 1);
                return true;
            }
        }

        return false;
    }

    public enum myLogType {
        error,
        warning,
        confirmation,
        log
    }

    public static void Log(string msg, myLogType type)
    {
        string confirmPrefix = "<color=#00ff72>[OK!]: ";
        string errorPrefix = "<color=#ff225b>[ERR]: ";
        string warningPrefix = "<color=#ffa922>[WNG]: ";
        string otherPrefix = "<color=white>[LOG]: ";

        string prefix = otherPrefix;
        switch (type)
        {
            case myLogType.confirmation:
                {
                    prefix = confirmPrefix;
                    break;
                }

            case myLogType.warning:
                {
                    prefix = warningPrefix;
                    
                    break;
                }

            case myLogType.error:
                {
                    prefix = errorPrefix;
                    
                    break;
                }

            case myLogType.log:
                {
                    prefix = otherPrefix;
                   
                    break;
                }

            default:
                {
                    break;
                }
        }

        WriteLine(prefix + msg + "</color>");
    }

    private static void LogHandler(string message, string stack, LogType type)
    {
        //if( !s_logging )
        //{
        //	return;
        //}

        string assertPrefix = "<color=white>[Assert]:";
        string errorPrefix = "<color=red>[ERROR]: ";
        string exceptPrefix = "<color=red>[EXCEPT]: ";
        string warningPrefix = "<color=orange>[WARNING]:";
        string otherPrefix = "<color=white>";

        string prefix = otherPrefix;
        switch (type)
        {
            case LogType.Assert:
                {
                    prefix = assertPrefix;
                    break;
                }

            case LogType.Warning:
                {
                    prefix = warningPrefix;
                    s_lastWarningCallStack = stack;
                    break;
                }

            case LogType.Error:
                {
                    prefix = errorPrefix;
                    s_lastErrorCallStack = stack;
                    break;
                }

            case LogType.Exception:
                {
                    prefix = exceptPrefix;
                    s_lastExceptionCallStack = stack;
                    break;
                }

            default:
                {
                    break;
                }
        }

        WriteLine(prefix + message + "</color>");

        //switch (type)
        //{
        //    case LogType.Assert:
        //    case LogType.Error:
        //    case LogType.Exception:
        //        {
        //            //WriteLine ( "Call stack:\n" + stack );
        //            break;
        //        }
        //
        //    default:
        //        {
        //            break;
        //        }
        //}
    }

    public static string[] CComParameterSplit(string parameters)
    {
        return parameters.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] CComParameterSplit(string parameters, int requiredParameters)
    {
        string[] split = CComParameterSplit(parameters);
        if (split.Length < (requiredParameters + 1))
        {
            WriteLine("Error: not enough parameters for command. Expected " + requiredParameters + " found " + (split.Length - 1));
        }

        if (split.Length > (requiredParameters + 1))
        {
            int extras = ((split.Length - 1) - requiredParameters);
            WriteLine("Warning: " + extras + "additional parameters will be dropped:");
            for (int i = split.Length - extras; i < split.Length; ++i)
            {
                WriteLine("\"" + split[i] + "\"");
            }
        }

        return split;
    }

    private static string[] CVarParameterSplit(string parameters)
    {
        string[] split = CComParameterSplit(parameters);
        if (split.Length == 0)
        {
            WriteLine("Error: not enough parameters to set or display the value of a console variable.");
        }

        if (split.Length > 2)
        {
            int extras = (split.Length - 3);
            WriteLine("Warning: " + extras + "additional parameters will be dropped:");
            for (int i = split.Length - extras; i < split.Length; ++i)
            {
                WriteLine("\"" + split[i] + "\"");
            }
        }

        return split;
    }

    private static string DeNewLine(string message)
    {
        return message.Replace("\n", " | ");
    }

    private static void DumpCallStack(string stackString)
    {
        string[] lines = stackString.Split(new char[] { '\r', '\n' });

        if (lines.Length == 0)
        {
            return;
        }

        int ignoreCount = 0;
        while ((lines[lines.Length - 1 - ignoreCount].Length == 0) && (ignoreCount < lines.Length))
        {
            ++ignoreCount;
        }
        int lineCount = lines.Length - ignoreCount;
        for (int i = 0; i < lineCount; ++i)
        {
            // SE - if the call stack is 100 deep without recursion you have much bigger problems than you can ever solve with a debugger...
            WriteLine((i + 1).ToString() + ((i < 9) ? "  " : " ") + lines[i]);
        }
    }

    private class AutoCompleteDictionary<T> : SortedDictionary<string, T>
    {
        public AutoCompleteDictionary()
        : base(new AutoCompleteComparer())
        {
            m_comparer = this.Comparer as AutoCompleteComparer;
        }

        public T LowerBound(string lookupString)
        {
            m_comparer.Reset();
            this.ContainsKey(lookupString);
            return this[m_comparer.LowerBound];
        }

        public T UpperBound(string lookupString)
        {
            m_comparer.Reset();
            this.ContainsKey(lookupString);
            return this[m_comparer.UpperBound];
        }

        public T AutoCompleteLookup(string lookupString)
        {
            m_comparer.Reset();
            this.ContainsKey(lookupString);
            string key = (m_comparer.UpperBound == null) ? m_comparer.LowerBound : m_comparer.UpperBound;
            return this[key];
        }

        private class AutoCompleteComparer : IComparer<string>
        {
            private string m_lowerBound = null;
            private string m_upperBound = null;

            public string LowerBound { get { return m_lowerBound; } }
            public string UpperBound { get { return m_upperBound; } }

            public int Compare(string x, string y)
            {
                int comparison = Comparer<string>.Default.Compare(x, y);

                if (comparison >= 0)
                {
                    m_lowerBound = y;
                }

                if (comparison <= 0)
                {
                    m_upperBound = y;
                }

                return comparison;
            }

            public void Reset()
            {
                m_lowerBound = null;
                m_upperBound = null;
            }
        }

        private AutoCompleteComparer m_comparer;
    }


    private static GameObject s_textInput = null;

    private static AutoCompleteDictionary<Command> s_commandDictionary = new AutoCompleteDictionary<Command>();
    private static AutoCompleteDictionary<Command> s_variableDictionary = new AutoCompleteDictionary<Command>();

    private static AutoCompleteDictionary<Command> s_masterDictionary = new AutoCompleteDictionary<Command>();

    private static List<string> s_commandHistory = new List<string>();
    private static List<string> s_outputHistory = new List<string>();

    private static string s_lastExceptionCallStack = "(none yet)";
    private static string s_lastErrorCallStack = "(none yet)";
    private static string s_lastWarningCallStack = "(none yet)";

    private static bool s_showConsole = false;
}
