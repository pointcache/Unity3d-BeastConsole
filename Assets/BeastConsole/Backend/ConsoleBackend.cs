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

    // SE: broadly patterned after the debug console implementation from GLToy...
    // https://code.google.com/p/gltoy/source/browse/trunk/GLToy/Independent/Core/Console/GLToy_Console.h
    /// <summary>
    /// A Quake style debug console - should be added to an otherwise empty game object and have a font set in the inspector
    /// </summary>
    internal class ConsoleBackend {

        internal Action<string> OnWriteLine = delegate { };
        internal Action<string> OnExecutedLine = delegate { };

        internal GameObject s_textInput = null;
        internal AutoCompleteDictionary<Command> s_commandDictionary = new AutoCompleteDictionary<Command>();
        internal AutoCompleteDictionary<Command> s_variableDictionary = new AutoCompleteDictionary<Command>();
        internal AutoCompleteDictionary<Command> s_masterDictionary = new AutoCompleteDictionary<Command>();
        internal List<string> s_commandHistory = new List<string>();
        internal List<string> s_outputHistory = new List<string>();
        internal string s_lastExceptionCallStack = "(none yet)";
        internal string s_lastErrorCallStack = "(none yet)";
        internal string s_lastWarningCallStack = "(none yet)";


        // --- internals
        internal ConsoleBackend() {
            // run this only once...
            if (s_textInput != null) {
                return;
            }
#if UNITY_EDITOR
            // Application.logMessageReceived += LogHandler;
#endif


            RegisterCommand("echo", "echo <string>", "writes <string> to the console log (alias for echo)", Echo);
            RegisterCommand("help", "displays help information for console command where available", Help);
            RegisterCommand("list", "lists all currently registered console variables", ListCvars);
            RegisterCommand("print", "print <string>", "writes <string> to the console log", Echo);
            RegisterCommand("quit", "quit the game (not sure this works with iOS/Android)", Quit);
           // RegisterCommand("callstack.warning", "display the call stack for the last warning message", LastWarningCallStack);
           // RegisterCommand("callstack.error", "display the call stack for the last error message", LastErrorCallStack);
           // RegisterCommand("callstack.exception", "display the call stack for the last exception message", LastExceptionCallStack);

        }

        internal void WriteLine(string line) {
            string msg = DeNewLine(line);
            s_outputHistory.Add(msg);
            OnWriteLine(msg);
        }

        internal string DeNewLine(string message) {
            return message.Replace("\n", " | ");
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
        public void Print(string message) {
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

        /// <summary>
        /// Execute a string as if it were a single line of input to the console
        /// </summary>
        public void ExecuteLine(string inputLine) {
            string[] words = CComParameterSplit(inputLine);
            if (words.Length > 0) {
                if (s_masterDictionary.ContainsKey(words[0])) {
                    WriteLine("<b>=> </b><color=lime>" + inputLine + "</color>");
                    s_masterDictionary[words[0]].m_callback(inputLine);
                }
                else {
                    WriteLine("<color=red>Unrecognised command or variable name: " + words[0] + "</color>");
                }
                s_commandHistory.Add(inputLine);
                OnExecutedLine(inputLine);
            }
        }
        // public static void ExecuteFile( string path ) {} //...
        public void RemoveCommandIfExists(string name) {
            s_commandDictionary.Remove(name);
            s_masterDictionary.Remove(name);
        }
        /// <summary>
        /// Register a console command with an example of usage and a help description
        /// e.g. SmartConsole.RegisterCommand( "echo", "echo <string>", "writes <string> to the console log", SmartConsole.Echo );
        /// </summary>
        public void RegisterCommand(string name, string exampleUsage, string helpDescription, Command.ConsoleCommandFunction callback) {
            Command command = new Command();
            command.m_name = name;
            command.m_paramsExample = exampleUsage;
            command.m_help = helpDescription;
            command.m_callback = callback;
            command.m_backend = this;
            s_commandDictionary.Add(name, command);
            s_masterDictionary.Add(name, command);
        }
        /// <summary>
        /// Register a console command with a help description
        /// e.g. SmartConsole.RegisterCommand( "help", "displays help information for console command where available", SmartConsole.Help );
        /// </summary>
        public  void RegisterCommand(string name, string helpDescription, Command.ConsoleCommandFunction callback) {
            RegisterCommand(name, "", helpDescription, callback);
        }
        /// <summary>
        /// Register a console command
        /// e.g. SmartConsole.RegisterCommand( "foo", Foo );
        /// </summary>
        public  void RegisterCommand(string name, Command.ConsoleCommandFunction callback) {
            RegisterCommand(name, "", "(no description)", callback);
        }
        // public static void RegisterVariable<T>(rVar<T> var, string name, string desc) {
        //     if (s_variableDictionary.ContainsKey(name)) {
        //         Debug.LogError("Tried to add already existing console variable!");
        //         return;
        //     }
        //     Variable<T> returnValue = new Variable<T>(var, name, desc);
        //     s_variableDictionary.Add(name, returnValue);
        //     s_masterDictionary.Add(name, returnValue);
        // }
        /// <summary>
        /// Destroy a console variable (so its name can be reused)
        /// </summary>
        public void UnregisterVariable(string name) {
            s_variableDictionary.Remove(name);
            s_masterDictionary.Remove(name);
        }
        /// <summary>
        /// Destroy a console variable (so its name can be reused)
        /// </summary>
        public void UnregisterVariable<T>(Variable<T> variable) where T : new() {
            s_variableDictionary.Remove(variable.m_name);
            s_masterDictionary.Remove(variable.m_name);
        }

        private void Help(string parameters) {
            // try and lay it out nicely...
            const int nameLength = 25;
            const int exampleLength = 35;
            foreach (Command command in s_commandDictionary.Values) {
                string outputString = command.m_name;
                for (int i = command.m_name.Length; i < nameLength; ++i) {
                    outputString += " ";
                }
                if (command.m_paramsExample.Length > 0) {
                    outputString += " example: " + command.m_paramsExample;
                }
                else {
                    outputString += "          ";
                }
                for (int i = command.m_paramsExample.Length; i < exampleLength; ++i) {
                    outputString += " ";
                }
                WriteLine(outputString + command.m_help);
            }
        }

        private void Echo(string parameters) {
            string outputMessage = "";
            string[] split = CComParameterSplit(parameters);
            for (int i = 1; i < split.Length; ++i) {
                outputMessage += split[i] + " ";
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

        private void Quit(string parameters) {
            Application.Quit();
        }

        private void ListCvars(string parameters) {
            // try and lay it out nicely...
            const int nameLength = 50;
            foreach (Command variable in s_variableDictionary.Values) {
                string outputString = variable.m_name;
                for (int i = variable.m_name.Length; i < nameLength; ++i) {
                    outputString += " ";
                }
                WriteLine(outputString + variable.m_help);
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
        public string[] CComParameterSplit(string parameters) {
            return parameters.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        public string[] CComParameterSplit(string parameters, int requiredParameters) {
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