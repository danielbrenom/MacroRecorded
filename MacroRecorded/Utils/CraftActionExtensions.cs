using FFXIVClientStructs.FFXIV.Client.Game;
using MacroRecorded.Data;

namespace MacroRecorded.Utils;

public static class CraftActionExtensions
{
    public static string ToMacroText(this CraftAction action, bool isLast, (int, int) configuredWait)
    {
        var format = action.ActionName.Contains(' ') ? PluginConstants.MultiWordMacroFormat : PluginConstants.MacroFormat;
        var waitAmount = action.Type == ActionType.CraftAction ? configuredWait.Item1 : configuredWait.Item2;
        var wait = isLast ? string.Empty : $" <wait.{waitAmount}>";
        return string.Format(format, action.ActionName, wait);
    }
}