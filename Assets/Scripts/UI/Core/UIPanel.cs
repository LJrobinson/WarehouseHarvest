using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    public bool closeOnEscape = true;

    public bool IsOpen => gameObject.activeSelf;

    public virtual void Open()
    {
        gameObject.SetActive(true);
        OnOpened();
    }

    public virtual void Close()
    {
        OnClosed();
        gameObject.SetActive(false);
    }

    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}