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

    private Warehouse warehouse;
    private RealEstatePanelController controller;

    public void Setup(Warehouse warehouseData, bool isUnlocked, bool isActive, RealEstatePanelController panelController)
    {
        warehouse = warehouseData;
        controller = panelController;

        if (warehouse == null)
            return;

        if (warehouseNameText != null)
            warehouseNameText.text = warehouse.warehouseName;

        if (descriptionText != null)
            descriptionText.text = warehouse.description;

        if (costText != null)
            costText.text = $"${warehouse.purchaseCost:N0}";

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

            actionButton.interactable = !isActive;

            TextMeshProUGUI btnText = actionButton.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                if (isActive)
                    btnText.text = "CURRENT";
                else if (isUnlocked)
                    btnText.text = "SELECT";
                else
                    btnText.text = "BUY";
            }
        }
    }

    private void OnButtonPressed()
    {
        if (controller == null || warehouse == null)
            return;

        controller.OnClickPurchaseOrSelect(warehouse);
    }
}