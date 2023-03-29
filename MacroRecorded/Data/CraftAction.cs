using System;
using MacroRecorded.Utils;

namespace MacroRecorded.Data;

public class CraftAction 
{
    public string ActionName { get; }
    public uint ActionId { get; }
    public uint IconId { get; }
    public double Time { get; }

    public CraftAction(string name, uint actionId, uint iconId, double time)
    {
        ActionName = name;
        ActionId = actionId;
        IconId = iconId;
        Time = time;
    }

    public string ToMacroAction(bool isLast)
    {
        var format = ActionName.Contains(' ') ? PluginConstants.MultiWordMacroFormat : PluginConstants.MacroFormat;
        var wait = isLast ? string.Empty : " <wait.3>";
        return string.Format(format, ActionName, wait);
    }
}