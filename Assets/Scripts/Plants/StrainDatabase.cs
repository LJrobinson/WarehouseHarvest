using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StrainDatabase", menuName = "WarehouseHarvest/Strain Database")]
public class StrainDatabase : ScriptableObject
{
    public List<PlantStrainData> strains = new List<PlantStrainData>();

    [ContextMenu("Refresh Database")]
    public void RefreshDatabase()
    {
#if UNITY_EDITOR
        strains.Clear();

        string[] guids = AssetDatabase.FindAssets("t:PlantStrainData");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PlantStrainData strain = AssetDatabase.LoadAssetAtPath<PlantStrainData>(path);

            if (strain != null)
                strains.Add(strain);
        }

        strains = strains.OrderBy(s => s.strainName).ToList();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log($"Database Refreshed: {strains.Count} strains loaded.");
#else
        Debug.LogWarning("RefreshDatabase only works in the Unity Editor.");
#endif
    }

    public PlantStrainData GetRandomStrain()
    {
        if (strains == null || strains.Count == 0)
        {
            Debug.LogError("StrainDatabase has no strains!");
            return null;
        }

        return strains[Random.Range(0, strains.Count)];
    }

    public PlantStrainData GetStrainByName(string name)
    {
        foreach (var s in strains)
        {
            if (s != null && s.strainName == name)
                return s;
        }
        return null;
    }
}