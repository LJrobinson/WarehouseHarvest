using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySeedRowUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text strainNameText;
    public TMP_Text countText;
    public TMP_Text rarityBreakdownText;
    public TMP_Text shinyText;
    public Button button;

    private SeedInventorySummary summary;
    private Action<SeedInventorySummary> clickCallback;

    public void Setup(SeedInventorySummary summary, Action<SeedInventorySummary> onClick)
    {
        this.summary = summary;
        this.clickCallback = onClick;

        if (strainNameText != null)
            strainNameText.text = summary.isMystery ? "??? Bagseed" : summary.strain.strainName;

        if (countText != null)
            countText.text = summary.totalCount.ToString();

        if (rarityBreakdownText != null)
        {
            rarityBreakdownText.text =
                $"C:{summary.rarityCounts[SeedRarity.Common]} " +
                $"U:{summary.rarityCounts[SeedRarity.Uncommon]} " +
                $"R:{summary.rarityCounts[SeedRarity.Rare]} " +
                $"E:{summary.rarityCounts[SeedRarity.Epic]} " +
                $"L:{summary.rarityCounts[SeedRarity.Legendary]}";
        }

        if (shinyText != null)
            shinyText.text = summary.shinyCount > 0 ? $" {summary.shinyCount}" : "";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => clickCallback?.Invoke(this.summary));
        }
    }
}