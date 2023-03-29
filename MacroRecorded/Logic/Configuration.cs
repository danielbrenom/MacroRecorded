using System.Numerics;
using Dalamud.Configuration;

namespace MacroRecorded.Logic;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool RecordStarted { get; set; }

    public bool ShowRecordingGrid { get; set; }
    public bool IntegrateMacroChain { get; set; }
    public int ActionIconSize = 40;
    public int CraftingActionsLimit = 5;

    public int GridLineWidth = 1;
    public Vector4 GridLineColor = new(0.3f, 0.3f, 0.3f, 1f);
}