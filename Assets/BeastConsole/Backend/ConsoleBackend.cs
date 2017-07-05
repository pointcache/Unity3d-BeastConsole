namespace BeastConsole.Backend {

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
    using BeastConsole.Backend.Internal;
    using System.Collections;
    using System.Collections.Generic;
    using System;
    using System.Reflection;
    using System.Linq;

    // SE: broadly patterned after the debug console implementation from GLToy...
    // https://code.google.com/p/gltoy/source/browse/trunk/GLToy/Independent/Core/Console/GLToy_Console.h
    /// <summary>
    /// A Quake style debug console - should be added to an otherwise empty game object and have a font set in the inspector
    /// </summary>
    internal class ConsoleBackend {

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


        // --- internals
        internal ConsoleBackend() {
            // run this only once...
            if (m_textInput != null) {
                return;
            }
#if UNITY_EDITOR
            // Application.logMessageReceived += LogHandler;
#endif

            RegisterCommand("echo", "writes <string> to the console log (alias for echo)", this, Echo);
            RegisterCommand("list", "lists all currently registered console variables", this, ListCvars);
            RegisterCommand("print", "writes <string> to the console log", this, Echo);
            RegisterCommand("quit", "quit the game (not sure this works with iOS/Android)", this, Quit);
            //RegisterCommand("help", "displays help information for console command where available", this, Help);
            // RegisterCommand("callstack.warning", "display the call stack for the last warning message", LastWarningCallStack);
            // RegisterCommand("callstack.error", "display the call stack for the last error message", LastErrorCallStack);
            // RegisterCommand("callstack.exception", "display the call stack for the last exception message", LastExceptionCallStack);
            GetAllConsoleCommandAttributes();
            GetAllConsoleVariableAttributes();
        }

        internal void GetAllConsoleCommandAttributes() {

            var methods = Assembly.GetCallingAssembly().GetTypes()
                  .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                  .Where(m => m.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).Length > 0)
                  .ToArray();

            foreach (var m in methods) {
                RegisterAttributeCommand(m, m.GetCustomAttributes(typeof(ConsoleCommandAttribute), false)[0] as ConsoleCommandAttribute);
            }
        }

        internal void GetAllConsoleVariableAttributes() {

            var fields = Assembly.GetCallingAssembly().GetTypes()
                  .SelectMany(t => t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                  .Where(m => m.GetCustomAttributes(typeof(ConsoleVariableAttribute), false).Length > 0)
                  .ToArray();

            foreach (var f in fields) {
                RegisterAttributeVariableField(f, f.GetCustomAttributes(typeof(ConsoleVariableAttribute), false)[0] as ConsoleVariableAttribute);
            }

            var props = Assembly.GetCallingAssembly().GetTypes()
                  .SelectMany(t => t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                  .Where(m => m.GetCustomAttributes(typeof(ConsoleVariableAttribute), false).Length > 0)
                  .ToArray();

            foreach (var p in props) {
                RegisterAttributeVariableProp(p, p.GetCustomAttributes(typeof(ConsoleVariableAttribute), false)[0] as ConsoleVariableAttribute);
            }
        }

        internal void WriteLine(string line) {
            //string msg = DeNewLine(line);
            m_outputHistory.Add(line);
            OnWriteLine(line);
        }

        internal string DeNewLine(string message) {
            return message.Replace("\n", " | ");
        }


        internal void Print(string message) {
            WriteLine(message);
        }


        /// <summary>
        /// Execute a string as if it were a single line of input to the console
        /// </summary>
        internal void ExecuteLine(string inputLine) {
            string[] words = CComParameterSplit(inputLine);
            if (words.Length > 0) {
                Command com = null;
                m_masterDictionary.TryGetValue(words[0], out com);
                if (com != null) {
                    WriteLine("<b>> </b><color=lime>" + inputLine + "</color>");
                    m_masterDictionary[words[0]].Execute(inputLine);
                    m_commandHistory.Add(inputLine);
                    OnExecutedCommand(inputLine, com);
                }
                else {
                    WriteLine("<color=red>Unrecognised command or variable name: " + words[0] + "</color>");
                }
            }
        }
        // public static void ExecuteFile( string path ) {} //...
        internal void RemoveCommandIfExists(string name, object owner) {
            Command comm = null;
            m_commandDictionary.TryGetValue(name, out comm);
            if (comm != null) {
                comm.RemoveCommand(owner);
                if (comm.m_command.GetInvocationList().Length == 0) {
                    m_commandDictionary.Remove(name);
                    m_masterDictionary.Remove(name);
                }
            }
        }
        /// <summary>
        /// Register a console command with an example of usage and a help description
        /// e.g. SmartConsole.RegisterCommand( "echo", "echo <string>", "writes <string> to the console log", SmartConsole.Echo );
        /// </summary>
        internal void RegisterCommand(string name, string helpDescription, object owner, Action<string[]> callback) {

            Command comm = null;
            m_masterDictionary.TryGetValue(name, out comm);
            if (comm != null) {
                comm.AddCommand(owner, callback);
                return;
            }
            else {
                Command command = new Command(name, helpDescription, this);
                command.AddCommand(owner, callback);
                m_commandDictionary.Add(name, command);
                m_masterDictionary.Add(name, command);
                m_commandsTrie.Add(new TrieEntry<string>(name, name));
            }
        }

        private void RegisterAttributeCommand(MethodInfo info, ConsoleCommandAttribute attr) {
            Command comm = null;
            m_masterDictionary.TryGetValue(attr.name, out comm);
            if (comm != null) {
                Debug.LogError("Multiple Attribute ConsoleCommands with the same name: " + attr.name + " , this is not allowed.");
                return;
            }
            else {
                AttributeCommand cmd = new AttributeCommand(attr.name, attr.description, this);
                cmd.Initialize(info);
                m_attributeCommandsDictionary.Add(attr.name, cmd);
                m_masterDictionary.Add(attr.name, cmd);
                m_commandsTrie.Add(new TrieEntry<string>(attr.name, attr.name));
            }
        }

        internal void RegisterVariable<T>(Action<T> setter, object owner, string name, string desc) {
            Command comm = null;
            m_masterDictionary.TryGetValue(name, out comm);
            if (comm != null) {
                var variable = comm as Variable<T>;
                variable.Add(owner, setter);
                return;
            }
            else {
                Variable<T> returnValue = new Variable<T>(name, desc, setter, owner, this);
                m_variableDictionary.Add(name, returnValue);
                m_masterDictionary.Add(name, returnValue);
                m_commandsTrie.Add(new TrieEntry<string>(name, name));

            }
        }

        internal void RegisterAttributeVariableField(FieldInfo info, ConsoleVariableAttribute attr) {
            Command comm = null;
            m_masterDictionary.TryGetValue(attr.name, out comm);
            if (comm != null) {
                Debug.LogError("Multiple Attribute Variables with the same name: " + attr.name + " , this is not allowed.");
                return;
            }
            else {
                FieldCommand cmd = new FieldCommand(attr.name, attr.description, this);
                cmd.Initialize(info);
                m_variableDictionary.Add(attr.name, cmd);
                m_masterDictionary.Add(attr.name, cmd);
                m_commandsTrie.Add(new TrieEntry<string>(attr.name, attr.name));
            }
        }

        internal void RegisterAttributeVariableProp(PropertyInfo info, ConsoleVariableAttribute attr) {
            Command comm = null;
            m_masterDictionary.TryGetValue(attr.name, out comm);
            if (comm != null) {
                Debug.LogError("Multiple Attribute Variables with the same name: " + attr.name + " , this is not allowed.");
                return;
            }
            else {
                PropertyCommand cmd = new PropertyCommand(attr.name, attr.description, this);
                cmd.Initialize(info);
                m_variableDictionary.Add(attr.name, cmd);
                m_masterDictionary.Add(attr.name, cmd);
                m_commandsTrie.Add(new TrieEntry<string>(attr.name, attr.name));
            }
        }

        internal void UnregisterVariable<T>(string name, object owner) {

            Command comm = null;
            m_variableDictionary.TryGetValue(name, out comm);
            if (comm != null) {
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
        internal void UnregisterVariable<T>(Variable<T> variable) where T : new() {
            m_variableDictionary.Remove(variable.m_name);
            m_masterDictionary.Remove(variable.m_name);
        }

        //private void Help(string[] parameters) {
        //    // try and lay it out nicely...
        //    const int nameLength = 25;
        //    const int exampleLength = 35;
        //    foreach (Command command in m_commandDictionary.Values) {
        //        string outputString = command.m_name;
        //        for (int i = command.m_name.Length; i < nameLength; ++i) {
        //            outputString += " ";
        //        }
        //        if (command.m_paramsExample.Length > 0) {
        //            outputString += " example: " + command.m_paramsExample;
        //        }
        //        else {
        //            outputString += "          ";
        //        }
        //        for (int i = command.m_paramsExample.Length; i < exampleLength; ++i) {
        //            outputString += " ";
        //        }
        //        WriteLine(outputString + command.m_description);
        //    }
        //}

        private void Echo(string[] parameters) {
            string outputMessage = "";
            for (int i = 1; i < parameters.Length; ++i) {
                outputMessage += parameters[i] + " ";
            }
            if (outputMessage.EndsWith(" ")) {
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

        private void Quit(string[] parameters) {
            Application.Quit();
        }

        private void ListCvars(string[] parameters) {
            // try and lay it out nicely...
            const int nameLength = 50;
            foreach (Command variable in m_variableDictionary.Values) {
                string outputString = variable.m_name;
                for (int i = variable.m_name.Length; i < nameLength; ++i) {
                    outputString += " ";
                }
                WriteLine(outputString + variable.m_description);
            }
        }



        //public enum myLogType {
        //    error,
        //    warning,
        //    confirmation,
        //    log
        //}
        // public static void Log(string msg, myLogType type) {
        //     string confirmPrefix = "<color=#00ff72>[OK!]: ";
        //     string errorPrefix = "<color=#ff225b>[ERR]: ";
        //     string warningPrefix = "<color=#ffa922>[WNG]: ";
        //     string otherPrefix = "<color=white>[LOG]: ";
        //     string prefix = otherPrefix;
        //     switch (type) {
        //         case myLogType.confirmation: {
        //                 prefix = confirmPrefix;
        //                 break;
        //             }
        //         case myLogType.warning: {
        //                 prefix = warningPrefix;
        //                 break;
        //             }
        //         case myLogType.error: {
        //                 prefix = errorPrefix;
        //                 break;
        //             }
        //         case myLogType.log: {
        //                 prefix = otherPrefix;
        //                 break;
        //             }
        //         default: {
        //                 break;
        //             }
        //     }
        //     WriteLine(prefix + msg + "</color>");
        // }
        // private static void LogHandler(string message, string stack, LogType type) {
        //     switch (type) {
        //         case LogType.Assert: {
        //                 Log(message, myLogType.warning);
        //                 break;
        //             }
        //         case LogType.Warning: {
        //                 Log(message, myLogType.warning);
        //                 s_lastWarningCallStack = stack;
        //                 break;
        //             }
        //         case LogType.Error: {
        //                 Log(message, myLogType.error);
        //                 s_lastErrorCallStack = stack;
        //                 break;
        //             }
        //         case LogType.Exception: {
        //                 Log(message, myLogType.error);
        //                 s_lastExceptionCallStack = stack;
        //                 break;
        //             }
        //         case LogType.Log: {
        //                 Log(message, myLogType.log);
        //                 s_lastExceptionCallStack = stack;
        //                 break;
        //             }
        //         default: {
        //                 break;
        //             }
        //     }
        // }
        internal string[] CComParameterSplit(string parameters) {
            return parameters.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        internal string[] CComParameterSplit(string parameters, int requiredParameters) {
            string[] split = CComParameterSplit(parameters);
            if (split.Length < (requiredParameters + 1)) {
                WriteLine("Error: not enough parameters for command. Expected " + requiredParameters + " found " + (split.Length - 1));
            }
            if (split.Length > (requiredParameters + 1)) {
                int extras = ((split.Length - 1) - requiredParameters);
                WriteLine("Warning: " + extras + "additional parameters will be dropped:");
                for (int i = split.Length - extras; i < split.Length; ++i) {
                    WriteLine("\"" + split[i] + "\"");
                }
            }
            return split;
        }
        internal string[] CVarParameterSplit(string parameters) {
            string[] split = CComParameterSplit(parameters);
            if (split.Length == 0) {
                WriteLine("Error: not enough parameters to set or display the value of a console variable.");
            }
            if (split.Length > 2) {
                int extras = (split.Length - 3);
                WriteLine("Warning: " + extras + "additional parameters will be dropped:");
                for (int i = split.Length - extras; i < split.Length; ++i) {
                    WriteLine("\"" + split[i] + "\"");
                }
            }
            return split;
        }

        internal void DumpCallStack(string stackString) {
            string[] lines = stackString.Split(new char[] { '\r', '\n' });
            if (lines.Length == 0) {
                return;
            }
            int ignoreCount = 0;
            while ((lines[lines.Length - 1 - ignoreCount].Length == 0) && (ignoreCount < lines.Length)) {
                ++ignoreCount;
            }
            int lineCount = lines.Length - ignoreCount;
            for (int i = 0; i < lineCount; ++i) {
                // SE - if the call stack is 100 deep without recursion you have much bigger problems than you can ever solve with a debugger...
                WriteLine((i + 1).ToString() + ((i < 9) ? "  " : " ") + lines[i]);
            }
        }
    }
}