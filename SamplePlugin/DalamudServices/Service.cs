using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;

namespace SamplePlugin.DalamudServices;
public class Service
{
    //[PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    //[PluginService] public static IAddonEventManager AddonEventManager { get; private set; } = null!;
    //[PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    //[PluginService] public static IAetheryteList AetheryteList { get; private set; } = null!;
    //[PluginService] public static IBuddyList BuddyList { get; private set; } = null!;
    //[PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    //[PluginService] public static IClientState ClientState { get; private set; } = null!;
    //[PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    //[PluginService] public static ICondition Condition { get; private set; } = null!;
    //[PluginService] public static IContextMenu ContextMenu { get; private set; } = null!;
    //[PluginService] public static IDataManager DataManager { get; private set; } = null!;
    //[PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
    //[PluginService] public static IDutyState DutyState { get; private set; } = null!;
    //[PluginService] public static IFateTable FateTable { get; private set; } = null!;
    //[PluginService] public static IFlyTextGui FlyTextGui { get; private set; } = null!;
    //[PluginService] public static IFramework Framework { get; private set; } = null!;
    //[PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    //[PluginService] public static IGameGui GameGui { get; private set; } = null!;
    //[PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    //[PluginService] public static IGameInventory GameInventory { get; private set; } = null!;
    //[PluginService] public static IGameLifecycle GameLifecycle { get; private set; } = null!;
    //[PluginService] public static IGameNetwork GameNetwork { get; private set; } = null!;
    //[PluginService] public static IGamepadState GamepadState { get; private set; } = null!;
    //[PluginService] public static IJobGauges JobGauges { get; private set; } = null!;
    //[PluginService] public static IKeyState KeyState { get; private set; } = null!;
    //[PluginService] public static IMarketBoard MarketBoard { get; private set; } = null!;
    [PluginService] public static INamePlateGui NamePlateGui { get; private set; } = null!;
    //[PluginService] public static INotificationManager NotificationManager { get; private set; } = null!;
    //[PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    //[PluginService] public static IPartyFinderGui PartyFinderGui { get; private set; } = null!;
    //[PluginService] public static IPartyList PartyList { get; private set; } = null!;
    //[PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    //[PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
    //[PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static ITextureReadbackProvider TextureReadbackProvider { get; private set; } = null!;
    //[PluginService] public static ITextureSubstitutionProvider TextureSubstitutionProvider { get; private set; } = null!;
    //[PluginService] public static ITitleScreenMenu TitleScreenMenu { get; private set; } = null!;
    //[PluginService] public static IToastGui ToastGui { get; private set; } = null!;

    internal static bool IsInitialized = false;

    public static void Init(IDalamudPluginInterface pi)
    {
        if (IsInitialized)
        {
            Svc.Log.Warning("Services already initialized, skipping");
        }
        IsInitialized = true;
        try
        {
            pi.Create<Service>();
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex.ToString());
        }
    }

}
