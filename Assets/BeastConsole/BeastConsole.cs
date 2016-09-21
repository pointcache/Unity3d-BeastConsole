using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BeastConsole : MonoBehaviour {

    public SmartConsole.Options Options;
    public bool ShowFps;
    GameObject consoleRoot;
    GameObject fps_display;
	void Start()
    {
        var evsys = GameObject.FindObjectOfType<EventSystem>();
        if(!evsys)
        {
            Debug.LogError("UnityEvent System not found in scene, manually add it.");
            Debug.Break();
        }
        GameObject prefab = Resources.Load<GameObject>("BeastConsole/BeastConsole");
        consoleRoot = GameObject.Instantiate(prefab);
        consoleRoot.transform.SetParent(transform);
        SmartConsole.options = Options;
        SmartConsole.entryTemplate = Resources.Load<GameObject>("BeastConsole/ConsoleEntry");
        SmartConsole.consoleContent = consoleRoot.transform.FindDeepChild("Content").gameObject;
        SmartConsole.consoleRoot = consoleRoot.transform.FindDeepChild("Root").GetComponent<RectTransform>();
        SmartConsole.inputField = consoleRoot.transform.FindDeepChild("InputField").GetComponent<InputField>();
        SmartConsole.scrollBar = consoleRoot.transform.FindDeepChild("Scrollbar Vertical").GetComponent<Scrollbar>();
        fps_display = consoleRoot.transform.FindDeepChild("fps_display").gameObject;

        CFG.Initialize();
        
        CFG.showfps.OnChanged += x => { fps_display.SetActive(x); CFG.minFPS.Set(120); CFG.maxFPS.Set(0); };
        CFG.showfps.Set(ShowFps);
        consoleRoot.AddComponent<SmartConsole>();
        
        StartCoroutine(TrackFPS());
    }



    IEnumerator TrackFPS()
    {
        float count;
        int frames = 0;
        while (true)
        {
            if (Time.timeScale == 1)
            {
                yield return new WaitForSeconds(.1f);
                count = (1 / Time.deltaTime);
                int fps = (int)Mathf.Round(count);
                frames += fps;
                CFG.currentFPS.Set(fps);
                if (Time.time > 1f)
                {

                    if (fps < CFG.minFPS)
                        CFG.minFPS.Set(fps);
                    if (fps > CFG.maxFPS)
                        CFG.maxFPS.Set(fps);
                    //CFG.avgFPS.SetSilent(CFG.maxFPS - CFG.minFPS);
                }
            }
        }
    }

    


}
public static class TransformDeepChildExtension
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }

}