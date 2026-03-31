using UnityEngine;

public class PlantInstance : MonoBehaviour
{
    [Range(0f, 100f)]
    public float growthPercent = 0f;

    public bool IsReadyToHarvest => growthPercent >= 100f;

    public void Grow(float amount)
    {
        growthPercent = Mathf.Clamp(growthPercent + amount, 0f, 100f);
        Debug.Log($"Plant grew. Growth is now: {growthPercent}%");
    }
}