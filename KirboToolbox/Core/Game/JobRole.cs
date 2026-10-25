using Dalamud.Game.ClientState.Objects.Types;

using ECommons.ExcelServices;

using Lumina.Excel.Sheets;

namespace KirboToolbox.Core.Game;

/// <summary>
/// The role of jobs.
/// </summary>
public enum JobRole : byte
{
    None = 0,
    Tank = 1,
    Melee = 2,
    Ranged = 3,
    Healer = 4,
    RangedPhysical = 5,
    RangedMagical = 6,
    DiscipleOfTheLand = 7,
    DiscipleOfTheHand = 8,
}

/// <summary>
/// Ways to get the <see cref="JobRole"/> of a job or character.
/// </summary>
public static class JobRoleExtension
{
    /// <summary>Get the role from a ClassJob sheet row.</summary>
    public static JobRole GetJobRole(this ClassJob job)
    {
        var role = (JobRole)job.Role;

        if (role is JobRole.Ranged or JobRole.None)
        {
            role = job.ClassJobCategory.RowId switch
            {
                30 => JobRole.RangedPhysical,
                31 => JobRole.RangedMagical,
                32 => JobRole.DiscipleOfTheLand,
                33 => JobRole.DiscipleOfTheHand,
                _ => JobRole.None,
            };
        }

        return role;
    }

    /// <summary>Role of a character's job. Base classes count as their upgraded job (GLA = PLD).</summary>
    public static JobRole GetJobRole(this IBattleChara chara)
    {
        var job = ((Job)chara.ClassJob.RowId).GetUpgradedJob();

        if (job.IsTank())
        {
            return JobRole.Tank;
        }

        if (job.IsHealer())
        {
            return JobRole.Healer;
        }

        if (job.IsMeleeDps())
        {
            return JobRole.Melee;
        }

        if (job.IsPhysicalRangedDps())
        {
            return JobRole.RangedPhysical;
        }

        if (job.IsMagicalRangedDps())
        {
            return JobRole.RangedMagical;
        }

        if (job.IsDoh())
        {
            return JobRole.DiscipleOfTheHand;
        }

        if (job.IsDol())
        {
            return JobRole.DiscipleOfTheLand;
        }

        return JobRole.None;
    }
}
