using UnityEngine;
using TMPro;

public class InventorySeedDetailsUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text strainNameText;
    public TMP_Text totalSeedsText;
    public TMP_Text rarityBreakdownText;
    public TMP_Text shinyText;

    [Header("Stats UI (hook later)")]
    public TMP_Text firstHarvestText;
    public TMP_Text totalHarvestsText;
    public TMP_Text moneyEarnedText;
    public TMP_Text moneySpentText;

    public void Show(SeedInventorySummary summary)
    {
        var stats = StrainStatsManager.Instance.GetStats(summary.isMystery ? "MYSTERY" : summary.strain.strainName);

        firstHarvestText.text = stats.firstHarvestComplete ? stats.firstHarvestDate : "Not harvested yet";
        totalHarvestsText.text = stats.totalHarvests.ToString();
        moneyEarnedText.text = $"${stats.moneyEarned}";
        moneySpentText.text = $"${stats.moneySpentOnSeeds}";

        if (summary == null)
        {
            Clear();
            return;
        }

        if (strainNameText != null)
            strainNameText.text = summary.isMystery ? "??? Bagseed" : summary.strain.strainName;

        if (totalSeedsText != null)
            totalSeedsText.text = summary.totalCount.ToString();

        if (rarityBreakdownText != null)
        {
            rarityBreakdownText.text =
                $"Common: {summary.rarityCounts[SeedRarity.Common]}\n" +
                $"Uncommon: {summary.rarityCounts[SeedRarity.Uncommon]}\n" +
                $"Rare: {summary.rarityCounts[SeedRarity.Rare]}\n" +
                $"Epic: {summary.rarityCounts[SeedRarity.Epic]}\n" +
                $"Legendary: {summary.rarityCounts[SeedRarity.Legendary]}";
        }

        if (shinyText != null)
            shinyText.text = summary.shinyCount > 0 ? $"Shiny Seeds: {summary.shinyCount}" : "Shiny Seeds: 0";

        // Placeholder until stats system is wired
        if (firstHarvestText != null)
            firstHarvestText.text = "-";

        if (totalHarvestsText != null)
            totalHarvestsText.text = "-";

        if (moneyEarnedText != null)
            moneyEarnedText.text = "-";

        if (moneySpentText != null)
            moneySpentText.text = "-";
    }

    public void Clear()
    {
        if (strainNameText != null) strainNameText.text = "Select a strain...";
        if (totalSeedsText != null) totalSeedsText.text = "";
        if (rarityBreakdownText != null) rarityBreakdownText.text = "";
        if (shinyText != null) shinyText.text = "";

        if (firstHarvestText != null) firstHarvestText.text = "";
        if (totalHarvestsText != null) totalHarvestsText.text = "";
        if (moneyEarnedText != null) moneyEarnedText.text = "";
        if (moneySpentText != null) moneySpentText.text = "";
    }
}