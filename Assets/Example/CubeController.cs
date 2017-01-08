using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CubeController : MonoBehaviour {

    public GameObject cube;

    private void OnEnable()
    {
        GameConfig.cfg.Size.OnChanged += SetSize;
    }

    void SetSize(float val)
    {
        Vector3 v = Vector3.one * val;
        cube.transform.localScale = v;
    }
}
