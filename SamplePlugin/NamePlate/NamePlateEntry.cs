using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SamplePlugin.DalamudServices;

namespace SamplePlugin.NamePlate;
public sealed class NamePlateEntry
{
    public string Name { get; set; } = string.Empty;
    public ulong ObjectId { get; set; }
    public int NameIconId { get; set; }
    public bool IsBoss { get; set; }
    public bool IsTargetable { get; set; }
}
