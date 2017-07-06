namespace BeastConsole {
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class ConsoleVariableAttribute : Attribute {

        public readonly string name, description;

        public ConsoleVariableAttribute(string name, string description) {
            this.name = name;
            this.description = description;
        }

        public ConsoleVariableAttribute(string name) {
            this.name = name;
            this.description = "no description";
        }
    }
}