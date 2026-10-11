using System;

namespace SamplePlugin.Data;

/// <summary>
/// How an object relates to the local player. Flags, so one rule can select several at once.
/// Every object has exactly ONE relation (see ObjectHelper.GetRelation), so columns never overlap.
/// </summary>
[Flags]
public enum Relation
{
    None     = 0,
    Party    = 1 << 0,
    Alliance = 1 << 1,
    /// <summary>Friendly or neutral, and not in your party or alliance.</summary>
    Others   = 1 << 2,
    Enemy    = 1 << 3,
    All      = Party | Alliance | Others | Enemy,
}

public static class RelationInfo
{
    /// <summary>Columns shown in the highlight tab, in order. Add a Relation here to get a new column.</summary>
    public static readonly Relation[] Columns = { Relation.Party, Relation.Alliance, Relation.Others, Relation.Enemy };

    public static string Label(this Relation r) => r switch
    {
        Relation.Party => "Party",
        Relation.Alliance => "Alliance",
        Relation.Others => "Others",
        Relation.Enemy => "Enemy",
        _ => r.ToString(),
    };

    public static string Tooltip(this Relation r) => r switch
    {
        Relation.Party => "Members of your party.",
        Relation.Alliance => "Members of your alliance who are not in your party.",
        Relation.Others => "Friendly or neutral players, NPCs and objects that are not in your party or alliance.",
        Relation.Enemy => "Anything hostile to you.",
        _ => string.Empty,
    };
}
