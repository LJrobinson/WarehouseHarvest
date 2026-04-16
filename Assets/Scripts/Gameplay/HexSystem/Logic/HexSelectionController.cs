using UnityEngine;
using UnityEngine.InputSystem;

namespace Vertigro.Logic
{
    public class HexSelectionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera worldCamera;

        [Header("Raycast")]
        [SerializeField] private LayerMask raycastLayers = ~0;
        [SerializeField] private float maxDistance = 500f;

        [Header("Highlight")]
        [SerializeField] private Color selectedColor = Color.yellow;

        public HexNode SelectedNode { get; private set; }

        private Renderer _selectedRenderer;
        private Color _selectedOriginalColor;

        private void Update()
        {
            if (worldCamera == null)
            {
                Debug.LogWarning("HexSelectionController: No world camera assigned.");
                return;
            }

            if (Mouse.current == null)
            {
                Debug.LogWarning("HexSelectionController: No mouse detected for screen raycast.");
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = worldCamera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastLayers))
                return;

            HexNode node = hit.collider.GetComponent<HexNode>();

            if (node == null)
                node = hit.collider.GetComponentInParent<HexNode>();

            if (node == null)
                return;

            SelectNode(node);
        }

        private void SelectNode(HexNode node)
        {
            if (node == SelectedNode)
            {
                LogSelection(node);
                return;
            }

            ClearSelectionVisual();

            SelectedNode = node;
            _selectedRenderer = node.GetComponentInChildren<Renderer>();

            if (_selectedRenderer != null)
            {
                _selectedOriginalColor = _selectedRenderer.material.color;
                _selectedRenderer.material.color = selectedColor;
            }
            else
            {
                Debug.LogWarning($"HexSelectionController: No Renderer found for node {node.name}.");
            }

            LogSelection(node);
        }

        private void ClearSelectionVisual()
        {
            if (_selectedRenderer != null)
                _selectedRenderer.material.color = _selectedOriginalColor;

            SelectedNode = null;
            _selectedRenderer = null;
        }

        private static void LogSelection(HexNode node)
        {
            Debug.Log($"Selected HexNode | id:{node.name} | coords:{node.hexCoords} | floor:{node.floorLevel}");
        }
    }
}
