using UnityEngine;
using System.Collections;
using UnityEngine.UI;


/// <summary>
/// Binds text to variable
/// </summary>
public class BindText : MonoBehaviour {

    private Text _text;
    public Text text { get { if (!_text) _text = GetComponent<Text>(); return _text; } }

    public string placeholder;
    public string Prefix;
    public string varName;
    CFG.VariableBase variable;

    void OnEnable()
    {
        text.text = placeholder;
        variable = CFG.GetVar(varName);
        if(variable == null)
            StartCoroutine(getvar());
        else
            variable.OnChangedBase += Sync;
    }


    IEnumerator getvar()
    {
        while(true)
        {
            variable = CFG.GetVar(varName);
            if (variable != null)
                break;
            yield return null;
        }
        variable.OnChangedBase += Sync;
        yield break;
    }

    public void Sync()
    {
        if (variable != null)
            text.text = Prefix + variable.ToString();
    }
}
