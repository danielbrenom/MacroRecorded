using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using MacroRecorded.Data;
using MacroRecorded.Logic;
using MacroRecorded.Utils;

namespace MacroRecorded.Windows;

public class PluginUi : Window
{
    private readonly ActionWatcher _actionWatcher;
    private readonly Configuration _configuration;
    private ImGuiWindowFlags _flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    private readonly float _scale;
    private Vector2 _itemTextSize;

    public PluginUi(ActionWatcher actionWatcher, Configuration configuration) : base(WindowConstants.MainWindowName)
    {
        Flags = _flags;
        _actionWatcher = actionWatcher;
        _configuration = configuration;

        _scale = ImGui.GetIO().FontGlobalScale;
        var sizeAnchor = new Vector2(350, 300);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = sizeAnchor * _scale,
            MaximumSize = sizeAnchor * _scale * 1.5f
        };
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        _itemTextSize = ImGui.CalcTextSize(string.Empty);
        var actionsList = _actionWatcher.CraftActions;

        #region Recorder Controls

        ImGui.BeginChild("recording_controls", new Vector2(150, 80) * _scale, true, ImGuiWindowFlags.NoScrollbar);
        ImGui.SetNextItemWidth(ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().ItemSpacing.X);
        ImGui.Text("Recorder Controls");
        ImGui.Separator();
        ImGui.BeginGroup();
        ImGui.PushFont(UiBuilder.IconFont);

        if (_configuration.RecordStarted) ImGui.BeginDisabled();
        if (ImGui.Button($"{(char)FontAwesomeIcon.Play}##startRec", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f)))
            _configuration.RecordStarted = true;

        if (_configuration.RecordStarted) ImGui.EndDisabled();

        ImGui.SameLine();

        if (!_configuration.RecordStarted) ImGui.BeginDisabled();
        if (ImGui.Button($"{(char)FontAwesomeIcon.Stop}##stopRec", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f)))
            _configuration.RecordStarted = false;
        if (!_configuration.RecordStarted) ImGui.EndDisabled();

        ImGui.SameLine();

        if (!actionsList.Any() || _configuration.RecordStarted) ImGui.BeginDisabled();
        if (ImGui.Button($"{(char)FontAwesomeIcon.Trash}##clearRec", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f)))
            _actionWatcher.ResetRecording();
        ImGui.PopFont();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Clear recording");

        if (!actionsList.Any() || _configuration.RecordStarted) ImGui.EndDisabled();
        
        ImGui.SameLine();
        
        ImGui.PushFont(UiBuilder.IconFont);
        if(ImGui.Button($"{(char)FontAwesomeIcon.Info}##info", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f))){}
        ImGui.PopFont();
        if(ImGui.IsItemHovered())
            ImGui.SetTooltip("Start crafting and hit Play to record your actions. \nPress Stop to stop and be able to clear");
        
        ImGui.EndGroup();

        if (_configuration.RecordStarted)
        {
            ImGui.Text("Recording...");
        }

        ImGui.EndChild();

        #endregion

        #region Macro Controls

        GenerateMacroControls(actionsList);

        #endregion

        ImGui.BeginChild("macro_recorded_area", new Vector2(0, 0) * _scale, true, ImGuiWindowFlags.NoScrollbar);
        ImGui.Text("Recorded Macro");
        ImGui.Separator();
        ImGui.BeginChild("macro_recoding_text", new Vector2(0, 0), false);
        foreach (var action in actionsList)
        {
            ImGui.Text(action.ToMacroAction(actionsList[^1] == action));
        }

        ImGui.EndChild();
        ImGui.EndChild();
    }

    private void GenerateMacroControls(IReadOnlyList<CraftAction> actionsList)
    {
        var macroSlices = actionsList.Count / 16;
        ImGui.SameLine();
        ImGui.BeginChild("macro_controls", new Vector2(0, 80) * _scale, true, ImGuiWindowFlags.NoScrollbar);
        ImGui.Text("Export Controls");
        ImGui.Separator();
        ImGui.PushFont(UiBuilder.IconFont);
        if (macroSlices > 0 || !actionsList.Any()) ImGui.BeginDisabled();
        if (ImGui.Button($"{(char)FontAwesomeIcon.FileExport}##exportRec", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f)))
        {
            ClipboardHelper.TransferToClipboard(actionsList);
        }
        ImGui.PopFont();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip($"Export macro");

        if (macroSlices > 0 || !actionsList.Any()) ImGui.EndDisabled();
        if (macroSlices > 0)
        {
            for (var slice = 0; slice <= macroSlices; slice++)
            {
                ImGui.SameLine();
                if (ImGui.Button($"{slice + 1}##exportRec{slice + 1}", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f)))
                {
                    ClipboardHelper.TransferToClipboard(actionsList, slice);
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip($"Export macro {slice + 1}");
            }
        }

        ImGui.EndChild();
    }
}