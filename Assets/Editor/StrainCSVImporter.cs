using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class StrainCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import Cannabis Strains")]
    public static void ImportStrains()
    {
        string filePath = EditorUtility.OpenFilePanel("Select Strain CSV", "Assets", "csv");
        if (string.IsNullOrEmpty(filePath)) return;

        string[] lines;

        // Open with ReadWrite sharing to prevent the IOException if Excel is open
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs))
        {
            string content = sr.ReadToEnd();
            lines = content.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        string folderPath = "Assets/Strains";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Strains");
        }

        // Start a batch to stop Unity from refreshing 1000 times (Performance Boost)
        AssetDatabase.StartAssetEditing();

        try
        {
            for (int i = 1; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(',');
                if (data.Length < 19) continue;

                PlantStrainData asset = ScriptableObject.CreateInstance<PlantStrainData>();

                // 1. Data Mapping
                asset.strainName = data[0]; // The "Pretty Name" remains unchanged
                asset.description = data[1];
                asset.idealWaterMin = ParseFloat(data[2]);
                asset.idealWaterMax = ParseFloat(data[3]);
                asset.idealNutrientsMin = ParseFloat(data[4]);
                asset.idealNutrientsMax = ParseFloat(data[5]);
                asset.moldSusceptibility = ParseFloat(data[6]);
                asset.pestSusceptibility = ParseFloat(data[7]);
                asset.growthPerDay = ParseFloat(data[8]);
                asset.ripenessPerDayInFlower = ParseFloat(data[9]);
                asset.harvestWindowStart = ParseFloat(data[10]);
                asset.harvestWindowEnd = ParseFloat(data[11]);
                asset.overripeThreshold = ParseFloat(data[12]);
                asset.seedCost = ParseInt(data[13]);
                asset.pack5Cost = ParseInt(data[14]);
                asset.pack20Cost = ParseInt(data[15]);
                asset.payoutMultiplier = ParseFloat(data[16]);
                asset.geneticsScore = ParseInt(data[17]);
                asset.shinyChance = ParseFloat(data[18]);

                // 2. Clean Filename: Remove all non-alphanumeric characters and spaces
                // Example: "AK-47" -> "AK47", "Elphaba's Bliss" -> "ElphabasBliss"
                string cleanFileName = Regex.Replace(data[0], @"[^a-zA-Z0-9]", "");

                // 3. Save Asset
                string fullPath = $"{folderPath}/{cleanFileName}.asset";
                AssetDatabase.CreateAsset(asset, fullPath);
            }
        }
        finally
        {
            // End the batch so Unity imports everything at once
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Clean Import Complete!", "Dope");
    }

    // Helper methods to prevent crashing on empty cells
    private static float ParseFloat(string value) => float.TryParse(value, out float result) ? result : 0f;
    private static int ParseInt(string value) => int.TryParse(value, out int result) ? result : 0;
}