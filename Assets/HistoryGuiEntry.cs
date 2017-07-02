namespace BeastConsole.GUI {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BeastConsole.GUI;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    internal class HistoryGuiEntry : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler {

        private string m_line;
        private ConsoleGui m_gui;
        internal static int s_selectedCount;

        internal void Initialize(string line, ConsoleGui gui) {
            m_gui = gui;
            m_line = line;
            GetComponentInChildren<Text>().text = m_line;
        }

        public void OnDeselect(BaseEventData eventData) {
            s_selectedCount--;
        }

        public void OnSelect(BaseEventData eventData) {
            s_selectedCount++;
            Debug.Log("test");
        }

        public void OnSubmit(BaseEventData eventData) {
            BeastConsole.Console.ExecuteLine(m_line);
            m_gui.SelectInput();
        }
    }
}