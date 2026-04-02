using UnityEngine;
using UnityEditor;
using System.IO;

public class StrainCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import Cannabis Strains")]
    public static void ImportStrains()
    {
        // 1. Path to your CSV file (Ensure it's in your Assets folder or use a file picker)
        string filePath = EditorUtility.OpenFilePanel("Select Strain CSV", "Assets", "csv");

        if (string.IsNullOrEmpty(filePath)) return;

        string[] lines = File.ReadAllLines(filePath);

        // 2. Loop through lines (Starting at 1 to skip headers)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] data = lines[i].Split(',');

            // Ensure we have enough data columns
            if (data.Length < 19) continue;

            // 3. Create the ScriptableObject instance
            PlantStrainData asset = ScriptableObject.CreateInstance<PlantStrainData>();

            // 4. Map CSV columns to your ScriptableObject variables
            asset.strainName = data[0];
            asset.description = data[1];
            asset.idealWaterMin = float.Parse(data[2]);
            asset.idealWaterMax = float.Parse(data[3]);
            asset.idealNutrientsMin = float.Parse(data[4]);
            asset.idealNutrientsMax = float.Parse(data[5]);
            asset.moldSusceptibility = float.Parse(data[6]);
            asset.pestSusceptibility = float.Parse(data[7]);
            asset.growthPerDay = float.Parse(data[8]);
            asset.ripenessPerDayInFlower = float.Parse(data[9]);
            asset.harvestWindowStart = float.Parse(data[10]);
            asset.harvestWindowEnd = float.Parse(data[11]);
            asset.overripeThreshold = float.Parse(data[12]);
            asset.seedCost = int.Parse(data[13]);
            asset.pack5Cost = int.Parse(data[14]);
            asset.pack20Cost = int.Parse(data[15]);
            asset.payoutMultiplier = float.Parse(data[16]);
            asset.geneticsScore = int.Parse(data[17]);
            asset.shinyChance = float.Parse(data[18]);

            // 5. Save the asset to a specific folder
            // Make sure this folder "Assets/Strains" exists first!
            string folderPath = "Assets/Strains";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Strains");
            }

            // Clean the name so Windows doesn't get mad at weird characters
            string fileName = data[0].Replace(" ", "_").Replace("/", "_");
            AssetDatabase.CreateAsset(asset, $"{folderPath}/{fileName}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "1000 Strains Imported!", "Dope");
    }
}