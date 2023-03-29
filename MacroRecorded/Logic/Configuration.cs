using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace MacroRecorded.Logic;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 114;
    
    public bool RecordCrafting { get; set; }
    
    public bool ShowRecordingGrid { get; set; }
    public int ActionIconSize = 40;
    public int CraftingActionsLimit = 5;
    
    public int GridLineWidth = 1;
    public Vector4 GridLineColor = new(0.3f, 0.3f, 0.3f, 1f);

    [NonSerialized] private DalamudPluginInterface _pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
}