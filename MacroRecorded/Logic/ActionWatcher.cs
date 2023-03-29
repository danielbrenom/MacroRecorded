using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel;
using MacroRecorded.Data;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;
using LuminaCraftAction = Lumina.Excel.GeneratedSheets.CraftAction;

namespace MacroRecorded.Logic;

public class ActionWatcher : IDisposable
{
    private readonly Framework _framework;

    // private readonly SigScanner _sigScanner;
    private readonly ClientState _clientState;
    private readonly Condition _condition;
    private readonly Configuration _configuration;

    // <ActionManager*, global::FFXIVClientStructs.FFXIV.Client.Game.ActionType, uint, long, uint, uint, uint, void*, bool>
    // By using this, the HookDelegate need the ActionManager
    // public partial bool UseAction(ActionType actionType, uint actionID, long targetID = 0xE000_0000, uint a4 = 0, uint a5 = 0, uint a6 = 0, void* a7 = null);
    private unsafe delegate void OnUseActionDelegate(ActionManager* manager, ActionType actionType, uint actionId, long targetId = 0xE000_0000, uint a4 = 0, uint a5 = 0, uint a6 = 0, void* a7 = null);

    private readonly Hook<OnUseActionDelegate> _onUseActionHook;

    private bool IsCrafting;
    private readonly ExcelSheet<LuminaAction> _actionSheet;
    private readonly ExcelSheet<LuminaCraftAction> _craftSheet;
    private const int MaxActionCount = 50;
    private List<CraftAction> _craftActions = new(MaxActionCount);
    public IReadOnlyList<CraftAction> CraftActions => _craftActions.AsReadOnly();

    public ActionWatcher(DataManager dataManager, Framework framework, SigScanner sigScanner, ClientState clientState, Configuration configuration, Condition condition)
    {
        _actionSheet = dataManager.GetExcelSheet<LuminaAction>();
        _craftSheet = dataManager.GetExcelSheet<LuminaCraftAction>();
        _framework = framework;
        _clientState = clientState;
        _condition = condition;
        _configuration = configuration;
        try
        {
            unsafe
            {
                _onUseActionHook = Hook<OnUseActionDelegate>.FromAddress(sigScanner.ScanText(ActionManager.Addresses.UseAction.String), OnUseAction);
                _onUseActionHook.Enable();
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error initializing: {e.Message}");
        }

        _framework.Update += Update;
    }

    private void Update(Framework framework)
    {
        IsCrafting = _condition[ConditionFlag.Crafting];
    }

    public void ResetRecording()
    {
        _craftActions = new List<CraftAction>();
    }

    private unsafe void OnUseAction(ActionManager* manager, ActionType actionType, uint actionId, long targetId, uint a4, uint a5, uint a6, void* a7)
    {
        _onUseActionHook?.Original(manager, actionType, actionId, targetId, a4, a5, a6, a7);
        var player = _clientState.LocalPlayer;
        if (player == null || !IsCrafting || !_configuration.RecordStarted)
            return;
        AddSpellAction(actionId);
        AddCraftAction(actionId);
    }

    private void AddSpellAction(uint actionId)
    {
        var action = _actionSheet?.GetRow(actionId);
        if (action == null) return;

        if (_craftActions.Count >= MaxActionCount)
        {
            _craftActions.RemoveAt(0);
        }

        var now = ImGui.GetTime();

        var item = new CraftAction(action.Name, actionId, action.Icon, now);
        _craftActions.Add(item);
    }

    private void AddCraftAction(uint actionId)
    {
        var action = _craftSheet?.GetRow(actionId);
        if (action == null) return;

        if (_craftActions.Count >= MaxActionCount)
        {
            _craftActions.RemoveAt(0);
        }

        var now = ImGui.GetTime();

        var item = new CraftAction(action.Name, actionId, action.Icon, now);
        _craftActions.Add(item);
    }

    ~ActionWatcher()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        _framework.Update -= Update;
        _onUseActionHook?.Disable();
        _onUseActionHook?.Dispose();
    }
}