namespace BeastConsole.GUI {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BeastConsole.GUI;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    internal class HistoryGuiEntry : GuiBase, ISelectHandler, IDeselectHandler, ISubmitHandler {

        private string m_line;
        private ConsoleGui m_gui;
        internal static HistoryGuiEntry s_selected;

        internal void Initialize(string line, ConsoleGui gui) {
            m_gui = gui;
            m_line = line;
            GetComponentInChildren<Text>().text = m_line;
        }

        public void OnDeselect(BaseEventData eventData) {
            s_selected = null;
        }

        public void OnSelect(BaseEventData eventData) {
            s_selected = this;
        }

        public void OnSubmit(BaseEventData eventData) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                BeastConsole.Console.ExecuteLine(m_line);
            }
            else {
                string line = m_line;
                if (line[line.Length - 1] != ' ')
                    line = m_line + " ";
                m_gui.SetInputText(line);
            }
            m_gui.SelectInput();
        }
    }
}