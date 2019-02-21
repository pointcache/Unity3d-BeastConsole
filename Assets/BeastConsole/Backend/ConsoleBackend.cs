namespace BeastConsole.Backend
{

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BeastConsole.Backend.Internal;
    using BeastConsole.GUI;
#if UNITY_EDITOR
    using UnityEditor;
#endif
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

    // SE: broadly patterned after the debug console implementation from GLToy...
    // https://code.google.com/p/gltoy/source/browse/trunk/GLToy/Independent/Core/Console/GLToy_Console.h
    /// <summary>
    /// A Quake style debug console - should be added to an otherwise empty game object and have a font set in the inspector
    /// </summary>
    internal class ConsoleBackend
    {

        internal Action<string> OnWriteLine = delegate { };
        internal Action<string, Command> OnExecutedCommand = delegate { };

        internal GameObject m_textInput = null;
        internal AutoCompleteDictionary<Command> m_commandDictionary = new AutoCompleteDictionary<Command>();
        internal AutoCompleteDictionary<Command> m_variableDictionary = new AutoCompleteDictionary<Command>();
        internal AutoCompleteDictionary<Command> m_attributeCommandsDictionary = new AutoCompleteDictionary<Command>();
        internal AutoCompleteDictionary<Command> m_masterDictionary = new AutoCompleteDictionary<Command>();
        internal Trie<string> m_commandsTrie = new Trie<string>();
        internal List<string> m_commandHistory = new List<string>();
        internal List<string> m_outputHistory = new List<string>();
        internal string m_lastExceptionCallStack = "(none yet)";
        internal string m_lastErrorCallStack = "(none yet)";
        internal string m_lastWarningCallStack = "(none yet)";

        private string commandPrefix;
        private string errorPrefix;
        private string warningPrefix;
        private string logPrefix;
        private string greyColor;


        // --- internals
        internal ConsoleBackend(bool handleLogs, ConsoleGui.Options options)
        {
            // run this only once...
            if (m_textInput != null)
            {
                return;
            }
#if UNITY_EDITOR
            // Application.logMessageReceived += LogHandler;
#endif

            commandPrefix = ConsoleUtility.ToHex(options.colors.command) + "[CMD]: ";
            errorPrefix = ConsoleUtility.ToHex(options.colors.error) + "[ERR]: ";
            warningPrefix = ConsoleUtility.ToHex(options.colors.warning) + "[WNG]: ";
            logPrefix = ConsoleUtility.ToHex(options.colors.log) + "[LOG]: ";
            greyColor = ConsoleUtility.ToHex(options.colors.suggestionGreyed);

            RegisterCommand("echo", "writes <string> to the console log (alias for echo)", this, Echo);
            RegisterCommand("list", "lists all currently registered console variables", this, ListCvars);
            RegisterCommand("print", "writes <string> to the console log", this, Echo);
            RegisterCommand("quit", "quit the game (not sure this works with iOS/Android)", this, Quit);

            // RegisterCommand("help", "displays help information for console command where available", this, Help);
            // RegisterCommand("callstack.warning", "display the call stack for the last warning message", LastWarningCallStack);
            // RegisterCommand("callstack.error", "display the call stack for the last error message", LastErrorCallStack);
            // RegisterCommand("callstack.exception", "display the call stack for the last exception message", LastExceptionCallStack);
            CollectAllData();

            if (handleLogs)
            {
                Application.logMessageReceived += LogHandler;
            }
        }



        internal void CollectAllData()
        {
            Assembly assembly = Assembly.Load("Assembly-CSharp");

            var types = assembly.GetTypes();

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

            foreach (var type in types)
            {
                var doparse = type.GetCustomAttribute(typeof(ConsoleParseAttribute));
                if (doparse == null)
                    continue;

                var methods = type.GetMethods(flags);

                foreach (var method in methods)
                {
                    var atr = method.GetCustomAttribute(typeof(ConsoleCommandAttribute), false);
                    if (atr != null)
                    {
                        RegisterAttributeCommand(method, atr as ConsoleCommandAttribute);
                    }
                }

                var fields = type.GetFields(flags);

                foreach (var field in fields)
                {

                    var atr = field.GetCustomAttribute(typeof(ConsoleVariableAttribute), false);
                    if (atr != null)
                    {
                        RegisterAttributeVariableField(field, atr as ConsoleVariableAttribute);
                    }
                }

                var properties = type.GetProperties(flags);

                foreach (var prop in properties)
                {
                    var atr = prop.GetCustomAttribute(typeof(ConsoleVariableAttribute), false);
                    if (atr != null)
                    {
                        RegisterAttributeVariableProp(prop, atr as ConsoleVariableAttribute);
                    }
                }
            }
        }

        internal void WriteLine(string line)
        {
            m_outputHistory.Add(line);
            OnWriteLine(line);
        }


        internal void Print(string message)
        {
            WriteLine(message);
        }

        /// <summary>
        /// Execute a string as if it were a single line of input to the console
        /// </summary>
        internal void ExecuteLine(string inputLine)
        {
            string[] words = CComParameterSplit(inputLine);
            if (words.Length > 0)
            {
                try
                {
                    m_masterDictionary.TryGetValue(words[0], out Command com);
                    if (com != null)
                    {
                        WriteLine(ConsoleUtility.WrapInColor(commandPrefix + inputLine, ""));
                        m_masterDictionary[words[0]].Execute(inputLine);
                        OnExecutedCommand(inputLine, com);
                    }
                    else
                    {
                        WriteLine("<color=red>Unrecognised command or variable name: " + words[0] + "</color>");
                    }
                }
                finally
                {
                    m_commandHistory.Add(inputLine);
                }
            }
        }
        // public static void ExecuteFile( string path ) {} //...
        internal void RemoveCommandIfExists(string name, object owner)
        {
            m_commandDictionary.TryGetValue(name, out Command comm);
            if (comm != null)
            {
                comm.RemoveCommand(owner);
                if (comm.m_command.GetInvocationList().Length == 0)
                {
                    m_commandDictionary.Remove(name);
                    m_masterDictionary.Remove(name);
                }
            }
        }
        /// <summary>
        /// Register a console command with an example of usage and a help description
        /// e.g. SmartConsole.RegisterCommand( "echo", "echo <string>", "writes <string> to the console log", SmartConsole.Echo );
        /// </summary>
        internal void RegisterCommand(string name, string helpDescription, object owner, Action<string[]> callback)
        {

            m_masterDictionary.TryGetValue(name, out Command comm);
            if (comm != null)
            {
                comm.AddCommand(owner, callback);
                return;
            }
            else
            {
                Command command = new Command(name, helpDescription, this);
                command.AddCommand(owner, callback);
                m_commandDictionary.Add(name, command);
                m_masterDictionary.Add(name, command);
                m_commandsTrie.Add(new TrieEntry<string>(name, name));
            }
        }

        private void RegisterAttributeCommand(MethodInfo info, ConsoleCommandAttribute attr)
        {
            string commandName = attr.PrefixOnly ? attr.name + "." + info.Name : attr.name;

            m_masterDictionary.TryGetValue(commandName, out Command comm);
            if (comm != null)
            {
                Debug.LogError("Multiple Attribute ConsoleCommands with the same name: " + commandName + " , this is not allowed.");
                return;
            }
            else
            {
                AttributeCommand cmd = new AttributeCommand(commandName, attr.description, this);
                cmd.Initialize(info);
                m_attributeCommandsDictionary.Add(commandName, cmd);
                m_masterDictionary.Add(commandName, cmd);
                m_commandsTrie.Add(new TrieEntry<string>(commandName, commandName));
            }
        }

        internal void RegisterVariable<T>(Action<T> setter, object owner, string name, string desc)
        {
            m_masterDictionary.TryGetValue(name, out Command comm);
            if (comm != null)
            {
                var variable = comm as Variable<T>;
                variable.Add(owner, setter);
                return;
            }
            else
            {
                Variable<T> returnValue = new Variable<T>(name, desc, setter, owner, this);
                m_variableDictionary.Add(name, returnValue);
                m_masterDictionary.Add(name, returnValue);
                m_commandsTrie.Add(new TrieEntry<string>(name, name));

            }
        }

        internal void RegisterAttributeVariableField(FieldInfo info, ConsoleVariableAttribute attr)
        {
            m_masterDictionary.TryGetValue(attr.name, out Command comm);
            if (comm != null)
            {
                Debug.LogError("Multiple Attribute Variables with the same name: " + attr.name + " , this is not allowed.");
                return;
            }
            else
            {
                FieldCommand cmd = new FieldCommand(attr.name, attr.description, this);
                cmd.Initialize(info);
                m_variableDictionary.Add(attr.name, cmd);
                m_masterDictionary.Add(attr.name, cmd);
                m_commandsTrie.Add(new TrieEntry<string>(attr.name, attr.name));
            }
        }

        internal void RegisterAttributeVariableProp(PropertyInfo info, ConsoleVariableAttribute attr)
        {
            m_masterDictionary.TryGetValue(attr.name, out Command comm);
            if (comm != null)
            {
                Debug.LogError("Multiple Attribute Variables with the same name: " + attr.name + " , this is not allowed.");
                return;
            }
            else
            {
                PropertyCommand cmd = new PropertyCommand(attr.name, attr.description, this);
                cmd.Initialize(info);
                m_variableDictionary.Add(attr.name, cmd);
                m_masterDictionary.Add(attr.name, cmd);
                m_commandsTrie.Add(new TrieEntry<string>(attr.name, attr.name));
            }
        }

        internal void UnregisterVariable<T>(string name, object owner)
        {

            m_variableDictionary.TryGetValue(name, out Command comm);
            if (comm != null)
            {
                var variable = comm as Variable<T>;
                variable.Remove(owner);
                return;
            }

            m_variableDictionary.Remove(name);
            m_masterDictionary.Remove(name);
        }
        /// <summary>
        /// Destroy a console variable (so its name can be reused)
        /// </summary>
        internal void UnregisterVariable<T>(Variable<T> variable) where T : new()
        {
            m_variableDictionary.Remove(variable.m_name);
            m_masterDictionary.Remove(variable.m_name);
        }


        private void Echo(string[] parameters)
        {
            string outputMessage = "";
            for (int i = 1; i < parameters.Length; ++i)
            {
                outputMessage += parameters[i] + " ";
            }
            if (outputMessage.EndsWith(" "))
            {
                outputMessage.Substring(0, outputMessage.Length - 1);
            }
            WriteLine(outputMessage);
        }

        //  private static void LastExceptionCallStack(string parameters) {
        //      DumpCallStack(s_lastExceptionCallStack);
        //  }
        //  private static void LastErrorCallStack(string parameters) {
        //      DumpCallStack(s_lastErrorCallStack);
        //  }
        //  private static void LastWarningCallStack(string parameters) {
        //      DumpCallStack(s_lastWarningCallStack);
        //  }

        private void Quit(string[] parameters)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        private void ListCvars(string[] parameters)
        {
            string outputStr = "";
            foreach (Command cmd in m_masterDictionary.Values)
            {
                outputStr += cmd.m_name + " - ";
                outputStr += ConsoleUtility.WrapInColor(greyColor, cmd.m_description) + "\n";

            }
            WriteLine("All Commands : ");
            WriteLine(outputStr);
        }

        public enum myLogType
        {
            error,
            warning,
            confirmation,
            log
        }

        private void LogHandler(string message, string stack, LogType type)
        {
            switch (type)
            {
                case LogType.Assert:
                {
                    WriteLine(ConsoleUtility.WrapInColor(warningPrefix, message));
                    break;
                }
                case LogType.Warning:
                {
                    WriteLine(ConsoleUtility.WrapInColor(warningPrefix, message));
                    break;
                }
                case LogType.Error:
                {
                    WriteLine(ConsoleUtility.WrapInColor(errorPrefix, message));
                    break;
                }
                case LogType.Exception:
                {
                    WriteLine(ConsoleUtility.WrapInColor(errorPrefix, message));
                    break;
                }
                case LogType.Log:
                {
                    WriteLine(ConsoleUtility.WrapInColor(logPrefix, message));
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
        internal string[] CComParameterSplit(string parameters)
        {
            return parameters.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        internal string[] CComParameterSplit(string parameters, int requiredParameters)
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
        internal string[] CVarParameterSplit(string parameters)
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

        internal void DumpCallStack(string stackString)
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


    }
}