using MacroRecorded.Logic;

namespace MacroRecorded.Services;

public static class PluginModule
{
    public static void Register(PluginDependencyContainer container)
    {
        container.Register<ActionWatcher>();
    }
}