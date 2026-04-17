using UnityEngine;
using UnityEngine.InputSystem;
using Vertigro.Data;

namespace Vertigro.Logic
{
    public class HexSelectionController : MonoBehaviour
    {
        public enum PlacementMode
        {
            None,
            Insert,
            Plant
        }

        [Header("References")]
        [SerializeField] private Camera worldCamera;

        [Header("Raycast")]
        [SerializeField] private LayerMask raycastLayers = ~0;
        [SerializeField] private float maxDistance = 500f;

        [Header("Highlight")]
        [SerializeField] private Color selectedColor = Color.yellow;

        [Header("Placement")]
        [SerializeField] private PlacementMode placementMode = PlacementMode.None;
        [SerializeField] private InsertData selectedInsert;
        [SerializeField] private GameObject selectedPlantPrefab;
        [SerializeField] private SeedInventory seedInventory;
        [SerializeField] private bool placeOnClick = true;

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

            if (placeOnClick && placementMode != PlacementMode.None)
                TryPlaceOnNode(node);
        }

        private void HandleNodeAction()
        {
            if (SelectedNode == null)
                return;

            if (Keyboard.current == null)
                return;

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SelectedNode.Interact();
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

        public void SelectInsertPlacement()
        {
            placementMode = PlacementMode.Insert;
        }

        public void SelectPlantPlacement()
        {
            placementMode = PlacementMode.Plant;
        }

        public void ClearPlacement()
        {
            placementMode = PlacementMode.None;
        }

        public void SetSelectedInsert(InsertData insert)
        {
            selectedInsert = insert;
            placementMode = PlacementMode.Insert;
        }

        public void SetSelectedPlantPrefab(GameObject plantPrefab)
        {
            selectedPlantPrefab = plantPrefab;
            placementMode = PlacementMode.Plant;
        }

        public void PlaceSelectedOnSelectedNode()
        {
            TryPlaceOnNode(SelectedNode);
        }

        public void PlaceSelectedOnHoveredNode()
        {
            TryPlaceOnNode(hoveredNode);
        }

        private bool TryPlaceOnNode(HexNode node)
        {
            if (node == null)
                return false;

            switch (placementMode)
            {
                case PlacementMode.Insert:
                    return TryPlaceInsert(node);

                case PlacementMode.Plant:
                    return TryPlacePlant(node);

                default:
                    return false;
            }
        }

        private bool TryPlaceInsert(HexNode node)
        {
            if (selectedInsert == null)
            {
                Debug.LogWarning("HexSelectionController: No insert selected for placement.");
                return false;
            }

            if (!node.TryPlaceInsert(selectedInsert))
            {
                Debug.Log("Cannot place insert: hex is not empty.");
                return false;
            }

            Debug.Log($"Placed insert '{selectedInsert.insertName}' on {node.name}.");
            return true;
        }

        private bool TryPlacePlant(HexNode node)
        {
            if (selectedPlantPrefab == null)
            {
                Debug.LogWarning("HexSelectionController: No plant prefab selected for placement.");
                return false;
            }

            SeedInstance seed = GetFirstInventorySeed();
            bool placed = seed != null
                ? node.TryPlantSeed(seed, selectedPlantPrefab)
                : node.TryPlacePlant(selectedPlantPrefab);

            if (!placed)
            {
                Debug.Log("Cannot place plant: hex is not empty or plant prefab is invalid.");
                return false;
            }

            if (seed != null && seedInventory != null)
                seedInventory.RemoveSpecificSeed(seed);

            Debug.Log($"Placed plant on {node.name}.");
            return true;
        }

        private SeedInstance GetFirstInventorySeed()
        {
            if (seedInventory == null)
                return null;

            var seeds = seedInventory.GetAllSeeds();

            if (seeds == null || seeds.Count == 0)
                return null;

            return seeds[0];
        }
    }
}
