namespace BeastConsole.GUI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using System;

    public class AutoCompleteGuiEntry : GuiBase, ISelectHandler, IDeselectHandler, ISubmitHandler {

        private string m_line;
        internal static int s_selectedCount;
        private ConsoleGui m_gui;

        internal void Initialize(string text, ConsoleGui gui) {
            m_gui = gui;
            m_line = text;
            GetComponentInChildren<Text>().text = m_line;
        }

        public void OnDeselect(BaseEventData eventData) {
            s_selectedCount--;
        }

        public void OnSelect(BaseEventData eventData) {
            s_selectedCount++;
        }

        public void OnSubmit(BaseEventData eventData) {
            m_gui.SetInputText("");
            m_gui.SelectInput();
            string line = m_line;
            if (line[line.Length - 1] != ' ')
                line = m_line + " ";
            m_gui.SetInputText(line);
        }
    }
}