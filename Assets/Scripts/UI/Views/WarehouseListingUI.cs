using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WarehouseListingUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI warehouseNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI statusText;
    public Button actionButton;

    private WarehouseRealEstateData listingData;
    private RealEstatePanelController controller;

    public void Setup(WarehouseRealEstateData data, bool isUnlocked, bool isActive, RealEstatePanelController panelController)
    {
        listingData = data;
        controller = panelController;

        if (warehouseNameText != null)
            warehouseNameText.text = data.warehouseName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (costText != null)
            costText.text = $"${data.cost:N0}";

        if (statusText != null)
        {
            if (isActive)
                statusText.text = "ACTIVE";
            else if (isUnlocked)
                statusText.text = "OWNED";
            else
                statusText.text = "LOCKED";
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnButtonPressed);

            if (isActive)
                actionButton.interactable = false;
            else
                actionButton.interactable = true;

            TextMeshProUGUI btnText = actionButton.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                if (isUnlocked)
                    btnText.text = "SELECT";
                else
                    btnText.text = "BUY";
            }
        }
    }

    private void OnButtonPressed()
    {
        if (controller == null || listingData == null)
            return;

        controller.OnClickPurchaseOrSelect(listingData);
    }
}