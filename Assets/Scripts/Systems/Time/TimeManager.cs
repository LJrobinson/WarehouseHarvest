using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public int CurrentDay { get; private set; } = 1;

    public void AdvanceDay()
    {
        CurrentDay++;
        Debug.Log($"Day advanced to {CurrentDay}");
    }

    public void SetDay(int day)
    {
        CurrentDay = day;
    }
}