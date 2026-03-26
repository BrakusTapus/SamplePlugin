//TODO Make an easy to use method that can check for a objects role/job
//TODO seperate highlightable objects/targets based on players/npc/gameobjects/friendly/enemy/aliance/jobs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using SamplePlugin.Configs;
using SamplePlugin.Data;
using SamplePlugin.Helpers;
using SamplePlugin.Helpers.UI;
using SamplePlugin.Updaters;

namespace SamplePlugin.UI;

internal class TargetHighlight : Window
{
    private Plugin plugin;
    private Configuration Configuration;
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
        this.plugin = plugin;
        Configuration = plugin.Configuration;
    }

    public override void Draw()
    {
        // Create a dictionary to store the filtered results for each job role.
        Dictionary<JobRole, IEnumerable<IBattleChara>> filteredResults = new Dictionary<JobRole, IEnumerable<IBattleChara>>();
        if (Svc.Objects.LocalPlayer == null)
        {
            return;
        }

        if (Svc.Condition[ConditionFlag.BetweenAreas])
        {
            return;
        }

        bool highlightOverlayValue = Configuration.EnableHighLightOverlay;
        if (highlightOverlayValue)
        {
            ImGuiWindowFlags imGuiWindowFlags = ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground;
            if (ImGui.Begin("Canvas_Hightlight", imGuiWindowFlags))
            {
                // Filter AllGameObjects to include only targetable objects.
                IEnumerable<IGameObject> battleCharas = MainUpdater.AllGameObjects.OfType<IGameObject>() .Where(b => b.IsTargetable);

                // Get the player character.
                IPlayerCharacter player = Svc.Objects.LocalPlayer;

                // Iterate through your BattleChara targets and call TargetHighlight method for each one.
                foreach (IGameObject target in battleCharas)
                {
                    bool highlightPlayer = Configuration.HighlightPlayer;
                    if (highlightPlayer)
                    {
                        if (target == player)
                        {
                            var playerColor = Configuration.UseGradientColor
                                ? GetGradientColor()
                                : ImGuiColors.DalamudViolet;
                            DrawWorldSpaceRectangleAroundGameObject(player, playerColor);
                        }
                    }
                    bool highlightGameObjects = Configuration.HighlightAllGameObjects;
                    if (highlightGameObjects)
                    {
                        // Check if the target is not the player character.
                        if (target != player)
                        {
                            HighlightAllGameObjects(target); // Use the updated TargetHighlight method without specifying a color.
                        }
                    }
                }
            }
        }

        ImGui.End();
    }


    /// <summary>
    /// Returns a smoothly animated rainbow color that cycles over a 1-second period.
    /// Useful for making highlights visually distinct without needing a static colour choice.
    /// </summary>
    private static Vector4 GetGradientColor()
    {
        const float PERIOD = 1f;
        var t     = (float)ImGui.GetTime() % PERIOD               / PERIOD;
        var red   = (MathF.Sin(2 * MathF.PI * t)             + 1) / 2;
        var green = (MathF.Sin(2 * MathF.PI * (t + 1f / 3f)) + 1) / 2;
        var blue  = (MathF.Sin(2 * MathF.PI * (t + 2f / 3f)) + 1) / 2;
        return new Vector4(red, green, blue, 1f);
    }

    /// <summary>
    /// Draws a layered glow bloom around a rectangle by rendering multiple expanding
    /// filled rects with decreasing alpha. Call this *before* AddRect so the crisp
    /// border sits on top of the glow.
    /// </summary>
    /// <param name="drawList">The ImGui draw list to render into.</param>
    /// <param name="min">Top-left corner of the base rectangle.</param>
    /// <param name="max">Bottom-right corner of the base rectangle.</param>
    /// <param name="colU32">Packed RGBA colour (use ImGui.GetColorU32).</param>
    /// <param name="rounding">Corner rounding radius, should match your AddRect call.</param>
    /// <param name="glowSize">How far (in pixels) the glow spreads outward.</param>
    /// <param name="steps">Number of glow layers — more = smoother but more draw calls.</param>
    /// <param name="innerBoost">Extra brightness boost near the rectangle edge (0 = none).</param>
    private static void AddGlowRect(
        ImDrawListPtr drawList,
        Vector2 min,
        Vector2 max,
        uint colU32,
        float rounding,
        float glowSize = 8f,
        int steps = 8,
        float innerBoost = 0.25f)
    {
        for (var i = 0; i < steps; i++)
        {
            var t      = (i + 1f) / steps;
            var expand = t        * glowSize;

            // Quadratic falloff so the glow is bright near the rect and fades outward.
            var a = 1f - t;
            a *= a;
            a *= 1f / steps * 2.2f;
            a *= 1f + (1f - t) * innerBoost;

            // Unpack the base colour, override alpha with the computed falloff value.
            var c = ColorU32ToVector4(colU32);
            c.W *= a;
            var cU32 = ColorVector4ToU32(c);

            var r = rounding + expand * 0.6f;
            drawList.AddRectFilled(
                min - new Vector2(expand),
                max + new Vector2(expand),
                cU32,
                r
            );
        }
    }

    // Inline colour conversion helpers
    private static Vector4 ColorU32ToVector4(uint col)
    {
        return new Vector4(
            (col & 0xFF) / 255f,
            ((col >> 8) & 0xFF) / 255f,
            ((col >> 16) & 0xFF) / 255f,
            ((col >> 24) & 0xFF) / 255f
        );
    }

    private static uint ColorVector4ToU32(Vector4 col)
    {
        var r = (uint)Math.Clamp((int)(col.X * 255f), 0, 255);
        var g = (uint)Math.Clamp((int)(col.Y * 255f), 0, 255);
        var b = (uint)Math.Clamp((int)(col.Z * 255f), 0, 255);
        var a = (uint)Math.Clamp((int)(col.W * 255f), 0, 255);
        return r | (g << 8) | (b << 16) | (a << 24);
    }


    private static unsafe void HighlightAllGameObjects(IGameObject target)
    {
        GameObject* gameObject = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address;

        // World positions
        Vector3 basePosition = target.Position;
        Vector3 topPosition = basePosition with { Y = basePosition.Y + gameObject->Height + 0.85f };

        // Project both to screen
        if (!Svc.GameGui.WorldToScreen(basePosition, out var screenBase))
        {
            return;
        }

        if (!Svc.GameGui.WorldToScreen(topPosition, out var screenTop))
        {
            return;
        }

        // Calculate rectangle width based on distance
        FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object camera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera->Object;
        float distance = Utils.DistanceBetweenObjects(camera.Position, target.Position, 0);
        float scale = 100 * (25 / distance);
        float width = (float)(scale * target.HitboxRadius);

        // Draw rectangle from top to bottom
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
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
        {
            return;
        }

        // Icon dimensions
        const float iconSize = 22f;
        Vector2 iconTopLeft = new Vector2(screenTop.X - iconSize / 2f, screenTop.Y - iconSize - 4f); // 4px padding above rectangle
        Vector2 iconBottomRight = iconTopLeft + new Vector2(iconSize, iconSize);

        // Draw icon image
        drawList.AddImage(
            icon.Handle,
            iconTopLeft,
            iconBottomRight
        );
    }

    private static unsafe void DrawWorldSpaceRectangleAroundGameObject(IGameObject gameObject, Vector4 color, float thickness = 3f, float rounding = 5f, ImDrawFlags drawFlags = ImDrawFlags.RoundCornersAll)
    {
        if (gameObject == null || gameObject.Address == IntPtr.Zero)
        {
            return;
        }

        GameObject* objStruct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)gameObject.Address;

        // World-space positions: from base to top of object
        Vector3 baseWorldPos = gameObject.Position;
        Vector3 topWorldPos = baseWorldPos with { Y = baseWorldPos.Y + objStruct->Height + 0.85f };

        // Project world to screen space
        if (!Svc.GameGui.WorldToScreen(baseWorldPos, out Vector2 screenBase) ||
            !Svc.GameGui.WorldToScreen(topWorldPos, out Vector2 screenTop))
        {
            return;
        }

        // Perspective scaling based on camera distance
        FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object camera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera->Object;
        float distance = Utils.DistanceBetweenObjects(camera.Position, gameObject.Position, 0);
        float scale = 100 * (25 / distance);
        float width = (float)(scale * gameObject.HitboxRadius);

        // Convert Vector4 color to uint
        uint packedColor = ImGui.GetColorU32(color);

        // Draw rectangle
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        drawList.AddRect(
            new Vector2(screenBase.X - width / 2f, screenTop.Y),
            new Vector2(screenBase.X + width / 2f, screenBase.Y),
            packedColor,
            rounding,
            drawFlags,
            thickness
        );
    }

    private static unsafe void DrawWorldSpaceRectangleAroundBattleChara(IBattleChara battleChara, Vector4 color, float thickness = 3f, float rounding = 5f, ImDrawFlags drawFlags = ImDrawFlags.RoundCornersAll)
    {
        if (battleChara == null || battleChara.Address == IntPtr.Zero)
            return;

        var objStruct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)battleChara.Address;

        // World-space positions: from base to top of object
        var baseWorldPos = battleChara.Position;
        var topWorldPos = baseWorldPos with { Y = baseWorldPos.Y + objStruct->Height + 0.85f };

        // Project world to screen space
        if (!Svc.GameGui.WorldToScreen(baseWorldPos, out var screenBase) ||
            !Svc.GameGui.WorldToScreen(topWorldPos, out var screenTop))
            return;

        // Perspective scaling based on camera distance
        var camera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera->Object;
        var distance = Utils.DistanceBetweenObjects(camera.Position, battleChara.Position, 0);
        var scale = 100 * (25 / distance);
        var width = (float)(scale * battleChara.HitboxRadius);

        // Convert Vector4 color to uint
        uint packedColor = ImGui.GetColorU32(color);

        // Draw rectangle
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRect(
            new Vector2(screenBase.X - width / 2f, screenTop.Y),
            new Vector2(screenBase.X + width / 2f, screenBase.Y),
            packedColor,
            rounding,
            drawFlags,
            thickness
        );
    }

    private static unsafe void HighlightAllHostiles(IGameObject target)
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

    private static unsafe void HighlightWithColor(IGameObject target, Vector4 color)
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
