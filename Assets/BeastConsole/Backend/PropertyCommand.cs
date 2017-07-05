namespace BeastConsole.Backend.Internal {

    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class PropertyCommand : Command {

        internal PropertyInfo m_propertyInfo;

        internal Type m_Type;
        internal Type m_declaringType;

        public PropertyCommand(string name, string description, ConsoleBackend backend) : base(name, description, backend) {
        }

        internal void Initialize(PropertyInfo info) {

            m_propertyInfo = info;
            m_Type = m_propertyInfo.PropertyType;
            m_declaringType = m_propertyInfo.DeclaringType;

        }

        internal override void Execute(string line) {

            var split = line.TrimEnd().Split(' ');

            if(split.Length <= 1) {
                return;
            }

            object param = StringToObject(split[1], m_Type);

            var gos = GameObject.FindObjectsOfType(m_declaringType);
            int count = gos.Length;
            for (int i = 0; i < count; i++) {
                m_propertyInfo.SetValue(gos[i], param, null);
            }
        }
    }

}