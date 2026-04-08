using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    public PlayerProfileSaveData Profile { get; private set; } = new PlayerProfileSaveData();

    private float sessionStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        sessionStartTime = Time.time;
    }

    public void ResetSessionTimer()
    {
        sessionStartTime = Time.time;
    }

    public float GetCurrentSessionTime()
    {
        return Time.time - sessionStartTime;
    }

    public void AddPlaytimeFromSession()
    {
        Profile.totalPlayTimeSeconds += GetCurrentSessionTime();
        ResetSessionTimer();
    }

    // ---------------------------
    // Identity
    // ---------------------------
    public void SetPlayerName(string name)
    {
        Profile.playerName = name;
    }

    public void SetSteamName(string steamName)
    {
        Profile.steamName = steamName;
    }

    // ---------------------------
    // Economy Stats
    // ---------------------------
    public void AddMoneyEarned(int amount)
    {
        if (amount <= 0) return;
        Profile.totalMoneyEarned += amount;
    }

    public void AddMoneySpent(int amount)
    {
        if (amount <= 0) return;
        Profile.totalMoneySpent += amount;
    }

    // ---------------------------
    // Plant / Progression Stats
    // ---------------------------
    public void AddPlantGrown(int amount = 1)
    {
        if (amount <= 0) return;
        Profile.totalPlantsGrown += amount;
    }

    public void AddHarvest(int amount = 1)
    {
        if (amount <= 0) return;
        Profile.totalHarvests += amount;
    }

    public void SetStrainsUnlocked(int count)
    {
        Profile.totalStrainsUnlocked = Mathf.Max(0, count);
    }

    // ---------------------------
    // Save/Load Integration
    // ---------------------------
    public void LoadFromSave(PlayerProfileSaveData savedProfile)
    {
        if (savedProfile == null)
        {
            Profile = new PlayerProfileSaveData();
        }
        else
        {
            Profile = savedProfile;
        }

        ResetSessionTimer();
    }

    public PlayerProfileSaveData GetSaveData()
    {
        return Profile;
    }
}