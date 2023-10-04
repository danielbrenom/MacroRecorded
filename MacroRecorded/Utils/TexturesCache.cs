//<copyright>
//<author>Tischel</author>
//<url>https://github.com/Tischel/ActionTimeline/blob/master/ActionTimeline/Helpers/TexturesCache.cs</url>
//</copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Internal;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;

namespace MacroRecorded.Utils;

public class TexturesCache : IDisposable
{
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly TextureLoader _textureLoader;
    private readonly IDataManager _dataManager;
    private Dictionary<uint, IDalamudTextureWrap> _cache = new();

    public TexturesCache(DalamudPluginInterface pluginInterface, IDataManager dataManager, TextureLoader textureLoader)
    {
        _pluginInterface = pluginInterface;
        _dataManager = dataManager;
        _textureLoader = textureLoader;
    }

    public IDalamudTextureWrap GetTexture<T>(uint rowId, bool highQuality = false, uint stackCount = 0) where T : ExcelRow
    {
        var sheet = _dataManager.GetExcelSheet<T>();

        return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), highQuality, stackCount);
    }

    public IDalamudTextureWrap GetTexture<T>(dynamic row, bool highQuality = false, uint stackCount = 0) where T : ExcelRow
    {
        if (row == null)
        {
            return null;
        }

        var iconId = row.Icon;
        return GetTextureFromIconId(iconId, highQuality, stackCount);
    }

    public IDalamudTextureWrap GetTextureFromIconId(uint iconId, bool highQuality = false, uint stackCount = 0)
    {
        if (_cache.TryGetValue(iconId + stackCount, out var texture))
        {
            return texture;
        }

        var newTexture = LoadTexture(iconId + stackCount, highQuality);
        if (newTexture == null)
        {
            return null;
        }

        _cache.Add(iconId + stackCount, newTexture);

        return newTexture;
    }

    private IDalamudTextureWrap LoadTexture(uint id, bool highQuality)
    {
        var hqText = highQuality ? "hq/" : "";
        var path = $"ui/icon/{id / 1000 * 1000:000000}/{hqText}{id:000000}_hr1.tex";

        return _textureLoader.LoadTexture(path, false);
    }

    private void RemoveTexture<T>(uint rowId) where T : ExcelRow
    {
        var sheet = _dataManager.GetExcelSheet<T>();

        if (sheet == null)
        {
            return;
        }

        RemoveTexture<T>(sheet.GetRow(rowId));
    }

    public void RemoveTexture<T>(dynamic row) where T : ExcelRow
    {
        if (row == null || row?.Icon == null)
        {
            return;
        }

        var iconId = row!.Icon;
        RemoveTexture(iconId);
    }

    public void RemoveTexture(uint iconId)
    {
        _cache.Remove(iconId);
    }

    public void Clear() => _cache.Clear();

    ~TexturesCache()
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
        if (!disposing)
        {
            return;
        }

        foreach (var tex in _cache.Keys.Select(key => _cache[key]))
        {
            tex?.Dispose();
        }

        _cache.Clear();
    }
}