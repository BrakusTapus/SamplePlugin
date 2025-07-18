using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using SamplePlugin.DalamudServices;
using SamplePlugin.Helpers;

namespace SamplePlugin.Updaters;

/// <summary>
/// Updates and stores a list of active NamePlate entries.
/// </summary>
internal static class NamePlateUpdater
{
    public static IReadOnlyList<NamePlateEntry> AllNamePlates => _allNamePlates;
    private static readonly List<NamePlateEntry> _allNamePlates = new();

    public static void Enable()
    {
        Service.NamePlateGui.OnNamePlateUpdate += OnNamePlateUpdate;
    }

    public static void Dispose()
    {
        Service.NamePlateGui.OnNamePlateUpdate -= OnNamePlateUpdate;
    }

    internal static void OnNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        // Build a set of valid GameObjectIds from current handlers
        HashSet<ulong> currentGameObjectIds = new();
        foreach (INamePlateUpdateHandler handler in handlers)
        {
            if (handler.NamePlateKind == NamePlateKind.PlayerCharacter || handler.NamePlateKind == NamePlateKind.BattleNpcEnemy)
            {
                currentGameObjectIds.Add(handler.GameObjectId);

                var existing = _allNamePlates.FirstOrDefault(p => p.GameObjectId == handler.GameObjectId);
                if (existing != null)
                {
                    // Update existing entry
                    existing.GameObjectId = handler.GameObjectId;
                    existing.Name = handler.Name.ToString();
                    existing.NameIconId = handler.NameIconId;
                    existing.MarkerIconId = handler.MarkerIconId;
                    existing.IsBoss = handler.IsBossFromNamePlateIconId();
                    existing.Kind = handler.NamePlateKind;
                    existing.BattleChara = handler.BattleChara;
                }
                else
                {
                    // Add new entry
                    _allNamePlates.Add(new NamePlateEntry
                    {
                        GameObjectId = handler.GameObjectId,
                        Name = handler.Name.ToString(),
                        NameIconId = handler.NameIconId,
                        MarkerIconId = handler.MarkerIconId,
                        IsBoss = handler.IsBossFromNamePlateIconId(),
                        Kind = handler.NamePlateKind,
                        BattleChara = handler.BattleChara
                    });
                }
            }
        }

        // Remove entries that are no longer valid
        _allNamePlates.RemoveAll(entry => entry.BattleChara == null || !entry.BattleChara.IsTargetable || ObjectHelper.DistanceToPlayer(entry.BattleChara) >= 49);

    }

    public static void ClearList()
    {
        _allNamePlates.Clear();
    }
}

/// <summary>
/// A simple container for NamePlate information.
/// </summary>
internal class NamePlateEntry
{
    /// <summary>The unique ID of the object the nameplate belongs to.</summary>
    public ulong GameObjectId { get; set; }

    /// <summary>The displayed name on the nameplate.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The name icon ID shown on the nameplate.</summary>
    public int NameIconId { get; set; }

    /// <summary>The marker icon ID shown on the nameplate (e.g., icons above nameplates).</summary>
    public int MarkerIconId { get; set; }

    /// <summary>Whether this nameplate belongs to a boss-type enemy.</summary>
    public bool IsBoss { get; set; }

    /// <summary>The kind of the nameplate (player, NPC, etc.).</summary>
    public NamePlateKind Kind { get; set; }

    /// <summary>Distance from the camera to the nameplate, in yalms.</summary>
    public float DistanceYalms { get; set; }

    /// <summary>
    /// Gets the <see cref="IGameObject"/> associated with this nameplate, if possible. Performs an object table scan
    /// and caches the result if successful.
    /// </summary>
    public IGameObject? GameObject { get; }

    /// <summary>
    /// Gets a read-only view of the nameplate info object data for a nameplate. Modifications to
    /// <see cref="NamePlateUpdateHandler"/> fields do not affect fields in the returned view.
    /// </summary>
    public INamePlateInfoView InfoView { get; }

    /// <summary>
    /// Gets the <see cref="IBattleChara"/> associated with this nameplate, if possible. Returns null if the nameplate
    /// has an associated <see cref="IGameObject"/>, but that object cannot be assigned to <see cref="IBattleChara"/>.
    /// </summary>
    public IBattleChara? BattleChara { get; set; }

    public bool IsTargetable => BattleChara.IsTargetable;
}
