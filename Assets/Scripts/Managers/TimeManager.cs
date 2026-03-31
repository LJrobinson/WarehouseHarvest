using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public int CurrentDay { get; private set; } = 1;

    public void AdvanceDay()
    {
        CurrentDay++;
        Debug.Log($"Day advanced. Current day: {CurrentDay}");
    }
}