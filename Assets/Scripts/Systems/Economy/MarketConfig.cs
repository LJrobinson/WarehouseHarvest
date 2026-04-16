using UnityEngine;

[CreateAssetMenu(fileName = "MarketConfig", menuName = "Economy/Market Config")]
public class MarketConfig : ScriptableObject
{
    [Header("Price Limits")]
    public float minMultiplier = 0.2f;
    public float maxMultiplier = 2.5f;

    [Header("Supply Tuning")]
    [Tooltip("How much supplyIndex increases per gram harvested.")]
    public float supplyImpactPerGram = 0.01f;

    [Tooltip("How quickly supply decays each market tick. (0.90 = loses 10% per tick)")]
    public float supplyDecayRate = 0.90f;

    [Tooltip("How strongly supply reduces price. Higher = harsher price crashes.")]
    public float supplyWeight = 0.08f;

    [Header("Demand Tuning")]
    [Tooltip("How quickly demand decays each market tick.")]
    public float demandDecayRate = 0.95f;

    [Tooltip("How strongly demand increases price.")]
    public float demandWeight = 0.03f;

    [Header("Random Drift")]
    [Tooltip("Random daily drift added to demand index (ex: -0.2 to +0.2).")]
    public float dailyDemandDriftRange = 0.15f;

    [Header("Update Frequency")]
    [Tooltip("How many in-game hours between market updates.")]
    public int updateEveryHours = 24;

    [Header("NPC Market Simulation")]
    public bool simulateNPCMarket = true;

    [Tooltip("How much random NPC supply gets added per tick (scaled by popularity).")]
    public float npcSupplyBase = 5f;

    [Tooltip("How much random NPC demand gets added per tick (scaled by popularity).")]
    public float npcDemandBase = 1.5f;
}