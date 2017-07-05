namespace BeastConsole {
    using System;

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ConsoleCommandAttribute : Attribute {

        public readonly string name, description;

        public ConsoleCommandAttribute(string name, string description) {
            this.name = name;
            this.description = description;
        }
    }
}