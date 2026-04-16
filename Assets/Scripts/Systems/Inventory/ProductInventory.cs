using System.Collections.Generic;
using UnityEngine;

public class ProductInventory : MonoBehaviour
{
    private List<HarvestProductInstance> ownedProduct = new List<HarvestProductInstance>();

    public void AddProduct(HarvestProductInstance product)
    {
        if (product == null)
            return;

        ownedProduct.Add(product);

        Debug.Log($"[PRODUCT INVENTORY] Added: {product.DisplayName}");
    }

    public List<HarvestProductInstance> GetAllProduct()
    {
        return ownedProduct;
    }

    public int GetTotalItems()
    {
        return ownedProduct.Count;
    }

    public float GetTotalGrams()
    {
        float total = 0f;

        foreach (var product in ownedProduct)
            total += product.grams;

        return total;
    }

    public int GetEstimatedTotalValue()
    {
        int total = 0;

        foreach (var product in ownedProduct)
            total += product.TotalValue;

        return total;
    }

    public int SellAll(EconomyManager economyManager)
    {
        if (economyManager == null)
            return 0;

        int totalValue = 0;

        foreach (var product in ownedProduct)
        {
            totalValue += product.TotalValue;

            // strain tracking
            if (StrainStatsManager.Instance != null)
            {
                StrainStatsManager.Instance.RecordMoneyEarned(product.strainName, product.TotalValue);
            }
        }

        ownedProduct.Clear();

        economyManager.AddMoney(totalValue);

        Debug.Log($"[PRODUCT INVENTORY] Sold all product for ${totalValue}");

        return totalValue;
    }

    public void Clear()
    {
        ownedProduct.Clear();
    }
}