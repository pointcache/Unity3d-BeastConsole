namespace BeastConsole.GUI {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ConsoleGuiInputField : MonoBehaviour {

        private ConsoleGui m_gui;
        private InputField m_inputField;

        private void Awake() {
            m_gui = GetComponentInParent<ConsoleGui>();
            m_inputField = GetComponent<InputField>();
        }
    }
}