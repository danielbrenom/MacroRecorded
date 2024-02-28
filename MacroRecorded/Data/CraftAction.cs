using System;
using FFXIVClientStructs.FFXIV.Client.Game;
using MacroRecorded.Utils;

namespace MacroRecorded.Data;

public class CraftAction 
{
    public string ActionName { get; }
    public uint ActionId { get; }
    public uint IconId { get; }
    public double Time { get; }
    public ActionType Type { get; set; }

    public CraftAction(string name, uint actionId, uint iconId, double time, ActionType type)
    {
        ActionName = name;
        ActionId = actionId;
        IconId = iconId;
        Time = time;
        Type = type;
    }
}