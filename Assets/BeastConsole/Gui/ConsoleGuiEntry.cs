namespace BeastConsole.GUI {
    using System.Collections;
    using BeastConsole.Backend.Internal;
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteInEditMode]
    public class ConsoleGuiEntry : MonoBehaviour {

        [SerializeField]
        private Text m_entryText;
        public string Text
        {
            get { return m_entryText.text; }
        }
        [SerializeField]
        private Image m_background;

        [SerializeField]
        private LayoutElement m_layoutElement;

        private float m_lineOffset;

        public void Clear() {
            m_entryText.text = "";
        }

        private bool m_requireSetSize;
        private Command m_command;

        private void OnEnable() {
            if (m_requireSetSize)
                StartCoroutine(SetSize());
        }

        public void Initialize(string text, float lineOffset, bool darkline) {
            m_entryText.text = text;
            m_lineOffset = lineOffset;
            m_background.gameObject.SetActive(darkline);

            if (gameObject.activeInHierarchy)
                StartCoroutine(SetSize());
            else
                m_requireSetSize = true;
        }

        internal void SetCommand(Command command) {
            m_command = command;
        }

        public void OnSelected() {
            
        }

        IEnumerator SetSize() {
            for (;;) {
                yield return null;
                yield return null;
                m_layoutElement.preferredHeight = (m_entryText.cachedTextGenerator.lineCount * (m_entryText.fontSize + 2)) + m_lineOffset;
                m_requireSetSize = false;
                yield break;
            }
        }

        private void OnDestroy() {
            StopAllCoroutines();
        }
    }
}