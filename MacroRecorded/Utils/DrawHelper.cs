//<copyright>
//<author>Tischel</author>
//<url>https://github.com/Tischel/ActionTimeline/blob/master/ActionTimeline/Helpers/DrawHelper.cs</url>
//</copyright>

using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;

namespace MacroRecorded.Utils;

public class DrawHelper
{
    private readonly TexturesCache _texturesCache;

    public DrawHelper(TexturesCache texturesCache)
    {
        _texturesCache = texturesCache;
    }

    public void DrawIcon(uint iconId, Vector2 position, Vector2 size, float alpha, ImDrawListPtr drawList)
    {
        var texture = _texturesCache.GetTextureFromIconId(iconId);
        if (texture == null) return;

        var color = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, alpha));
        drawList.AddImage(texture.ImGuiHandle, position, position + size, Vector2.Zero, Vector2.One, color);
    }

    public void SetTooltip(string message)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(message);
        }
    }
}