using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance;

    [Header("Config")]
    [SerializeField] private MarketConfig marketConfig;

    [Header("Database")]
    [SerializeField] private StrainDatabase strainDatabase;

    private Dictionary<string, MarketStrainState> marketStateByStrain = new();
    private bool initialized = false;

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
        InitializeMarket();
    }

    private void InitializeMarket()
    {
        if (initialized) return;

        if (marketConfig == null)
        {
            Debug.LogError("[MarketManager] Missing MarketConfig reference!");
            return;
        }

        if (strainDatabase == null)
        {
            Debug.LogError("[MarketManager] Missing StrainDatabase reference!");
            return;
        }

        marketStateByStrain.Clear();

        foreach (var strain in strainDatabase.strains)
        {
            if (strain == null) continue;

            string id = strain.strainName;

            MarketStrainState state = new MarketStrainState
            {
                strainID = id,
                supplyIndex = 0f,
                demandIndex = Random.Range(-0.5f, 0.5f),
                cachedMultiplier = 1f,
                cachedPrice = GetBasePrice(strain)
            };

            marketStateByStrain[id] = state;
        }

        initialized = true;
        Debug.Log($"[MarketManager] Initialized market with {marketStateByStrain.Count} strains.");
    }

    private float GetBasePrice(PlantStrainData strain)
    {
        // This assumes you have something like "basePrice" on PlantStrainData.
        // If you don't, add it.
        return strain.basePricePerGram;
    }

    public float GetPrice(string strainID)
    {
        if (!initialized) InitializeMarket();

        if (marketStateByStrain.TryGetValue(strainID, out MarketStrainState state))
            return state.cachedPrice;

        Debug.LogWarning($"[MarketManager] GetPrice failed, unknown strainID: {strainID}");
        return 0f;
    }

    public float GetMultiplier(string strainID)
    {
        if (!initialized) InitializeMarket();

        if (marketStateByStrain.TryGetValue(strainID, out MarketStrainState state))
            return state.cachedMultiplier;

        return 1f;
    }

    public void RegisterHarvest(string strainID, float gramsHarvested)
    {
        if (!initialized) InitializeMarket();

        if (!marketStateByStrain.TryGetValue(strainID, out MarketStrainState state))
            return;

        state.supplyIndex += gramsHarvested * marketConfig.supplyImpactPerGram;
    }

    public void RegisterDemandBoost(string strainID, float demandAmount)
    {
        if (!initialized) InitializeMarket();

        if (!marketStateByStrain.TryGetValue(strainID, out MarketStrainState state))
            return;

        state.demandIndex += demandAmount;
    }

    public void MarketTick(int currentGameHour)
    {
        if (!initialized) InitializeMarket();

        foreach (var kvp in marketStateByStrain)
        {
            MarketStrainState state = kvp.Value;

            // decay
            state.supplyIndex *= marketConfig.supplyDecayRate;
            state.demandIndex *= marketConfig.demandDecayRate;

            // random drift
            state.demandIndex += Random.Range(-marketConfig.dailyDemandDriftRange, marketConfig.dailyDemandDriftRange);

            // NPC simulation
            if (marketConfig.simulateNPCMarket)
            {
                float npcSupply = Random.Range(0f, marketConfig.npcSupplyBase);
                float npcDemand = Random.Range(0f, marketConfig.npcDemandBase);

                state.supplyIndex += npcSupply;
                state.demandIndex += npcDemand;
            }

            // calculate multiplier
            float supplyMultiplier = 1f / (1f + (state.supplyIndex * marketConfig.supplyWeight));
            float demandMultiplier = 1f + (state.demandIndex * marketConfig.demandWeight);

            float finalMultiplier = supplyMultiplier * demandMultiplier;
            finalMultiplier = Mathf.Clamp(finalMultiplier, marketConfig.minMultiplier, marketConfig.maxMultiplier);

            // base price lookup
            PlantStrainData strainData = strainDatabase.GetStrainByName(state.strainID);
            float basePrice = strainData != null ? GetBasePrice(strainData) : 0f;

            state.cachedMultiplier = finalMultiplier;
            state.cachedPrice = basePrice * finalMultiplier;
        }

        Debug.Log($"[MarketManager] MarketTick updated prices at hour {currentGameHour}.");
    }

    public MarketSaveData GetSaveData(int currentDay)
    {
        MarketSaveData saveData = new MarketSaveData();
        saveData.lastMarketUpdateDay = currentDay;
        
        foreach (var kvp in marketStateByStrain)
        {
            saveData.strainStates.Add(new MarketStrainState
            {
                strainID = kvp.Value.strainID,
                supplyIndex = kvp.Value.supplyIndex,
                demandIndex = kvp.Value.demandIndex,
                cachedPrice = kvp.Value.cachedPrice,
                cachedMultiplier = kvp.Value.cachedMultiplier
            });
        }

        return saveData;
    }

    public void LoadFromSaveData(MarketSaveData saveData)
    {
        if (!initialized) InitializeMarket();

        if (saveData == null || saveData.strainStates == null)
        {
            Debug.LogWarning("[MarketManager] No market save data found. Using defaults.");
            return;
        }

        foreach (var state in saveData.strainStates)
        {
            if (marketStateByStrain.ContainsKey(state.strainID))
            {
                marketStateByStrain[state.strainID].supplyIndex = state.supplyIndex;
                marketStateByStrain[state.strainID].demandIndex = state.demandIndex;
                marketStateByStrain[state.strainID].cachedPrice = state.cachedPrice;
                marketStateByStrain[state.strainID].cachedMultiplier = state.cachedMultiplier;
            }
        }

        Debug.Log("[MarketManager] Market loaded from save data.");
    }
}