using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using SamplePlugin.DalamudServices;
using SamplePlugin.Data;

namespace SamplePlugin.Helpers.UI;

internal static unsafe class ImGuiExt
{
    /// <summary>
    /// Obtain an icon texture in the game using its ID.
    /// </summary>
    /// <param name="iconId"></param>
    /// <returns></returns>
    internal static ISharedImmediateTexture GetGameIconTexture(uint iconId)
    {
        var path = Svc.Texture.GetIconPath(new GameIconLookup(iconId));
        return Svc.Texture.GetFromGame(path);
    }

    /// <summary>
    /// Round (i) Image with a tooltip
    /// </summary>
    /// <param name="desc">What gets shown on hover</param>
    internal static void HelpMarker(string desc)
    {
        if (desc.Length == 0)
            return;

        ImGuiComponents.HelpMarker(desc);
    }

    internal static void DrawBackgroundFill(uint textureId)
    {
        // Load the texture based on the given ID
        if (!GetTexture(textureId, out var texture))
            return;

        // Get the current window size
        Vector2 windowSize = ImGui.GetWindowSize();

        // Draw the background image, stretching it to fill the window size
        ImGui.SetCursorPos(Vector2.Zero);
        ImGui.Image(texture.Handle, windowSize);
    }

    public static void DrawJobIcon(IBattleChara bnpc)
    {
        ImGui.SameLine();

        if (GetTexture(bnpc.ClassJob.RowId + 62000, out Dalamud.Interface.Textures.TextureWraps.IDalamudTextureWrap? texture))
        {
            ImGui.Image(texture.Handle, Vector2.One * 24 * ImGuiHelpers.GlobalScale);
            //ImguiTooltips.HoveredTooltip(UiString.JobConfigTip.GetDescription());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="texture"></param>
    /// <param name="default"></param>
    /// <returns></returns>
    public static bool GetTexture(uint id, out IDalamudTextureWrap texture, uint @default = 0)
        => ThreadLoadImageHandler.TryGetIconTextureWrap(id, true, out texture)
        || ThreadLoadImageHandler.TryGetIconTextureWrap(@default, true, out texture)
        || ThreadLoadImageHandler.TryGetIconTextureWrap(0, true, out texture);

    /// <summary>
    /// Converts RGB color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBB</param>
    /// <param name="alpha">Optional transparency value between 0 and 1</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGB(uint col, float alpha = 1.0f)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f, alpha);
    }

    /// <summary>
    /// Converts RGBA color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBBAA</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGBA(uint col)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[3] / 255f, (float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f);
    }

    /// <summary>
    /// Returns a color based on the provided percentage value.
    /// </summary>
    public static Vector4 GetParsedColor(int percent)
    {
        if (percent < 25)
        {
            return ImGuiColors.ParsedGrey;
        }
        else if (percent < 50)
        {
            return ImGuiColors.ParsedGreen;
        }
        else if (percent < 75)
        {
            return ImGuiColors.ParsedBlue;
        }
        else if (percent < 95)
        {
            return ImGuiColors.ParsedPurple;
        }
        else if (percent < 99)
        {
            return ImGuiColors.ParsedOrange;
        }
        else if (percent == 99)
        {
            return ImGuiColors.ParsedPink;
        }
        else if (percent == 100)
        {
            return ImGuiColors.ParsedGold;
        }
        else
        {
            return ImGuiColors.DalamudRed;
        }
    }

    /// <summary>
    /// The type of the icon
    /// </summary>
    public enum IconType : byte
    {
        /// <summary>
        /// 
        /// </summary>
        Gold,

        /// <summary>
        /// 
        /// </summary>
        Framed,

        /// <summary>
        /// 
        /// </summary>
        Glowing,

        /// <summary>
        /// 
        /// </summary>
        Grey,

        /// <summary>
        /// 
        /// </summary>
        Black,

        /// <summary>
        /// 
        /// </summary>
        Yellow,

        /// <summary>
        /// 
        /// </summary>
        Orange,

        /// <summary>
        /// 
        /// </summary>
        Red,

        /// <summary>
        /// 
        /// </summary>
        Purple,

        /// <summary>
        /// 
        /// </summary>
        Blue,

        /// <summary>
        /// 
        /// </summary>
        Green,

        /// <summary>
        /// 
        /// </summary>
        Role,
    }

    private static readonly Dictionary<IconType, uint[]> _icons = new()
    {
        { IconType.Gold, new uint[42]
        {
        62001, 62002, 62003, 62004, 62005, 62006, 62007, 62008, 62009, 62010,
        62011, 62012, 62013, 62014, 62015, 62016, 62017, 62018, 62019, 62020,
        62021, 62022, 62023, 62024, 62025, 62026, 62027, 62028, 62029, 62030,
        62031, 62032, 62033, 62034, 62035, 62036, 62037, 62038, 62039, 62040,
        62041, 62042
        }
        },

        { IconType.Framed, new uint[42]
        {
        62101, 62102, 62103, 62104, 62105, 62106, 62107, 62108, 62109, 62110,
        62111, 62112, 62113, 62114, 62115, 62116, 62117, 62118, 62119, 62120,
        62121, 62122, 62123, 62124, 62125, 62126, 62127, 62128, 62129, 62130,
        62131, 62132, 62133, 62134, 62135, 62136, 62137, 62138, 62139, 62140,
        62141, 62142
        }
        },

        { IconType.Glowing, new uint[42]
        {
        62301, 62302, 62303, 62304, 62305, 62306, 62307, 62310, 62311, 62312,
        62313, 62314, 62315, 62316, 62317, 62318, 62319, 62320, 62401, 62402,
        62403, 62404, 62405, 62406, 62407, 62308, 62408, 62409, 62309, 62410,
        62411, 62412, 62413, 62414, 62415, 62416, 62417, 62418, 62419, 62420,
        62421, 62422
        }
        },

        { IconType.Grey, new uint[42]
        {
        91022, 91023, 91024, 91025, 91026, 91028, 91029, 91031, 91032, 91033,
        91034, 91035, 91036, 91037, 91038, 91039, 91040, 91041, 91079, 91080,
        91081, 91082, 91083, 91084, 91085, 91030, 91086, 91087, 91121, 91122,
        91125, 91123, 91124, 91127, 91128, 91129, 91130, 91131, 91132, 91133,
        91185, 91186
        }
        },

        { IconType.Black, new uint[42]
        {
        91522, 91523, 91524, 91525, 91526, 91528, 91529, 91531, 91532, 91533,
        91534, 91535, 91536, 91537, 91538, 91539, 91540, 91541, 91579, 91580,
        91581, 91582, 91583, 91584, 91585, 91530, 91586, 91587, 91621, 91622,
        91625, 91623, 91624, 91627, 91628, 91629, 91630, 91631, 91632, 91633,
        91685, 91686
        }
        },

        { IconType.Yellow, new uint[42]
        {
        92022, 92023, 92024, 92025, 92026, 92028, 92029, 92031, 92032, 92033,
        92034, 92035, 92036, 92037, 92038, 92039, 92040, 92041, 92079, 92080,
        92081, 92082, 92083, 92084, 92085, 92030, 92086, 92087, 92121, 92122,
        92125, 92123, 92124, 92127, 92128, 92129, 92130, 92131, 92132, 92133,
        92185, 92186
        }
        },

        { IconType.Orange, new uint[42]
        {
        92522, 92523, 92524, 92525, 92526, 92528, 92529, 92531, 92532, 92533,
        92534, 92535, 92536, 92537, 92538, 92539, 92540, 92541, 92579, 92580,
        92581, 92582, 92583, 92584, 92585, 92530, 92586, 92587, 92621, 92622,
        92625, 92623, 92624, 92627, 92628, 92629, 92630, 92631, 92632, 92633,
        92685, 92686
        }
        },


        { IconType.Red, new uint[42]
        {
        93022, 93023, 93024, 93025, 93026, 93028, 93029, 93031, 93032, 93033,
        93034, 93035, 93036, 93037, 93038, 93039, 93040, 93041, 93079, 93080,
        93081, 93082, 93083, 93084, 93085, 93030, 93086, 93087, 93121, 93122,
        93125, 93123, 93124, 93127, 93128, 93129, 93130, 93131, 93132, 93133,
        93185, 93186
        }
        },


        { IconType.Purple, new uint[42]
        {
        93522, 93523, 93524, 93525, 93526, 93528, 93529, 93531, 93532, 93533,
        93534, 93535, 93536, 93537, 93538, 93539, 93540, 93541, 93579, 93580,
        93581, 93582, 93583, 93584, 93585, 93530, 93586, 93587, 93621, 93622,
        93625, 93623, 93624, 93627, 93628, 93629, 93630, 93631, 93632, 93633,
        93685, 93686
        }
        },

        { IconType.Blue, new uint[42]
        {
        94022, 94023, 94024, 94025, 94026, 94028, 94029, 94031, 94032, 94033,
        94034, 94035, 94036, 94037, 94038, 94039, 94040, 94041, 94079, 94080,
        94081, 94082, 94083, 94084, 94085, 94030, 94086, 94087, 94121, 94122,
        94125, 94123, 94124, 94127, 94128, 94129, 94130, 94131, 94132, 94133,
        94185, 94186
        }
        },

        { IconType.Green, new uint[42]
        {
        94522, 94523, 94524, 94525, 94526, 94528, 94529, 94531, 94532, 94533,
        94534, 94535, 94536, 94537, 94538, 94539, 94540, 94541, 94579, 94580,
        94581, 94582, 94583, 94584, 94585, 94530, 94586, 94587, 94621, 94622,
        94625, 94623, 94624, 94627, 94628, 94629, 94630, 94631, 94632, 94633,
        94685, 94686
        }
        },
        { IconType.Role, new uint[42]
        {
        62581, 62584, 62581, 62584, 62586, 62582, 62502, 62502, 62503, 62504,
        62505, 62506, 62507, 62508, 62509, 62510, 62511, 62512, 62581, 62584,
        62581, 62584, 62586, 62582, 62587, 62587, 62587, 62582, 62584, 62584,
        62586, 62581, 62582, 62584, 62587, 62587, 62581, 62586, 62584, 62582,
        62841, 62842
        }
        },

    };

    /// <summary>
    /// Gets the job icon based on the job role and job.
    /// </summary>
    /// <param name="role">The role of the job.</param>
    /// <param name="job">The job.</param>
    /// <returns>The icon ID for the job.</returns>
    public static uint GetJobIcon(JobRole role, Job job)
    {
        IconType type = IconType.Gold;
        switch (role)
        {
            case JobRole.Tank:
                type = IconType.Blue;
                break;
            case JobRole.RangedPhysical:
            case JobRole.RangedMagical:
            case JobRole.Melee:
                type = IconType.Red;
                break;
            case JobRole.Healer:
                type = IconType.Green;
                break;
        }
        return GetJobIcon(job, type);
    }

    /// <summary>
    /// Gets the job icon based on the job.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <returns>The icon ID for the job.</returns>
    public static uint GetJobIcon(Job job)
    {
        Lumina.Excel.ExcelSheet<ClassJob> classJobSheet = Svc.Data.GetExcelSheet<ClassJob>();
        if (classJobSheet == null)
        {
            throw new InvalidOperationException("ClassJob sheet not found.");
        }

        ClassJob classJobRow = classJobSheet.GetRow((uint)job);
        return classJobRow.RowId == 0
            ? throw new InvalidOperationException($"ClassJob row for job {job} not found.")
            : GetJobIcon(classJobRow.GetJobRole(), job);
    }

    /// <summary>
    /// Gets the job icon based on the job and icon type.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <param name="type">The icon type.</param>
    /// <returns>The icon ID for the job.</returns>
    public static uint GetJobIcon(Job job, IconType type)
    {
        const uint AdvIconId = 62143;

        if (job == Job.ADV)
        {
            return AdvIconId;
        }

        if (!_icons.TryGetValue(type, out var iconsForType))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid icon type.");
        }

        int jobIndex = (int)job - 1;
        if (jobIndex < 0 || jobIndex >= iconsForType.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(job), "Invalid job for icon type.");
        }

        return iconsForType[jobIndex];
    }


    //private static readonly Dictionary<ImGuiCol, Vector4>   ImGuiColToVector4   = [];
    //private static readonly Dictionary<ImGuiCol, uint>      ImGuiColToUInt      = [];
    //private static readonly Dictionary<KnownColor, Vector4> KnownColorToVector4 = [];
    //private static readonly Dictionary<KnownColor, uint>    KnownColorToUInt    = [];
    //private static readonly Dictionary<uint, Vector4>       UIntToVector4       = [];

    ////extension(ImGuiCol imguiCol)
    ////{
    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public Vector4 ToVector4() =>
    //    //    ImGuiColToVector4.GetOrAdd(imguiCol, _ => ImGui.GetColorU32(imguiCol).ToVector4());

    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public uint ToUInt() =>
    //    //    ImGuiColToUInt.GetOrAdd(imguiCol, _ => ImGui.GetColorU32(imguiCol));
    ////}

    ////extension(KnownColor knownColor)
    ////{
    ////    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ////    public Vector4 ToVector4() =>
    ////        KnownColorToVector4.GetOrAdd(knownColor, _ => knownColor.Vector());

    ////    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ////    public uint ToUInt() =>
    ////        KnownColorToUInt.GetOrAdd(knownColor, _ => knownColor.ToVector4().ToUInt());
    ////}

    //extension(uint color)
    //{
    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public Vector4 ToVector4() =>
    //    //    UIntToVector4.GetOrAdd(color, _ => ImGui.ColorConvertU32ToFloat4(color));

    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public Vector4 GetVector4UIColor() =>
    //    //    AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color).ToVector4();

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public uint GetUIntUIColor() =>
    //        AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color);
    //}

    //extension(scoped in Vector4 color)
    //{
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public Vector3 ToVector3() =>
    //        new(color.X, color.Y, color.Z);

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public Vector4 Invert() =>
    //        color with { X = 1f - color.X, Y = 1f - color.Y, Z = 1f - color.Z };

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public ByteColor ToByteColor()
    //    {
    //        var r = (byte)Math.Round(Math.Clamp(color.X, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
    //        var g = (byte)Math.Round(Math.Clamp(color.Y, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
    //        var b = (byte)Math.Round(Math.Clamp(color.Z, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
    //        var a = (byte)Math.Round(Math.Clamp(color.W, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);

    //        return new ByteColor { R = r, G = g, B = b, A = a };
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public uint ToUInt() =>
    //        ImGui.ColorConvertFloat4ToU32(color);
    //}

    //extension(ByteColor color)
    //{
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public Vector4 ToVector4()
    //    {
    //        var r = color.R / 255f;
    //        var g = color.G / 255f;
    //        var b = color.B / 255f;
    //        var a = color.A / 255f;

    //        return new Vector4(r, g, b, a);
    //    }
    //}
}
