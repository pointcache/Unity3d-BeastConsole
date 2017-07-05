namespace BeastConsole.Backend.Internal {

    using System;
    using System.Collections.Generic;

    // SE - this is a bit elaborate, needed to provide a way to do this
    // without relying on memory addresses or pointers... which has resulted in
    // a little blob of bloat and overhead for something that should be trivial... :/
    /// <summary>
    /// A class representing a console variable
    /// </summary>
    internal class Variable<T> : Command {

        internal Action<T> m_setter;
        internal Dictionary<object, Action<T>> m_dict = new Dictionary<object, Action<T>>();

        internal Variable(string name, string desc, Action<T> setter, object owner, ConsoleBackend backend) : base(name, desc, backend) {
            Add(owner, setter);
        }

        internal void Set(T val) // SE: I don't seem to know enough C# to provide a user friendly assignment operator solution
        {
            m_setter(val);
        }

        internal void Add(object owner, Action<T> setter) {
            m_dict.Add(owner, setter);
            m_setter += setter;
        }

        internal void Remove(object owner) {
            m_setter -= m_dict[owner];
        }

        internal override void Execute(string parameters) {
            string[] split = m_backend.CVarParameterSplit(parameters);
            if ((split.Length > 1) && m_backend.m_variableDictionary.ContainsKey(split[0])) {
                Variable<T> variable = m_backend.m_variableDictionary[split[0]] as Variable<T>;
                string conjunction = " is set to ";
                if (split.Length == 2) {
                    variable.SetFromString(split[1]);
                    conjunction = " has been set to ";
                }
                m_backend.WriteLine(variable.m_name + conjunction + split[1]);
            }
        }

        private void SetFromString(string value) {
            if (typeof(T) == typeof(bool)) {
                if (value == "0") {
                    Set((T)System.Convert.ChangeType("false", typeof(T)));
                }
                else if (value == "1") {
                    Set((T)System.Convert.ChangeType("true", typeof(T)));
                }
                else
                    Set((T)System.Convert.ChangeType(value, typeof(T)));
            }
            else
                Set((T)System.Convert.ChangeType(value, typeof(T)));
        }
    }
}