using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StrainDatabase", menuName = "WarehouseHarvest/Strain Database")]
public class StrainDatabase : ScriptableObject
{
    public List<PlantStrainData> strains = new List<PlantStrainData>();
}