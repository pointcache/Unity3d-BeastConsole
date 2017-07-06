namespace BeastConsole.Backend.Internal {

    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class FieldCommand : Command {

        internal FieldInfo m_fieldInfo;
        internal Type m_fieldType;
        internal Type m_declaringType;

        //If we detect that the field is an RVar 
        private bool isRVar;

        public FieldCommand(string name, string description, ConsoleBackend backend) : base(name, description, backend) {
        }

        internal void Initialize(FieldInfo info) {

            m_fieldInfo = info;
            if (m_fieldInfo.FieldType.BaseType.Name.Contains("RVar")) {
                isRVar = true;
                m_fieldType = m_fieldInfo.FieldType.BaseType.GetGenericArguments()[0];
            }
            else {
                m_fieldType = m_fieldInfo.FieldType;
            }
            m_declaringType = m_fieldInfo.DeclaringType;
        }

        internal override void Execute(string line) {

            var split = line.TrimEnd().Split(' ');

            if (split.Length <= 1) {
                return;
            }

            var gos = GameObject.FindObjectsOfType(m_declaringType);
            int count = gos.Length;
            object param = StringToObject(split[1], m_fieldType);

            for (int i = 0; i < count; i++) {
                if (isRVar) {
                    object rvar = m_fieldInfo.GetValue(gos[i]);
                    PropertyInfo setter = rvar.GetType().GetProperty("Value");
                    setter.SetValue(rvar, param, null);
                }
                else {
                    m_fieldInfo.SetValue(gos[i], param);

                }
            }
        }
    }
}
