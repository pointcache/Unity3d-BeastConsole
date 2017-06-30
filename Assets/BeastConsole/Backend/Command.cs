namespace BeastConsole.Backend.Internal {

    using UnityEngine;
    using System;
    using System.Collections.Generic;

    // control the general layout here
    // SE: annoying having to leak this out publicly - basically to facilitate the weird and wonderful cvar implementation
    /// <summary>
    /// A class representing a console command - WARNING: this is only exposed as a hack!
    /// </summary>
    internal class Command {
        public delegate void ConsoleCommandFunction(string parameters);

        public ConsoleCommandFunction m_callback = null;
        public string m_name = null;
        public string m_paramsExample = "";
        public string m_help = "(no description)";
        public Backend m_backend;
    }
}