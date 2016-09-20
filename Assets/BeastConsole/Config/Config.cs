using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static partial class CFG
{
    #region Player
    public static Variable<float> health;
    #endregion
    
    #region UI
    public static Variable<bool> consoleOpened;
    public static Variable<bool> forbidMovement;
    #endregion

    #region Misc
    public static Variable<bool> output_unity_log;
    public static Variable<bool> write_game_log_to_file;
    #endregion
    #region Audio
    public static Variable<float> masterVolume;
    public static Variable<float> musicVolume;
    public static Variable<float> ambientVolume;
    public static Variable<float> fxVolume;
    public static Variable<float> uiVolume;
    public static Variable<float> voiceVolume;

    #endregion

    #region Graphics
    public static Variable<int> targetFramerate;
    public static Variable<int> currentFPS;
    public static Variable<int> minFPS;
    public static Variable<int> maxFPS;
    public static Variable<float> avgFPS;
    public static Variable<float> fov;
    #endregion

    #region Controls
    public static Variable<float> sensitivity;
    #endregion

    #region Game
    public static Variable<float> gravity;
    # endregion
        
    static void reg_CONTROLS()
    {
        sensitivity = new Variable<float>("", "sensitivity", "Mouse Sensitivity", true);
    }

    static void reg_GFX()
    {
        targetFramerate = new Variable<int>(GraphicsGroup, "targetfps", "set max fps for game",  true);
        NewSettingsVar(targetFramerate as VariableBase, 0f, 300f);

        fov = new Variable<float>("", "fov", "Set field of view",  true);
    }

    static void reg_PLAYER()
    {
        health = new Variable<float>(PlayerGroup, "health", "health of the player", true);
    }

    static void reg_GAME()
    {
        gravity = new Variable<float>("", "gravity", "Sets the global gravity", true);
    }

    static void reg_UI()
    {
        forbidMovement = new Variable<bool>(UiGroup, "forbidmovement",  "",  false);
        consoleOpened = new Variable<bool>(UiGroup, "consoleopened", "", false);
    }

    static void reg_MISC()
    {
        output_unity_log = new Variable<bool>("log", "output_unity_log",  "relay log messages to unity logger", true);
        currentFPS = new Variable<int>("", "fps", "current fps", false);
    }

    static void reg_AUDIO()
    {
        masterVolume = new Variable<float>(AudioGroup, "mastervolume", "Control Master Volume", true);
        NewSettingsVar(masterVolume as VariableBase, 0f, 100f);

        musicVolume = new Variable<float>(AudioGroup, "musicvolume", "Control Music Volume",  true);
        NewSettingsVar(musicVolume as VariableBase, 0f, 100f);

        ambientVolume = new Variable<float>(AudioGroup, "ambientvolume", "Control Ambient Volume", true);
        NewSettingsVar(ambientVolume as VariableBase, 0f, 100f);

        fxVolume = new Variable<float>(AudioGroup, "fxvolume", "Control Fx Volume", true);
        NewSettingsVar(fxVolume as VariableBase, 0f, 100f);

        uiVolume = new Variable<float>(AudioGroup, "uivolume", "Control Ui Volume", true);
        NewSettingsVar(uiVolume as VariableBase, 0f, 100f);

        voiceVolume = new Variable<float>(AudioGroup, "voicevolume", "Control Voice Volume",  true);
        NewSettingsVar(voiceVolume as VariableBase, 0f, 100f);
    }


}

