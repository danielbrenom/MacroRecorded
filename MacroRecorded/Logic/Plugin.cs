using System;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using MacroRecorded.Services;
using MacroRecorded.Utils;
using MacroRecorded.Windows;

namespace MacroRecorded.Logic;

public class Plugin : IDalamudPlugin
{
    public string Name => PluginConstants.CommandName;

    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private readonly PluginServiceFactory _pluginServiceFactory;
    private readonly WindowService _windowService;

    public Plugin(DalamudPluginInterface pluginInterface, ICommandManager commandManager, IDataManager dataManager,
        IGameGui gameGui, IFramework framework, ISigScanner sigScanner, IClientState clientState,
        ICondition condition, IGameInteropProvider interopProvider, IPluginLog pluginLog)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        var configuration = (Configuration)PluginInterface.GetPluginConfig() ?? new Configuration();
        _windowService = new WindowService(new(WindowConstants.WindowSystemNamespace));
        _pluginServiceFactory = new PluginServiceFactory().RegisterService(pluginInterface)
                                                          .RegisterService(_windowService)
                                                          .RegisterService(configuration)
                                                          .RegisterService(dataManager)
                                                          .RegisterService(gameGui)
                                                          .RegisterService(framework)
                                                          .RegisterService(sigScanner)
                                                          .RegisterService(clientState)
                                                          .RegisterService(condition)
                                                          .RegisterService(pluginInterface.UiBuilder)
                                                          .RegisterService(interopProvider)
                                                          .RegisterService(pluginLog);
        _pluginServiceFactory.RegisterService(_pluginServiceFactory);
        PluginModule.Register(_pluginServiceFactory);

        var mainWindow = new PluginUi(_pluginServiceFactory.Create<ActionWatcher>(),
                                      _pluginServiceFactory.Create<Configuration>());
        _windowService.RegisterWindow(mainWindow, WindowConstants.MainWindowName);

        CommandManager.AddHandler(PluginConstants.CommandSlash, new CommandInfo(OnCommand)
        {
            HelpMessage = PluginConstants.CommandHelperText
        });
        CommandManager.AddHandler(PluginConstants.ShortCommandSlash, new CommandInfo(OnCommand)
        {
            HelpMessage = PluginConstants.CommandHelperText
        });

        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenMainUi += _windowService.GetWindow(WindowConstants.MainWindowName).Toggle;
        // PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
    }

    private void OnCommand(string command, string args)
    {
        var pluginWindow = _windowService.GetWindow(WindowConstants.MainWindowName);
        if (pluginWindow is not PluginUi) return;
        pluginWindow.IsOpen = true;
    }

    private void DrawUi()
    {
        _windowService.Draw();
    }


    public void Dispose()
    {
        PluginInterface?.SavePluginConfig(_pluginServiceFactory.Create<Configuration>());
        _pluginServiceFactory.Dispose();
        CommandManager.RemoveHandler(PluginConstants.CommandSlash);
        CommandManager.RemoveHandler(PluginConstants.ShortCommandSlash);
        _windowService.Unregister();
        GC.SuppressFinalize(this);
    }
}