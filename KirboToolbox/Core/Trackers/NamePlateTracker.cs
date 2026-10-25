using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.NamePlate;

using ECommons.DalamudServices;

using KirboToolbox.Core.Extensions;
using KirboToolbox.Helpers;

namespace KirboToolbox.Core.Trackers;

/// <summary>
/// Updates and stores a list of active NamePlate entries.
/// </summary>
internal static class NamePlateTracker
{
    public static IReadOnlyList<NamePlateEntry> AllNamePlates => Entries;
    private static readonly List<NamePlateEntry> Entries = new();

    public static void Enable()
    {
        Svc.NamePlates.OnNamePlateUpdate += OnNamePlateUpdate;
        Svc.Log.Debug($"NamePlateTracker: OnNamePlateUpdate is now enabled!");
    }

    public static void Disable()
    {
        ClearList();
        Svc.NamePlates.OnNamePlateUpdate -= OnNamePlateUpdate;
        Svc.Log.Debug($"NamePlateTracker: OnNamePlateUpdate is now disabled!");
    }

    /// <summary>
    /// Checks if the nameplate associated with this handler has a NameIconId of 61710, 61711, or 61712.
    /// Only returns true if the associated object is a BattleChara.
    /// </summary>
    /// <param name="handler">The nameplate update handler.</param>
    /// <returns>True if the NameIconId is one of the specified IDs, otherwise false.</returns>
    public static bool IsBossFromNamePlateIconId(this INamePlateUpdateHandler handler)
    {
        if (handler.BattleChara == null)
        {
            return false;
        }

        return handler.NameIconId is 61710 or 61711 or 61712;
    }

    internal static void OnNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        NamePlateTracker.Update(handlers);
        // Build a set of valid GameObjectIds from current handlers
        HashSet<ulong> currentGameObjectIds = new();
        foreach (INamePlateUpdateHandler handler in handlers)
        {
            if (handler.NamePlateKind == NamePlateKind.PlayerCharacter || handler.NamePlateKind == NamePlateKind.BattleNpcEnemy)
            {
                currentGameObjectIds.Add(handler.GameObjectId);

                var existing = Entries.FirstOrDefault(p => p.GameObjectId == handler.GameObjectId);
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
                    Entries.Add(new NamePlateEntry
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
        Entries.RemoveAll(entry => !entry.IsTargetable || ObjectHelper.DistanceToPlayer(entry.BattleChara) >= 49);

    }

    public static void ClearList()
    {
        Entries.Clear();
    }

    public static IReadOnlyList<INamePlateUpdateHandler> CurrentNameplates { get; private set; } = new List<INamePlateUpdateHandler>();

    public static void Update(IReadOnlyList<INamePlateUpdateHandler> updated)
    {
        CurrentNameplates = updated;
    }
    public static INamePlateUpdateHandler? GetCurrentNameplate(this IBattleChara battleChara)
    {
        if (battleChara == null)
        {
            return null;
        }

        return NamePlateTracker.CurrentNameplates
            .FirstOrDefault(n => n.GameObjectId == battleChara.GameObjectId);
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

    /// <summary>
    /// Gets the <see cref="IBattleChara"/> associated with this nameplate, if possible. Returns null if the nameplate
    /// has an associated <see cref="IGameObject"/>, but that object cannot be assigned to <see cref="IBattleChara"/>.
    /// </summary>
    public IBattleChara? BattleChara { get; set; }

    public bool IsTargetable =>
        BattleChara is { } chara
        && chara.Address != nint.Zero
        && chara.IsValid()
        && chara.IsTargetable;

    public static NamePlateEntry? CurrentTargetNameplate
    {
        get
        {
            var targetId = Svc.Targets.Target?.GameObjectId ?? 0;
            return NamePlateTracker.AllNamePlates.FirstOrDefault(n => n.GameObjectId == targetId);
        }
    }
}
