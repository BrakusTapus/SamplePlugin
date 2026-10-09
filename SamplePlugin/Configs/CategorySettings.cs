using System;
using System.Numerics;
using SamplePlugin.Data;

namespace SamplePlugin.Configs;

/// <summary>Saved settings for ONE highlight category. A category is "active" if it has an entry in Configuration.Highlights.</summary>
[Serializable]
public class CategorySettings
{
    /// <summary>Which relation columns are ticked.</summary>
    public Relation Relations { get; set; } = Relation.None;

    public Vector4 Color { get; set; } = new(1f, 1f, 1f, 1f);
}
