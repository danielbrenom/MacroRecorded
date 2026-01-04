using FFXIVClientStructs.FFXIV.Client.Game;

namespace MacroRecorded.Data;

public class CraftAction(string name, uint actionId, uint iconId, double time, ActionType type)
{
    public string ActionName { get; } = name;
    public uint ActionId { get; } = actionId;
    public uint IconId { get; } = iconId;
    public double Time { get; } = time;
    public ActionType Type { get; set; } = type;
}