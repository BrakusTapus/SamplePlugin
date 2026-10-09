using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using SamplePlugin.Configs;
using SamplePlugin.Data;

namespace SamplePlugin.UI;

/// <summary>
/// The "Highlight Info" tab: a list of the active highlight rules plus an Add menu.
/// Everything is generated from <see cref="HighlightCatalog"/>, so this class never needs to change
/// when a new type of thing to highlight is added.
/// </summary>
internal sealed class HighlightTab
{
    private const string AddPopupId = "highlight_add_popup";

    private readonly Plugin plugin;
    private string search = string.Empty;
    private bool colorDirty;

    public HighlightTab(Plugin plugin) => this.plugin = plugin;

    private Configuration Config => plugin.Configuration;

    public void Draw()
    {
        var enabled = Config.EnableHighLightOverlay;
        if (ImGui.Checkbox("Enable highlight overlay", ref enabled))
        {
            Config.EnableHighLightOverlay = enabled;
            plugin.TargetHighlightWindow.IsOpen = enabled;
            Config.Save();
        }

        if (!enabled)
            return;

        var self = Config.HighlightPlayer;
        if (ImGui.Checkbox("Highlight yourself", ref self))
        {
            Config.HighlightPlayer = self;
            Config.Save();
        }

        ImGui.Spacing();
        DrawRulesTable();
        ImGui.Spacing();
        DrawAddButton();

        ImGui.Separator();
        DrawAppearance();

        // Colour pickers change every frame while dragging; only write the file once the mouse is released.
        if (colorDirty && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            Config.Save();
            colorDirty = false;
        }
    }

    private void DrawRulesTable()
    {
        // Catalog order = the order rules are applied, so the table shows it too.
        var rules = HighlightCatalog.All.Where(c => Config.Highlights.ContainsKey(c.Id)).ToList();
        if (rules.Count == 0)
        {
            ImGui.TextDisabled("Nothing is highlighted yet. Use \"Add\" to pick what to highlight.");
            return;
        }

        var columns = RelationInfo.Columns;
        var scale = ImGuiHelpers.GlobalScale;

        using var table = ImRaii.Table("highlight_rules", columns.Length + 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        if (!table.Success)
            return;

        ImGui.TableSetupColumn("What", ImGuiTableColumnFlags.WidthStretch);
        foreach (var rel in columns)
            ImGui.TableSetupColumn(rel.Label(), ImGuiTableColumnFlags.WidthFixed, 62f * scale);
        ImGui.TableSetupColumn("Color", ImGuiTableColumnFlags.WidthFixed, 44f * scale);
        ImGui.TableSetupColumn(" ", ImGuiTableColumnFlags.WidthFixed, 26f * scale);

        DrawHeaderRow(rules, columns);

        string? removeId = null;
        foreach (var category in rules)
        {
            var settings = Config.Highlights[category.Id];
            ImGui.TableNextRow();

            // What (click the label to toggle the whole row)
            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{category.Label}##row_{category.Id}"))
                ToggleRow(category, settings);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Click to toggle the whole row.");

            // Relation checkboxes
            foreach (var rel in columns)
            {
                ImGui.TableNextColumn();
                if ((category.Available & rel) == 0)
                {
                    ImGui.TextDisabled("-");
                    continue;
                }

                var on = (settings.Relations & rel) != 0;
                if (ImGui.Checkbox($"##{category.Id}_{rel}", ref on))
                {
                    settings.Relations = on ? settings.Relations | rel : settings.Relations & ~rel;
                    Config.Save();
                }
            }

            // Colour
            ImGui.TableNextColumn();
            var color = settings.Color;
            if (ImGui.ColorEdit4($"##color_{category.Id}", ref color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
            {
                settings.Color = color;
                colorDirty = true;
            }

            // Remove
            ImGui.TableNextColumn();
            if (ImGui.SmallButton($"X##remove_{category.Id}"))
                removeId = category.Id;
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Stop highlighting this.");
        }

        if (removeId != null)
        {
            Config.Highlights.Remove(removeId);
            Config.Save();
        }
    }

    /// <summary>Header row with tooltips; clicking a relation header toggles that whole column.</summary>
    private void DrawHeaderRow(System.Collections.Generic.List<HighlightCategory> rules, Relation[] columns)
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

        ImGui.TableSetColumnIndex(0);
        ImGui.TableHeader("What");

        for (var i = 0; i < columns.Length; i++)
        {
            var rel = columns[i];
            ImGui.TableSetColumnIndex(i + 1);
            ImGui.TableHeader(rel.Label());
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"{rel.Tooltip()}\nClick to toggle the whole column.");
            if (ImGui.IsItemClicked())
                ToggleColumn(rules, rel);
        }

        ImGui.TableSetColumnIndex(columns.Length + 1);
        ImGui.TableHeader("Color");
        ImGui.TableSetColumnIndex(columns.Length + 2);
        ImGui.TableHeader(" ");
    }

    private void ToggleRow(HighlightCategory category, CategorySettings settings)
    {
        var allOn = (settings.Relations & category.Available) == category.Available;
        settings.Relations = allOn ? Relation.None : category.Available;
        Config.Save();
    }

    private void ToggleColumn(System.Collections.Generic.List<HighlightCategory> rules, Relation rel)
    {
        var applicable = rules.Where(c => (c.Available & rel) != 0).ToList();
        if (applicable.Count == 0)
            return;

        var anyOff = applicable.Any(c => (Config.Highlights[c.Id].Relations & rel) == 0);
        foreach (var c in applicable)
        {
            var s = Config.Highlights[c.Id];
            s.Relations = anyOff ? s.Relations | rel : s.Relations & ~rel;
        }
        Config.Save();
    }

    private void DrawAddButton()
    {
        if (ImGui.Button("+ Add what to highlight"))
        {
            search = string.Empty;
            ImGui.OpenPopup(AddPopupId);
        }

        using var popup = ImRaii.Popup(AddPopupId);
        if (!popup.Success)
            return;

        var scale = ImGuiHelpers.GlobalScale;
        ImGui.SetNextItemWidth(280f * scale);
        ImGui.InputTextWithHint("##highlight_search", "Search...", ref search, 64);
        ImGui.Separator();

        using var child = ImRaii.Child("##highlight_add_list", new Vector2(280f * scale, 240f * scale));
        if (!child.Success)
            return;

        var term = search.Trim();
        var anyShown = false;

        foreach (var group in HighlightCatalog.All.GroupBy(c => c.Group))
        {
            var items = group
                .Where(c => !Config.Highlights.ContainsKey(c.Id) && MatchesSearch(c, term))
                .ToList();
            if (items.Count == 0)
                continue;

            anyShown = true;
            ImGui.TextDisabled(group.Key);

            foreach (var category in items)
            {
                if (!ImGui.Selectable($"{category.Label}##add_{category.Id}"))
                    continue;

                Config.Highlights[category.Id] = new CategorySettings
                {
                    Relations = category.InitialRelations,
                    Color = category.DefaultColor,
                };
                Config.Save();
                ImGui.CloseCurrentPopup();
            }
        }

        if (!anyShown)
            ImGui.TextDisabled(term.Length == 0 ? "Everything is already added." : "No matches.");
    }

    private static bool MatchesSearch(HighlightCategory c, string term) =>
        term.Length == 0
        || c.Label.Contains(term, StringComparison.OrdinalIgnoreCase)
        || c.Group.Contains(term, StringComparison.OrdinalIgnoreCase);

    private void DrawAppearance()
    {
        if (!ImGui.CollapsingHeader("Appearance (all highlights)"))
            return;

        var gradient = Config.UseGradientColor;
        if (ImGui.Checkbox("Use animated gradient color", ref gradient))
        {
            Config.UseGradientColor = gradient;
            Config.Save();
        }
    }
}
