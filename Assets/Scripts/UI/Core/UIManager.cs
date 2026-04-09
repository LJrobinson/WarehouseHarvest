using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private List<UIPanel> panels = new List<UIPanel>();

    private Dictionary<string, UIPanel> panelLookup = new Dictionary<string, UIPanel>();
    private UIPanel currentPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildPanelLookup();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentPanel != null && currentPanel.closeOnEscape)
            {
                CloseCurrentPanel();
            }
        }
    }

    private void BuildPanelLookup()
    {
        panelLookup.Clear();

        foreach (var panel in panels)
        {
            if (panel == null) continue;

            string key = panel.name;

            if (!panelLookup.ContainsKey(key))
            {
                panelLookup.Add(key, panel);
            }
            else
            {
                Debug.LogWarning($"UIManager: Duplicate panel name found: {key}");
            }
        }
    }

    public void OpenPanel(string panelName)
    {
        if (!panelLookup.ContainsKey(panelName))
        {
            Debug.LogWarning($"UIManager: No panel found with name: {panelName}");
            return;
        }

        UIPanel panelToOpen = panelLookup[panelName];

        if (currentPanel != null && currentPanel != panelToOpen)
        {
            currentPanel.Close();
        }

        panelToOpen.Open();
        currentPanel = panelToOpen;
    }

    public void TogglePanel(string panelName)
    {
        if (!panelLookup.ContainsKey(panelName))
        {
            Debug.LogWarning($"UIManager: No panel found with name: {panelName}");
            return;
        }

        UIPanel panel = panelLookup[panelName];

        if (panel.IsOpen)
        {
            panel.Close();
            if (currentPanel == panel)
                currentPanel = null;
        }
        else
        {
            OpenPanel(panelName);
        }
    }

    public void CloseCurrentPanel()
    {
        if (currentPanel == null)
            return;

        currentPanel.Close();
        currentPanel = null;
    }

    public void CloseAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel != null && panel.IsOpen)
                panel.Close();
        }

        currentPanel = null;
    }

    public UIPanel GetCurrentPanel()
    {
        return currentPanel;
    }
}