using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BindSliderToFloat : MonoBehaviour {
    public string varName;
    public bool doUpdate;
    CFG.VariableBase variable;
    CFG.Variable<float> floatvar;
    Slider _slider;
    // Use this for initialization
    void Start()
    {
        _slider = GetComponent<Slider>();
        variable = CFG.GetVar(varName);
        floatvar = (CFG.Variable<float>)variable;
    }

    void Update()
    {
        if (!doUpdate)
            return;
        if (variable != null)
            _slider.value = floatvar;
    }
}
