using UnityEngine;
using TMPro;

using Vertigro.Logic;
public class HexPanelController : UIPanel
{
    
    public HexSelectionController selection;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI plantText;
    public TextMeshProUGUI insertText;
    public TextMeshProUGUI stateText;

    private void Update()
    {
        if (selection == null || stateText == null)
            return;

        var node = selection.SelectedNode;

        if (node == null)
        {
            stateText.text = "No Hex Selected";
            return;
        }

        stateText.text = node.GetDebugState();
    }
}