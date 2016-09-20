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
    
    //All initialization of variables
    static CFG()
    {
        reg_GFX();
        reg_PLAYER();
        reg_GAME();
        reg_MISC();
        reg_UI();
        reg_AUDIO();
        reg_CONTROLS();
    }

    public const string AudioGroup = "sound";
    public const string GraphicsGroup = "gfx";
    public const string AppGroup = "app";
    public const string ControlsGroup = "ctrl";
    public const string PlayerGroup = "player";
    public const string UiGroup = "ui";

    /// <summary>
    /// Used to create a variable outside of CFG static vars for use in debugging/ui
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="group"></param>
    /// <param name="name"></param>
    /// <param name="initval"></param>
    public static Variable<T> NEW_EXTERNAL_VAR<T>(string group, string name, T initval) where T : new()
    {
        var VAR = NewVar<T>(group, name, "", "", initval, null, false, false);
        return VAR;
    }

    static void reg_PLAYER()
    {
        health = NewVar(PlayerGroup, "health", "health of the player", "", 100f, x => health.SetSilent(x), false, true);
    }

    static void reg_GAME()
    {
        gravity = NewVar("", "gravity", "Gravity", "Sets the global gravity", config.game.GravForce, x => gravity.SetSilent(x), true, true);

    }

    static void reg_UI()
    {
        forbidMovement = NewVar(UiGroup, "forbidmovement", "", "", false, x => forbidMovement.SetSilent(x), false, false);
        consoleOpened = NewVar(UiGroup, "consoleopened", "", "", false, x => consoleOpened.SetSilent(x), false, false);
    }

    static void reg_MISC()
    {
        output_unity_log = NewVar("log", "output_unity_log", "", "relay log messages to unity logger", config.misc.output_unity_log, x => output_unity_log.SetSilent(x), true, true);
        write_game_log_to_file = NewVar("log", "write_game_log", "", "are we writing to alternative log file in game directory", config.misc.write_log_to_file, x => write_game_log_to_file.SetSilent(x), true, true);
        currentFPS = NewVar("", "fps", "current fps", "", 0, null, false, false);
        minFPS = NewVar("", "minfps", "minimum fps since last reset", "", 10000, null, false, false);
        maxFPS = NewVar("", "maxfps", "maximum fps since last reset", "", 0, null, false, false);
        avgFPS = NewVar("", "avgfps", "average fps since last reset", "", 0f, null, false, false);
    }

    static void reg_AUDIO()
    {
        masterVolume = NewVar(AudioGroup, "mastervolume", "Master Volume", "Control Master Volume", config.audio.InitMasterVolume, app.soundManager.SetTargetMasterVolume, true, true);
        NewSettingsVar(masterVolume as VariableBase, 0f, 100f);

        musicVolume = NewVar(AudioGroup, "musicvolume", "Music Volume", "Control Music Volume", config.audio.InitMusicVolume, app.soundManager.SetTargetMusicVolume, true, true);
        NewSettingsVar(musicVolume as VariableBase, 0f, 100f);

        ambientVolume = NewVar(AudioGroup, "ambientvolume", "Ambient Volume", "Control Ambient Volume", config.audio.InitAmbientVolume, app.soundManager.SetTargetAmbientVolume, true, true);
        NewSettingsVar(ambientVolume as VariableBase, 0f, 100f);

        fxVolume = NewVar(AudioGroup, "fxvolume", "Fx Volume", "Control Fx Volume", config.audio.InitFXVolume, app.soundManager.SetTargetFxVolume, true, true);
        NewSettingsVar(fxVolume as VariableBase, 0f, 100f);

        uiVolume = NewVar(AudioGroup, "uivolume", "Ui Volume", "Control Ui Volume", config.audio.InitUiVolume, app.soundManager.SetTargetUiVolume, true, true);
        NewSettingsVar(uiVolume as VariableBase, 0f, 100f);

        voiceVolume = NewVar(AudioGroup, "voicevolume", "Voice Volume", "Control Voice Volume", config.audio.InitVoiceVolume, app.soundManager.SetTargetVoiceVolume, true, true);
        NewSettingsVar(voiceVolume as VariableBase, 0f, 100f);
    }

    static void reg_CONTROLS()
    {
        sensitivity = NewVar("", "sensitivity", "Mouse Sensitivity", "", config.controls.ViewSensitivity, x => sensitivity.SetSilent(x), true, true);

    }

    static void reg_GFX()
    {
        targetFramerate = NewVar(GraphicsGroup, "targetfps", "Frame Limit", "set max fps for game", config.graphics.LimitFramerate, app.SetTargetFramerate, true, true);
        NewSettingsVar(targetFramerate as VariableBase, 0f, 300f);
        fov = NewVar("", "fov", "Set field of view", "", config.graphics.fov, x => fov.SetSilent(x), false, true);

    }



}

