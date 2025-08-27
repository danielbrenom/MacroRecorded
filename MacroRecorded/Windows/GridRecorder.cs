using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using MacroRecorded.Data;
using MacroRecorded.Logic;
using MacroRecorded.Utils;

namespace MacroRecorded.Windows;

public class GridRecorder : Window
{
    private readonly ActionWatcher _actionWatcher;
    private readonly Configuration _configuration;

    public GridRecorder(ActionWatcher actionWatcher, Configuration configuration) : base(WindowConstants.MainWindowName)
    {
        _actionWatcher = actionWatcher;
        _configuration = configuration;
    }

    public override void Draw()
    {
        return;
    }

    private void DrawActionIcons(IEnumerable<CraftAction> actionsList)
    {
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
                //TODO: redo this if needed
                //Use the new IDalamudTextureWrapper, no need for cache anymore
                //_drawHelper.DrawIcon(action.ActionId, position, regularSize, 1, drawList);
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
        var pos = ImGui.GetWindowPos();
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