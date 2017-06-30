namespace BeastConsole.GUI {

    using UnityEngine;
    using UnityEngine.UI;

    public class ConsoleGuiEntry : MonoBehaviour {

        public Text entryText;

        public void SetText(string text) {
            entryText.text = text;
        }

        public void Clear() {
            entryText.text = "";
        }
    }
}