using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject collectionPanel;
    public Transform scrollContent;
    public GameObject buttonPrefab;
    public TMP_InputField searchInput;
    public DiscoveryManager discoveryManager;

    [Header("UI Text")]
    public TMP_Text titleText;
    public TMP_Text detailText;

    //private List<PlantStrainData> discoveredStrains = new List<PlantStrainData>();

    private void Start()
    {
        Debug.Log("CollectionUIController STARTED");
        if (searchInput != null)
            searchInput.onValueChanged.AddListener(OnSearchChanged);

        RefreshList();
    }

    private void Awake()
    {
        if (discoveryManager == null)
            discoveryManager = DiscoveryManager.Instance;
    }

    /// <summary>
    /// Call this from Harvest or anywhere a new strain is discovered
    /// </summary>
    //public void DiscoverStrain(PlantStrainData strain)
    //{
    //    if (strain == null || discoveredStrains.Contains(strain))
    //        return;

    //    discoveredStrains.Add(strain);
    //    RefreshList(); // immediately update UI
    //}

    /// <summary>
    /// Rebuilds the scroll list based on discovered strains and search
    /// </summary>
    public void RefreshList()
    {
        Debug.Log("CollectionUIController.RefreshList() CALLED");
        Debug.Log("=== REFRESH LIST START ===");

        Debug.Log("scrollContent = " + scrollContent);
        Debug.Log("buttonPrefab = " + buttonPrefab);
        Debug.Log("discoveryManager = " + discoveryManager);

        if (discoveryManager != null)
        {
            Debug.Log("Discovered Count = " + discoveryManager.GetDiscoveredCount());
            Debug.Log("Total Strains = " + discoveryManager.GetTotalCount());
        }

        Debug.Log("=== REFRESH LIST END PRECHECK ===");

        if (discoveryManager == null)
        {
            Debug.LogError("CollectionUIController: discoveryManager not assigned!");
            return;
        }

        if (scrollContent == null || buttonPrefab == null)
        {
            Debug.LogError("CollectionUIController: scrollContent or buttonPrefab missing!");
            return;
        }

        int discoveredCount = discoveryManager.GetDiscoveredCount();
        int totalCount = discoveryManager.GetTotalCount();

        if (titleText != null)
            titleText.text = $"Discovered {discoveredCount}/{totalCount}";

        foreach (Transform child in scrollContent)
            Destroy(child.gameObject);

        string filter = searchInput != null ? searchInput.text.ToLower() : "";

        List<PlantStrainData> discovered = discoveryManager.GetDiscoveredStrains();
        Debug.Log("GetDiscoveredStrains() returned: " + discovered.Count);

        foreach (var strain in discovered)
        {
            if (strain == null) continue;

            if (!string.IsNullOrEmpty(filter) && !strain.strainName.ToLower().Contains(filter))
                continue;

            GameObject buttonGO = Instantiate(buttonPrefab, scrollContent);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = strain.strainName;

            Button btn = buttonGO.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnStrainButtonClicked(strain));
            }
        }

        Debug.Log($"Collection list rebuilt. Discovered: {discovered.Count}");
    }

    private void OnSearchChanged(string newText)
    {
        RefreshList();
    }

    private void OnStrainButtonClicked(PlantStrainData strain)
    {
        if (detailText != null)
        {
            detailText.text =
                $"STRAIN INFO\n" +
                $"----------------\n" +
                $"Name: {strain.strainName}\n\n" +
                $"{strain.description}\n\n" +
                $"Genetics: {strain.geneticsScore}\n" +
                $"Growth/Day: {strain.growthPerDay}\n" +
                $"Shiny Chance: {(strain.shinyChance * 100f):0.00}%";
        }
    }

    public void ToggleCollectionPanel()
    {
        collectionPanel.SetActive(!collectionPanel.activeSelf);
        RefreshList();
    }
}