namespace BeastConsole.Backend.Internal {

    using UnityEngine;
    using System;
    using System.Collections.Generic;

    internal class Command {
        internal Action<string[]> m_command = null;
        internal string m_name = null;
        internal string m_paramsExample = "";
        internal string m_help = "(no description)";
        internal ConsoleBackend m_backend;

        internal Dictionary<object, Action<string[]>> m_commanddict = new Dictionary<object, Action<string[]>>();

        internal void AddCommand(object owner, Action<string[]> command) {
            m_commanddict.Add(owner, command);
            m_command += command;
        }

        internal void RemoveCommand(object owner) {
            m_command -= m_commanddict[owner];
        }

        internal virtual void Execute(string line) {
            if (m_command == null) {
                BeastConsole.Console.WriteLine("Missing target method for command: " + line);
            }
            else
                m_command(line.Split(' '));
        }

        public override string ToString() {
            return m_name;
        }
    }
}