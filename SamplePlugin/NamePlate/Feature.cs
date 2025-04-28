using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.NamePlate;
using ECommons.DalamudServices;
using ImGuiNET;
using SamplePlugin.DalamudServices;
using SamplePlugin.Helpers;

namespace SamplePlugin.NamePlate;

public static class Feature
{
    public static void Enable()
    {
        Service.NamePlateGui.OnNamePlateUpdate += OnNamePlateUpdate;
    }

    public static void Dispose()
    {
        Service.NamePlateGui.OnNamePlateUpdate -= OnNamePlateUpdate;
    }

    public static void SetNamePlateIconID()
    {
        var namePlateGui = Service.NamePlateGui;
        namePlateGui.OnDataUpdate += (context, handlers) =>
        {
            foreach (Dalamud.Game.Gui.NamePlate.INamePlateUpdateHandler handler in handlers)
            {
                if (handler.IsUpdating || context.IsFullUpdate)
                {
                    handler.MarkerIconId = 66181 + (int)handler.NamePlateKind;
                }
                else
                {
                    handler.MarkerIconId = 66161 + (int)handler.NamePlateKind;
                }
            }
        };
    }

    // Monster ranks https://ffxiv.consolegameswiki.com/wiki/Aggression
    // 61707 rank 1 Regular overworld enemies; additional enemies in dungeon, trial, and raid bosses 
    // 61708 rank 2 Regular Dungeon enemies 
    // 61709 rank 3 Regular 8-player raid enemies (levels 50 and 60 only) 
    // 61710 rank 4 FATE Bosses/Elite Hunt Marks/Strong overworld enemies 
    // 61711 rank 5 Dungeon mid-bosses 
    // 61712 rank 6 Dungeon, Trial, and Raid final bosses 
    public static void OnNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        foreach (INamePlateUpdateHandler handler in handlers)
        {
            Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter? localPlayer = Svc.ClientState.LocalPlayer;
            if (localPlayer == null)
            {
                continue;
            }

            if (handler.NamePlateKind == NamePlateKind.PlayerCharacter || handler.NamePlateKind == NamePlateKind.BattleNpcEnemy)
            {
                int nameIconId = handler.NameIconId;
                var objectId = handler.GameObjectId;
                var name = handler.Name.ToString();
                var isBoss = handler.IsBossFromNamePlateIconId();

                Svc.Log.Info($"Nameplate Update - Name: {name}, ObjectId: {objectId:X8}, NameIconId: {nameIconId}, IsBoss: {isBoss}");
            }
        }
    }

    public static readonly List<NamePlateEntry> namePlateEntries = new();
    //public static readonly object namePlateLock = new();
    public static void OnNamePlateUpdate2(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        foreach (INamePlateUpdateHandler handler in handlers)
        {
            if (handler.NamePlateKind != NamePlateKind.PlayerCharacter && handler.NamePlateKind != NamePlateKind.BattleNpcEnemy)
                continue;

            NamePlateEntry? existing = namePlateEntries.FirstOrDefault(e => !e.IsTargetable/* && handler.GameObject != null*/);
            if (existing != null)
            {
                namePlateEntries.Remove(existing);
            }
            else
            {
                if (handler.BattleChara != null && handler.BattleChara.IsTargetable)
                {
                    // Add new entry
                    namePlateEntries.Add(new NamePlateEntry
                    {
                        Name = handler.Name.ToString(),
                        ObjectId = (uint)handler.GameObjectId,
                        NameIconId = handler.NameIconId,
                        IsBoss = handler.IsBossFromNamePlateIconId()
                    });
                }
            }
        }
    }

}
