using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using MacroRecorded.Logic;
using MacroRecorded.Utils;

namespace MacroRecorded.Windows;

public class ConfigurationUi : Window
{
    private readonly Configuration _configuration;
    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    private float _scale;

    private int _actionDuration;
    private int _buffDuration;

    public ConfigurationUi(Configuration configuration) : base(WindowConstants.ConfigWindowName, WindowFlags)
    {
        _configuration = configuration;
        _scale = ImGui.GetIO().FontGlobalScale;
        var sizeAnchor = new Vector2(350, 300);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = sizeAnchor * _scale,
            MaximumSize = sizeAnchor * _scale * 1.5f
        };
        SizeCondition = ImGuiCond.FirstUseEver;
        _actionDuration = configuration.CraftActionWait;
        _buffDuration = configuration.BuffActionWait;
    }

    public override void Draw()
    {
        _scale = ImGui.GetIO().FontGlobalScale;

        ImGui.BeginChild("configurations", new Vector2(0, -1f) * _scale);

        ImGui.Text("Action wait amount:");
        ImGui.SameLine();
        ImGui.SliderInt("##actionWait", ref _actionDuration, 1, 3);
        ImGui.Text("Buff wait amount:");
        ImGui.SameLine();
        ImGui.SliderInt("##buffWait", ref _buffDuration, 1, 3);

        const string saveText = "Save and close";
        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - (ImGui.CalcTextSize(saveText).X * _scale + 10f));
        ImGui.SetCursorPosY(ImGui.GetWindowContentRegionMax().Y - (ImGui.GetFontSize() * _scale + 10f));
        if (ImGui.Button(saveText))
            IsOpen = false;

        ImGui.EndChild();
    }

    public override void OnClose()
    {
        _configuration.CraftActionWait = _actionDuration;
        _configuration.BuffActionWait = _buffDuration;
        _configuration.Save();
        base.OnClose();
    }
}