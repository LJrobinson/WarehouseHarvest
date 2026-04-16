using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    public int CurrentDay { get; private set; } = 1;

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

    public void AdvanceDay()
    {
        CurrentDay++;
        Debug.Log($"Day advanced to {CurrentDay}");

        // Market updates once per day
        if (MarketManager.Instance != null)
        {
            MarketManager.Instance.MarketTick(CurrentDay);
        }
    }

    public void SetDay(int day)
    {
        CurrentDay = day;
    }
}