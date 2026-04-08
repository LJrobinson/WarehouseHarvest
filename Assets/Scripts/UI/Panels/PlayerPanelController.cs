using TMPro;
using UnityEngine;

public class PlayerPanelController : MonoBehaviour
{
    [Header("References")]
    public PlayerStatsManager playerStats;
    public EconomyManager economy;

    [Header("UI Fields")]
    public TMP_Text steamNameText;
    public TMP_Text playerNameText;

    public TMP_Text totalPlaytimeText;

    public TMP_Text currentMoneyText;
    public TMP_Text totalMoneyEarnedText;
    public TMP_Text totalMoneySpentText;

    public TMP_Text plantsGrownText;
    public TMP_Text harvestsText;
    public TMP_Text strainsUnlockedText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (playerStats == null || playerStats.Profile == null)
            return;

        var profile = playerStats.Profile;

        if (steamNameText != null)
            steamNameText.text = string.IsNullOrEmpty(profile.steamName) ? "Steam Name: Unknown..." : profile.steamName;

        if (playerNameText != null)
            playerNameText.text = string.IsNullOrEmpty(profile.playerName) ? "Player Name: N/A" : profile.playerName;

        float totalSeconds = profile.totalPlayTimeSeconds + playerStats.GetCurrentSessionTime();

        if (totalPlaytimeText != null)
            totalPlaytimeText.text = $"Total Playtime: " + FormatTime(totalSeconds);

        if (economy != null && currentMoneyText != null)
            currentMoneyText.text = $"Money: ${economy.Money:N0}";

        if (totalMoneyEarnedText != null)
            totalMoneyEarnedText.text = $"Total Money Earned: ${profile.totalMoneyEarned:N0}";

        if (totalMoneySpentText != null)
            totalMoneySpentText.text = $"Total Money Spent: ${profile.totalMoneySpent:N0}";

        if (plantsGrownText != null)
            plantsGrownText.text = $"Plants Grown: " + profile.totalPlantsGrown.ToString("N0");

        if (harvestsText != null)
            harvestsText.text = $"Harvests: " + profile.totalHarvests.ToString("N0");

        if (strainsUnlockedText != null)
            strainsUnlockedText.text = $"Strains Unlocked: " + profile.totalStrainsUnlocked.ToString("N0");
    }

    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int secs = totalSeconds % 60;

        if (hours > 0)
            return $"{hours}h {minutes}m";

        if (minutes > 0)
            return $"{minutes}m {secs}s";

        return $"{secs}s";
    }
}