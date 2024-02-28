using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using MacroRecorded.Data;
using MacroRecorded.Logic;
using MacroRecorded.Services;
using MacroRecorded.Utils;

namespace MacroRecorded.Windows;

public class PluginUi : Window
{
    private readonly ActionWatcher _actionWatcher;
    private readonly Configuration _configuration;
    private readonly WindowService _windowService;
    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    private float _scale;
    private Vector2 _itemTextSize;

    public PluginUi(ActionWatcher actionWatcher, Configuration configuration, WindowService windowService) : base(WindowConstants.MainWindowName, WindowFlags)
    {
        _actionWatcher = actionWatcher;
        _configuration = configuration;
        _windowService = windowService;
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
        _scale = ImGui.GetIO().FontGlobalScale;
        _itemTextSize = ImGui.CalcTextSize(string.Empty);
        var actionsList = _actionWatcher.CraftActions;

        #region Recorder Controls

        ImGui.BeginChild("recording_controls", new Vector2(180, 80) * _scale, true, ImGuiWindowFlags.NoScrollbar);
        ImGui.SetNextItemWidth(ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().ItemSpacing.X);
        ImGui.Text("Recorder Controls");
        ImGui.Separator();
        ImGui.BeginGroup();
        ImGui.PushFont(UiBuilder.IconFont);

        if (_configuration.RecordStarted || !_actionWatcher.CanStartRecording) ImGui.BeginDisabled();
        if (ImGui.Button($"{(char)FontAwesomeIcon.Play}##startRec", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f)))
            _configuration.RecordStarted = true;

        if (_configuration.RecordStarted || !_actionWatcher.CanStartRecording) ImGui.EndDisabled();

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
        
        ImGui.SameLine();
        
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{(char)FontAwesomeIcon.Cog}##config", new Vector2(25 * _scale, _itemTextSize.Y * _scale * 1.5f)))
        {
            var pluginWindow = _windowService.GetWindow(WindowConstants.ConfigWindowName);
            if (pluginWindow is not ConfigurationUi window) return;
            window.IsOpen = true;
        }
        ImGui.PopFont();
        
        ImGui.EndGroup();

        if (_configuration.RecordStarted) 
            ImGui.Text("Recording...");

        if (!_actionWatcher.CanStartRecording) 
            ImGui.Text("Not in Crafting");

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
            ImGui.Text(action.ToMacroText(actionsList[^1] == action, (_configuration.CraftActionWait, _configuration.BuffActionWait)));
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
            ClipboardHelper.TransferToClipboard(actionsList, (_configuration.CraftActionWait, _configuration.BuffActionWait));
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
                    ClipboardHelper.TransferToClipboard(actionsList, slice, (_configuration.CraftActionWait, _configuration.BuffActionWait));
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip($"Export macro {slice + 1}");
            }
        }

        ImGui.EndChild();
    }
}