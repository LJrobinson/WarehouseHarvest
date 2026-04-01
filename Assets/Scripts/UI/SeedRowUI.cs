using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text seedText;
    [SerializeField] private Button plantButton;

    private SeedInstance seed;
    private DevSandboxUI ui;

    public void Setup(SeedInstance seedInstance, DevSandboxUI sandboxUI)
    {
        if (seedText == null)
            Debug.LogError($"{gameObject.name}: seedText is NOT assigned in prefab!");

        if (plantButton == null)
            Debug.LogError($"{gameObject.name}: plantButton is NOT assigned in prefab!");

        seed = seedInstance;
        ui = sandboxUI;

        seedText.text = seed.DisplayName;

        plantButton.onClick.RemoveAllListeners();
        plantButton.onClick.AddListener(OnPlantClicked);
    }

    private void OnPlantClicked()
    {
        ui.PlantSpecificSeed(seed);
    }
}