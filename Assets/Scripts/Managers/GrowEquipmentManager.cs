using UnityEngine;

public class GrowEquipmentManager : MonoBehaviour
{
    public LightQuality lightQuality = LightQuality.Basic;
    public WaterQuality waterQuality = WaterQuality.Tap;

    public float GetLightGrowthBonus()
    {
        return lightQuality switch
        {
            LightQuality.Basic => 1.0f,
            LightQuality.LED => 1.10f,
            LightQuality.HighEndLED => 1.25f,
            LightQuality.Commercial => 1.45f,
            _ => 1.0f
        };
    }

    public float GetWaterStressReduction()
    {
        return waterQuality switch
        {
            WaterQuality.Tap => 0.0f,
            WaterQuality.Filtered => 0.5f,
            WaterQuality.ReverseOsmosis => 1.5f,
            WaterQuality.UltraPure => 3.0f,
            _ => 0.0f
        };
    }

    public float GetDiseaseResistanceBonus()
    {
        return waterQuality switch
        {
            WaterQuality.Tap => 0.0f,
            WaterQuality.Filtered => 0.02f,
            WaterQuality.ReverseOsmosis => 0.05f,
            WaterQuality.UltraPure => 0.08f,
            _ => 0.0f
        };
    }
}