namespace BeastConsole.Backend.Internal {

    using UnityEngine;
    using System;
    using System.Collections.Generic;

    // SE - this is a bit elaborate, needed to provide a way to do this
    // without relying on memory addresses or pointers... which has resulted in
    // a little blob of bloat and overhead for something that should be trivial... :/
    /// <summary>
    /// A class representing a console variable
    /// </summary>
    internal class Variable<T> : Command {
        public Variable( string name, string desc) {
            m_name = name;
            m_help = desc;
            m_callback = CommandFunction;
        }
        public void Set(T val) // SE: I don't seem to know enough C# to provide a user friendly assignment operator solution
        {
        }

        private void CommandFunction(string parameters) {
            string[] split = m_backend.CVarParameterSplit(parameters);
            if ((split.Length != 0) && m_backend.s_variableDictionary.ContainsKey(split[0])) {
                Variable<T> variable = m_backend.s_variableDictionary[split[0]] as Variable<T>;
                string conjunction = " is set to ";
                if (split.Length == 2) {
                    variable.SetFromString(split[1]);
                    conjunction = " has been set to ";
                }
                m_backend.WriteLine(variable.m_name + conjunction + variable);
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