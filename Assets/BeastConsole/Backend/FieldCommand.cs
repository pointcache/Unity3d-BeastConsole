namespace BeastConsole.Backend.Internal {

    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class FieldCommand : Command {

        internal FieldInfo m_fieldInfo;
        internal Type m_fieldType;
        internal Type m_declaringType;

        public FieldCommand(string name, string description, ConsoleBackend backend) : base(name, description, backend) {
        }

        internal void Initialize(FieldInfo info) {

            m_fieldInfo = info;
            m_fieldType = m_fieldInfo.FieldType;
            m_declaringType = m_fieldInfo.DeclaringType;

        }

        internal override void Execute(string line) {

            var split = line.TrimEnd().Split(' ');

            if(split.Length <= 1) {
                return;
            }

            object param = StringToObject(split[1], m_fieldType);

            var gos = GameObject.FindObjectsOfType(m_declaringType);
            int count = gos.Length;
            for (int i = 0; i < count; i++) {
                m_fieldInfo.SetValue(gos[i], param);
            }
        }
    }
}