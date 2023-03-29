using System;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using MacroRecorded.Logic;
using MacroRecorded.Utils;

namespace MacroRecorded.Windows;

public class PluginUi : Window
{
    private readonly ActionWatcher _actionWatcher;
    private readonly Configuration _configuration;
    private readonly DrawHelper _drawHelper;
    private ImGuiWindowFlags _flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    private readonly float _scale;

    public PluginUi(ActionWatcher actionWatcher, Configuration configuration, DrawHelper drawHelper) : base(WindowConstants.MainWindowName)
    {
        Flags = _flags;
        _actionWatcher = actionWatcher;
        _configuration = configuration;
        _drawHelper = drawHelper;

        _scale = ImGui.GetIO().FontGlobalScale;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 600) * _scale,
            MaximumSize = new Vector2(800, 600) * _scale * 1.5f
        };
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        ImGui.Text("Test With OnAction");
        
        // DrawGrid();

        var actionsList = _actionWatcher.CraftActions;
        if (actionsList is null) return;
        
        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetWindowPos();
        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();
        var now = ImGui.GetTime();
        var actionsLimit = _configuration.CraftingActionsLimit;

        var regularSize = new Vector2(_configuration.ActionIconSize);
        
        foreach (var action in actionsList)
        {
            //position
            var posX = GetPositionX(Math.Abs(now - action.Time), actionsLimit, width);
            var posY = height / 2f;
            
            //size
            var position = new Vector2(pos.X + posX - regularSize.X / 2f, pos.Y + posY - regularSize.Y / 2f);
            if (position.X >= -regularSize.X)
            {
                _drawHelper.DrawIcon(action.ActionId, position, regularSize, 1, drawList);
            }
            ImGui.Text($"Action executed: {action.ActionName}, execution time: {action.Time}");
        }
    }
    
    private float GetPositionX(double timeDiff, int maxTime, float width)
    {
        return width - ((float)timeDiff * width / maxTime);
    }

    private void DrawGrid()
    {
        // if (!_configuration.ShowRecordingGrid) { return; }

        var drawList = ImGui.GetWindowDrawList();
        Vector2 pos = ImGui.GetWindowPos();
        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();

        var now = ImGui.GetTime();
        var maxActionCount = _configuration.CraftingActionsLimit;

        var lineColor = ImGui.ColorConvertFloat4ToU32(_configuration.GridLineColor);
        // uint subdivisionLineColor = ImGui.ColorConvertFloat4ToU32(Settings.GridSubdivisionLineColor);

        for (var i = 0; i < maxActionCount; i++)
        {
            var step = width / maxActionCount;
            var x = step * i;

            drawList.AddLine(new Vector2(pos.X + x, pos.Y), new Vector2(pos.X + x, pos.Y + height), lineColor, _configuration.GridLineWidth);
        }
    }
}