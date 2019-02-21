namespace BeastConsole.Backend.Internal
{
    using System;
    using System.Collections.Generic;

    internal class Command
    {
        internal Action<string[]> m_command = null;
        internal string m_name = null;
        internal string m_description = "(no description)";
        internal ConsoleBackend m_backend;

        internal Dictionary<object, Action<string[]>> m_commanddict = new Dictionary<object, Action<string[]>>();

        internal Command(string name, string description, ConsoleBackend backend)
        {
            m_name = name;
            m_description = description;
            m_backend = backend;
        }

        internal void AddCommand(object owner, Action<string[]> command)
        {
            m_commanddict.Add(owner, command);
            m_command += command;
        }

        internal void RemoveCommand(object owner)
        {
            m_command -= m_commanddict[owner];
        }

        internal virtual void Execute(string line)
        {
            if (m_command == null)
            {
                BeastConsole.Console.WriteLine("Missing target method for command: " + line);
            }
            else
            {
                m_command(line.Split(' '));
            }
        }

        public override string ToString()
        {
            return m_name;
        }

        protected object StringToObject(string value, Type type)
        {
            object result = null;

            try
            {
                if (type == typeof(bool))
                {
                    if (value == "0")
                    {
                        result = System.Convert.ChangeType("false", typeof(bool));
                    }
                    else if (value == "1")
                    {
                        result = System.Convert.ChangeType("true", typeof(bool));
                    }
                    else
                        result = System.Convert.ChangeType(value, type);
                }
                else
                    result = System.Convert.ChangeType(value, type);
            }
            catch (Exception e)
            {
                BeastConsole.Console.WriteLine("command/variable | " + m_name + " : " + e.Message);
            }

            return result;
        }
    }
}