﻿using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace MacroRecorded.Logic;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool RecordStarted { get; set; }

    // public bool ShowRecordingGrid { get; set; }
    // public bool IntegrateMacroChain { get; set; }
    public int ActionIconSize = 40;
    public int CraftingActionsLimit = 5;

    public int GridLineWidth = 1;
    public Vector4 GridLineColor = new(0.3f, 0.3f, 0.3f, 1f);
    public int CraftActionWait { get; set; } = 3;
    public int BuffActionWait { get; set; } = 2;
    
    [NonSerialized]
    private IDalamudPluginInterface _pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
}