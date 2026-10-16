using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using SamplePlugin.Helpers.Structs;

namespace SamplePlugin.Helpers.Extensions;

public static unsafe class PlayerExtensions
{
    extension(IPlayerCharacter pc)
    {
        public float CurrentSpeed => ((ControlCustom*)Control.Instance())->CurrentGroundSpeed;
        public float Speed { get => pc.CurrentSpeed / 6f; set => SetSpeed(6 * value); }
        public ReviveState ReviveState => pc.IsDead ? AgentRevive.Instance()->State : ReviveState.Alive;

        public static FlagMapMarker MapFlag => AgentMap.Instance()->FlagMapMarkers[0];
        public static List<MapMarkerData> QuestLocations => [.. Map.Instance()->QuestMarkers.ToArray().SelectMany(i => i.MarkerData.ToList())];

        public void SetPosition(Vector3 destination) => pc.Character->SetPosition(destination.X, destination.Y, destination.Z);

        private static void SetSpeed(float speedBase)
        {
            ((ControlCustom*)Control.Instance())->GroundSpeedBase = speedBase;
            SetMoveControlData(speedBase);
        }

        private static void SetMoveControlData(float speed)
            => Dalamud.SafeMemory.Write(((delegate* unmanaged[Stdcall]<byte, nint>)ISigScanner.Get().ScanText(ExdModule_GetMoveControlRow_1))(1) + 8, speed);
    }

    private const string ExdModule_GetMoveControlRow_1 = "E8 ?? ?? ?? ?? 48 85 C0 74 AE 83 FD 05";
}
