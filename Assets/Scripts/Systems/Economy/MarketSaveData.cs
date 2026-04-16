using System;
using System.Collections.Generic;

[Serializable]
public class MarketSaveData
{
    public List<MarketStrainState> strainStates = new List<MarketStrainState>();

    public int lastMarketUpdateDay;
}

[Serializable]
public class MarketStrainState
{
    public string strainID;

    public float supplyIndex;
    public float demandIndex;

    public float cachedPrice;
    public float cachedMultiplier;
}