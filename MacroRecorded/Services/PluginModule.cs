using MacroRecorded.Logic;
using MacroRecorded.Utils;

namespace MacroRecorded.Services;

public static class PluginModule
{
    public static void Register(PluginServiceFactory container)
    {
        container.RegisterService<ActionWatcher>()
                 .RegisterService<TextureLoader>()
                 .RegisterService<TexturesCache>()
                 .RegisterService<DrawHelper>();
    }
}