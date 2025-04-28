using System;
using System.Collections.Generic;
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
        //_allNamePlates.Clear();
        foreach (INamePlateUpdateHandler handler in handlers)
        {
            // Only track player characters and enemy battle NPCs
            if (handler.NamePlateKind == NamePlateKind.PlayerCharacter || handler.NamePlateKind == NamePlateKind.BattleNpcEnemy)
            {
                NamePlateEntry entry = new NamePlateEntry
                {
                    GameObjectId = handler.GameObjectId,
                    Name = handler.Name.ToString(),
                    NameIconId = handler.NameIconId,
                    MarkerIconId = handler.MarkerIconId,
                    IsBoss = handler.IsBossFromNamePlateIconId(),
                    Kind = handler.NamePlateKind,
                };
                _allNamePlates.Add(entry);
            }
        }
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
    public IBattleChara? BattleChara { get; }
}
