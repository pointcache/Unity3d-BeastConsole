namespace BeastConsole.GUI {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    internal class ConsoleGuiInputField : MonoBehaviour, ISelectHandler {

        private ConsoleGui m_gui;
        private InputField m_inputField;

        public void OnSelect(BaseEventData eventData) {
            StartCoroutine(DeselectAndGoToEnd());
        }

        private IEnumerator DeselectAndGoToEnd() {
            for (;;) {
                yield return null;
                m_inputField.ActivateInputField();
                m_inputField.selectionAnchorPosition = 0;
                m_inputField.selectionFocusPosition = 0;
                m_inputField.MoveTextEnd(false);
                yield break;
            }
        }

        private void Awake() {
            m_gui = GetComponentInParent<ConsoleGui>();
            m_inputField = GetComponent<InputField>();
        }
    }
}