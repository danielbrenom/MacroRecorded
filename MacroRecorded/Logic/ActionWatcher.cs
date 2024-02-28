using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel;
using MacroRecorded.Data;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;
using LuminaCraftAction = Lumina.Excel.GeneratedSheets.CraftAction;

namespace MacroRecorded.Logic;

public class ActionWatcher : IDisposable
{
    private readonly IFramework _framework;

    // private readonly SigScanner _sigScanner;
    private readonly IClientState _clientState;
    private readonly ICondition _condition;
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

    public bool CanStartRecording { get; set; }
    public IReadOnlyList<CraftAction> CraftActions => _craftActions.AsReadOnly();

    public ActionWatcher(IDataManager dataManager, IFramework framework, IGameInteropProvider interopProvider, ISigScanner sigScanner,
        IClientState clientState, Configuration configuration, ICondition condition, IPluginLog pluginLog)
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
                _onUseActionHook = interopProvider.HookFromAddress<OnUseActionDelegate>(sigScanner.ScanText(ActionManager.Addresses.UseAction.String), OnUseAction);
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
        if (IsCrafting && !_condition[ConditionFlag.Crafting])
        {
            //Crafting ended
            IsCrafting = false;
            _onUseActionHook.Disable();
        }

        if (_condition[ConditionFlag.Crafting] && _configuration.RecordStarted)
        {
            //Crafting started
            IsCrafting = true;
            _onUseActionHook.Enable();
        }
    }

    public void ResetRecording()
    {
        _craftActions = new List<CraftAction>();
    }

    private unsafe void OnUseAction(ActionManager* manager, ActionType actionType, uint actionId, long targetId, uint a4, uint a5, uint a6, void* a7)
    {
        _onUseActionHook?.Original(manager, actionType, actionId, targetId, a4, a5, a6, a7);
        var player = _clientState.LocalPlayer;
        if (player is null || !IsCrafting || !_configuration.RecordStarted)
            return;
        AddSpellAction(actionId, actionType);
        AddCraftAction(actionId, actionType);
    }

    private void AddSpellAction(uint actionId, ActionType actionType)
    {
        var action = _actionSheet?.GetRow(actionId);
        if (action == null) return;

        if (_craftActions.Count >= MaxActionCount)
        {
            _craftActions.RemoveAt(0);
        }

        var now = ImGui.GetTime();
        var item = new CraftAction(action.Name, actionId, action.Icon, now, actionType);
        _craftActions.Add(item);
    }

    private void AddCraftAction(uint actionId, ActionType actionType)
    {
        var action = _craftSheet?.GetRow(actionId);
        if (action == null) return;

        if (_craftActions.Count >= MaxActionCount)
        {
            _craftActions.RemoveAt(0);
        }

        var now = ImGui.GetTime();
        var item = new CraftAction(action.Name, actionId, action.Icon, now, actionType);
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