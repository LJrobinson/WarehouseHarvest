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
        private HexNode hoveredNode;

        private Renderer _selectedRenderer;
        private Color _selectedOriginalColor;

        private void Update()
        {
            
            if (worldCamera == null)
            {
                Debug.LogWarning("HexSelectionController: No world camera assigned.");
                return;
            }

            HandleHover();
            HandleMouseSelection();
            HandleNodeAction();
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

        private void DeselectNode()
        {
            ClearSelectionVisual();
            if (UIManager.Instance != null)
                UIManager.Instance.CloseCurrentPanel();
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

        private void HandleMouseSelection()
        {
            if (Mouse.current == null)
                return;

            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = worldCamera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastLayers))
            {
                DeselectNode();
                return;
            }

            HexNode node = hit.collider.GetComponentInParent<HexNode>();

            if (node == null)
            {
                DeselectNode();
                return;
            }

            SelectNode(node);
        }

        private void HandleNodeAction()
        {
            if (SelectedNode == null)
                return;

            if (Keyboard.current == null)
                return;

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("E pressed on node: " + SelectedNode.name);
            }
        }

        private void HandleHover()
        {
            if (Mouse.current == null)
                return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = worldCamera.ScreenPointToRay(mousePos);

            HexNode newHovered = null;

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastLayers))
            {
                newHovered = hit.collider.GetComponentInParent<HexNode>();
            }

            hoveredNode = newHovered;
        }
    }
}
