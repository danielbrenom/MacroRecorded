using System.Collections.Generic;
using System.Linq;
using System.Text;
using MacroRecorded.Data;
using ImGuiNET;

namespace MacroRecorded.Utils;

public static class ClipboardHelper
{
    //The actions list is limited to 50 actions, macros usually wouldn't have more than 30 steps
    private static readonly Dictionary<int, int> SliceRanges = new()
    {
        { 0, 14 },
        { 15, 29 },
        { 30, 44 },
        { 45, 50 }
    };

    public static void TransferToClipboard(IReadOnlyList<CraftAction> actions, (int,int) configuredWait)
    {
        if (!actions.Any()) return;
        var builder = new StringBuilder();
        foreach (var action in actions)
        {
            builder.AppendLine(action.ToMacroText(actions[^1] == action, configuredWait));
        }

        ImGui.SetClipboardText(builder.ToString());
    }

    public static void TransferToClipboard(IReadOnlyList<CraftAction> actions, int slice, (int,int) configuredWait)
    {
        var range = SliceRanges.ElementAt(slice);
        var sliceActions = new List<CraftAction>();
        for (var i = range.Key; i <= range.Value; i++)
        {
            if (actions.ElementAtOrDefault(i) is not { } action) continue;
            sliceActions.Add(action);
        }

        TransferToClipboard(sliceActions, configuredWait);
    }
}