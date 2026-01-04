using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using MacroRecorded.Data;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaCraftAction = Lumina.Excel.Sheets.CraftAction;

namespace MacroRecorded.Logic;

public class ActionWatcher : IDisposable
{
    private readonly IFramework _framework;
    private readonly IPlayerState _playerState;
    private readonly ICondition _condition;
    private readonly Configuration _configuration;
    private readonly Hook<ActionManager.Delegates.UseAction> _onUseActionHook;
    private readonly ExcelSheet<LuminaAction> _actionSheet;
    private readonly ExcelSheet<LuminaCraftAction> _craftSheet;
    private bool _isCrafting;
    private const int MaxActionCount = 50;
    private List<CraftAction> _craftActions = new(MaxActionCount);

    public bool CanStartRecording { get; private set; }
    public IReadOnlyList<CraftAction> CraftActions => _craftActions.AsReadOnly();

    public ActionWatcher(IDataManager dataManager, IFramework framework, IGameInteropProvider interopProvider, IPlayerState playerState, Configuration configuration, ICondition condition, IPluginLog pluginLog)
    {
        _actionSheet = dataManager.GetExcelSheet<LuminaAction>();
        _craftSheet = dataManager.GetExcelSheet<LuminaCraftAction>();
        _framework = framework;
        _playerState = playerState;
        _condition = condition;
        _configuration = configuration;
        try
        {
            unsafe
            {
                _onUseActionHook = interopProvider.HookFromAddress<ActionManager.Delegates.UseAction>(ActionManager.MemberFunctionPointers.UseAction, OnUseAction);
            }
        }
        catch (Exception e)
        {
            pluginLog.Error($"Error initializing: {e.Message}");
        }

        _framework.Update += Update;
    }

    private void Update(IFramework framework)
    {
        CanStartRecording = _condition[ConditionFlag.Crafting];
        //The hook should be enabled only when the user is crafting, otherwise it'll interfere with users actions
        if (_isCrafting && !_condition[ConditionFlag.Crafting])
        {
            //Crafting ended
            _isCrafting = false;
            _onUseActionHook.Disable();
        }

        if (_condition[ConditionFlag.Crafting] && _configuration.RecordStarted)
        {
            //Crafting started
            _isCrafting = true;
            _onUseActionHook.Enable();
        }
    }

    public void ResetRecording()
    {
        _craftActions = [];
    }

    private unsafe bool OnUseAction(ActionManager* manager, ActionType actionType, uint actionId, ulong targetId, uint extraParam, ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted)
    {
        var result = _onUseActionHook?.Original(manager, actionType, actionId, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);
        if (_playerState is null || !_isCrafting || !_configuration.RecordStarted)
            return result ?? true;
        AddSpellAction(actionId, actionType);
        AddCraftAction(actionId, actionType);
        return result ?? true;
    }

    private void AddSpellAction(uint actionId, ActionType actionType)
    {
        var action = _actionSheet?.GetRowOrDefault(actionId);
        if (action == null) return;

        if (_craftActions.Count >= MaxActionCount)
        {
            _craftActions.RemoveAt(0);
        }

        var now = ImGui.GetTime();
        var item = new CraftAction(action.Value.Name.ExtractText(), actionId, action.Value.Icon, now, actionType);
        _craftActions.Add(item);
    }

    private void AddCraftAction(uint actionId, ActionType actionType)
    {
        var action = _craftSheet?.GetRowOrDefault(actionId);
        if (action == null) return;

        if (_craftActions.Count >= MaxActionCount)
        {
            _craftActions.RemoveAt(0);
        }

        var now = ImGui.GetTime();
        var item = new CraftAction(action.Value.Name.ExtractText(), actionId, action.Value.Icon, now, actionType);
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