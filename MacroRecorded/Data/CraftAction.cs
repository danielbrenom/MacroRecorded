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
}