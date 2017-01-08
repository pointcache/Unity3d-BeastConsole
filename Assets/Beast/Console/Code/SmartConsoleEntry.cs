using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SmartConsoleEntry : MonoBehaviour
{

    public Text entryText;

    public void SetText(string text)
    {
        entryText.text = text;
    }

    public void Clear()
    {
        entryText.text = "";
    }
}
