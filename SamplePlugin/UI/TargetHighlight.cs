using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using SamplePlugin.Data;
using SamplePlugin.Helpers;
using SamplePlugin.Helpers.UI;
using SamplePlugin.Updaters;

namespace SamplePlugin.UI;
internal class TargetHighlight : Window
{
    private Plugin Plugin;
    public TargetHighlight(Plugin plugin)
        : base(nameof(TargetHighlight), ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar)
    {
        float displaySizeWidth = ImGui.GetIO().DisplaySize.X - 10;
        float displaySizeHeight = ImGui.GetIO().DisplaySize.Y - 10;
        Size = new Vector2(displaySizeWidth, displaySizeHeight); //Define's size of window
        SizeCondition = ImGuiCond.Always; //Locks window size
        Position = new Vector2(0, 0);
        PositionCondition = ImGuiCond.Always;
        RespectCloseHotkey = false;
        Plugin = plugin;
    }

    public override void Draw()
    {
        // Create a dictionary to store the filtered results for each job role.
        var filteredResults = new Dictionary<JobRole, IEnumerable<IBattleChara>>();
        if (Svc.ClientState.LocalPlayer == null)
        {
            return;
        }

        if (Svc.Condition[ConditionFlag.BetweenAreas])
        {
            return;
        }

        ImGuiWindowFlags imGuiWindowFlags = ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground;
        if (ImGui.Begin("Canvas_Hightlight", imGuiWindowFlags))
        {
            // Filter AllTargets to include only BattleChara objects.
            IEnumerable<IGameObject> battleCharas = MainUpdater.AllGameObjects.OfType<IGameObject>() .Where(b => b.IsTargetable);

            // Get the player character.
            Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter player = Svc.ClientState.LocalPlayer;

            // Iterate through your BattleChara targets and call TargetHighlight method for each one.
            foreach (IGameObject target in battleCharas)
            {
                if (target == player)
                {
                    HighlightPlayer(player);
                }
                // Check if the target is not the player character.
                if (target != player)
                {
                    HighlightAllGameObjects(target); // Use the updated TargetHighlight method without specifying a color.
                }
            }
        }

        ImGui.End();
    }

    private unsafe static void HighlightPlayer(IPlayerCharacter player)
    {
        var gameObject = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)player.Address;

        // World positions
        var basePosition = player.Position;
        var topPosition = basePosition with { Y = basePosition.Y + gameObject->Height + 0.85f };

        // Project both to screen
        if (!Svc.GameGui.WorldToScreen(basePosition, out var screenBase))
            return;

        if (!Svc.GameGui.WorldToScreen(topPosition, out var screenTop))
            return;

        // Calculate rectangle width based on distance
        var camera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera->Object;
        var distance = Utils.DistanceBetweenObjects(camera.Position, player.Position, 0);
        var scale = 100 * (25 / distance);
        var width = (float)(scale * player.HitboxRadius);

        // Draw rectangle from top to bottom
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRect(
            new Vector2(screenBase.X - width / 2f, screenTop.Y),
            new Vector2(screenBase.X + width / 2f, screenBase.Y),
            ImGui.GetColorU32(ImGuiColors.DalamudWhite),
            5f,
            ImDrawFlags.RoundCornersAll,
            3f
        );
        // Get icon texture
        Dalamud.Interface.Textures.TextureWraps.IDalamudTextureWrap? icon = ImGuiExt.GetGameIconTexture(55).GetWrapOrDefault(); // TODO make it so the icon is job based
        if (icon is null)
            return;

        // Icon dimensions
        const float iconSize = 22f;
        Vector2 iconTopLeft = new Vector2(screenTop.X - iconSize / 2f, screenTop.Y - iconSize - 4f); // 4px padding above rectangle
        Vector2 iconBottomRight = iconTopLeft + new Vector2(iconSize, iconSize);

        // Draw icon image
        drawList.AddImage(
            icon.ImGuiHandle,
            iconTopLeft,
            iconBottomRight
        );
    }

    private unsafe static void HighlightAllGameObjects(IGameObject target)
    {
        var gameObject = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address;

        // World positions
        var basePosition = target.Position;
        var topPosition = basePosition with { Y = basePosition.Y + gameObject->Height + 0.85f };

        // Project both to screen
        if (!Svc.GameGui.WorldToScreen(basePosition, out var screenBase))
            return;

        if (!Svc.GameGui.WorldToScreen(topPosition, out var screenTop))
            return;

        // Calculate rectangle width based on distance
        var camera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera->Object;
        var distance = Utils.DistanceBetweenObjects(camera.Position, target.Position, 0);
        var scale = 100 * (25 / distance);
        var width = (float)(scale * target.HitboxRadius);

        // Draw rectangle from top to bottom
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRect(
            new Vector2(screenBase.X - width / 2f, screenTop.Y),
            new Vector2(screenBase.X + width / 2f, screenBase.Y),
            ImGui.GetColorU32(ImGuiColors.DalamudYellow),
            5f,
            ImDrawFlags.RoundCornersAll,
            3f
        );
        // Get icon texture
        Dalamud.Interface.Textures.TextureWraps.IDalamudTextureWrap? icon = ImGuiExt.GetGameIconTexture(55).GetWrapOrDefault(); // TODO make it so the icon is job based
        if (icon is null)
            return;

        // Icon dimensions
        const float iconSize = 22f;
        Vector2 iconTopLeft = new Vector2(screenTop.X - iconSize / 2f, screenTop.Y - iconSize - 4f); // 4px padding above rectangle
        Vector2 iconBottomRight = iconTopLeft + new Vector2(iconSize, iconSize);

        // Draw icon image
        drawList.AddImage(
            icon.ImGuiHandle,
            iconTopLeft,
            iconBottomRight
        );
    }

    private unsafe static void HighlightAllHostiles(IGameObject target)
    {
        Svc.GameGui.WorldToScreen(target.Position, out var screenPos);
        var camera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera->Object;
        var distance = Utils.DistanceBetweenObjects(camera.Position, target.Position, 0);
        var size = (int)Math.Round(100 * (25 / distance)) * Math.Max(target.HitboxRadius, ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address)->Height);
        ImGui.GetWindowDrawList().AddRect(
            new Vector2(screenPos.X - size / 2, screenPos.Y),
            new Vector2(screenPos.X + size / 2, screenPos.Y - size),
            ImGui.GetColorU32(ImGuiColors.DalamudYellow), // Yellow color from your EColor class
            5f,
            ImDrawFlags.RoundCornersAll,
            3f);
    }

    private unsafe static void HighlightWithColor(IGameObject target, Vector4 color)
    {

        Svc.GameGui.WorldToScreen(target.Position, out var screenPos);
        var camera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera->Object;
        var distance = Utils.DistanceBetweenObjects(camera.Position, target.Position, 0);
        var size = (int)Math.Round(100 * (25 / distance)) * Math.Max(target.HitboxRadius, ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address)->Height);


        // Calculate the position for the text
        var bottomleft = new Vector2(screenPos.X - size / 2, screenPos.Y);
        var topleft = new Vector2(screenPos.X - size / 2, screenPos.Y - size - 15);
        var textX = screenPos.X - (ImGui.CalcTextSize("Text").X / 2);
        var textY = screenPos.Y - size - 15; // Adjust the Y position as needed

        ImGui.GetWindowDrawList().AddRect(
                    new Vector2(screenPos.X - size / 2, screenPos.Y - size), // Top-left corner coordinates
                    new Vector2(screenPos.X + size / 2, screenPos.Y), // Bottom-right corner coordinates
                    ImGui.GetColorU32(color), // Use the specified color
                    5f,
                    ImDrawFlags.RoundCornersAll,
                    3f);
    }

    public void Dispose() { }
}
