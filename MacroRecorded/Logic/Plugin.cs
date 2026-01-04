using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using MacroRecorded.Services;
using MacroRecorded.Utils;
using MacroRecorded.Windows;

namespace MacroRecorded.Logic;

public class Plugin : IDalamudPlugin
{
    public string Name => PluginConstants.CommandName;

    private IDalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private readonly PluginDependencyContainer _pluginDependencyContainer;
    private readonly WindowService _windowService;

    public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commandManager, IDataManager dataManager,
        IGameGui gameGui, IFramework framework, IPlayerState playerState,
        ICondition condition, IGameInteropProvider interopProvider, IPluginLog pluginLog)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        var configuration = (Configuration)PluginInterface.GetPluginConfig() ?? new Configuration();
        configuration.Initialize(PluginInterface);
        _windowService = new WindowService(new WindowSystem(WindowConstants.WindowSystemNamespace));
        _pluginDependencyContainer = new PluginDependencyContainer().Register(pluginInterface)
                                                                    .Register(_windowService)
                                                                    .Register(configuration)
                                                                    .Register(dataManager)
                                                                    .Register(gameGui)
                                                                    .Register(framework)
                                                                    .Register(playerState)
                                                                    .Register(condition)
                                                                    .Register(pluginInterface.UiBuilder)
                                                                    .Register(interopProvider)
                                                                    .Register(pluginLog)
                                                                    .Register<ConfigurationUi>()
                                                                    .Register<PluginUi>();
        PluginModule.Register(_pluginDependencyContainer);
        _pluginDependencyContainer.Resolve();

        _windowService.RegisterWindow(_pluginDependencyContainer.Retrieve<PluginUi>(), WindowConstants.MainWindowName);
        _windowService.RegisterWindow(_pluginDependencyContainer.Retrieve<ConfigurationUi>(), WindowConstants.ConfigWindowName);

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
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
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

    private void DrawConfigUi()
    {
        var pluginWindow = _windowService.GetWindow(WindowConstants.ConfigWindowName);
        if (pluginWindow is not ConfigurationUi window) return;
        window.IsOpen = true;
    }

    public void Dispose()
    {
        PluginInterface?.SavePluginConfig(_pluginDependencyContainer.Retrieve<Configuration>());
        _pluginDependencyContainer.Dispose();
        CommandManager.RemoveHandler(PluginConstants.CommandSlash);
        CommandManager.RemoveHandler(PluginConstants.ShortCommandSlash);
        _windowService.Unregister();
        GC.SuppressFinalize(this);
    }
}