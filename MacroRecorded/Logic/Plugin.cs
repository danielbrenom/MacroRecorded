using System;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using MacroRecorded.Services;
using MacroRecorded.Utils;
using MacroRecorded.Windows;

namespace MacroRecorded.Logic;

public class Plugin : IDalamudPlugin
{
    public string Name => PluginConstants.CommandName;

    private DalamudPluginInterface PluginInterface { get; init; }
    private CommandManager CommandManager { get; init; }
    private readonly PluginServiceFactory _pluginServiceFactory;
    private readonly WindowService _windowService;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] DataManager dataManager,
        [RequiredVersion("1.0")] GameGui gameGui,
        [RequiredVersion("1.0")] Framework framework,
        [RequiredVersion("1.0")] SigScanner sigScanner,
        [RequiredVersion("1.0")] ClientState clientState,
        [RequiredVersion("1.0")] Condition condition)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        var configuration = (Configuration)PluginInterface.GetPluginConfig() ?? new Configuration();
        configuration.Initialize(pluginInterface);
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
                                                          .RegisterService(pluginInterface.UiBuilder);
        _pluginServiceFactory.RegisterService(_pluginServiceFactory);
        PluginModule.Register(_pluginServiceFactory);

        var mainWindow = new PluginUi(_pluginServiceFactory.Create<ActionWatcher>(),
                                      _pluginServiceFactory.Create<Configuration>(),
                                      _pluginServiceFactory.Create<DrawHelper>());
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
        // PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
    }

    private void OnCommand(string command, string args)
    {
        var pluginWindow = _windowService.GetWindow(WindowConstants.MainWindowName);
        if (pluginWindow is not PluginUi window) return;
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