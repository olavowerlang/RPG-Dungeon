using UnityEngine;
using UnityEngine.UI;

public class DashStaminaUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject visualRoot; // the actual bar to show/hide
    [SerializeField] private float widthPerStaminaUnit = 30f;

    private PlayerController _controller;
    private PlayerStats _stats;
    private RectTransform _visualRect;

    private void Start()
    {
        _stats = PlayerStats.Instance;
        if (_stats != null)
        {
            _controller = _stats.GetComponent<PlayerController>();
            _visualRect = visualRoot != null ? visualRoot.GetComponent<RectTransform>() : null;
            UpdateBarWidth();
        }

        if (visualRoot != null) visualRoot.SetActive(false);
    }

    private void Update()
    {
        if (_stats == null) return;

        bool shouldShow = _stats.hasDash;
        if (visualRoot != null && visualRoot.activeSelf != shouldShow)
            visualRoot.SetActive(shouldShow);

        if (!shouldShow) return;

        UpdateBarWidth();

        if (slider != null && _controller != null)
            slider.value = _controller.CurrentDashStamina / _stats.maxDashStamina;
    }

    private void UpdateBarWidth()
    {
        if (_visualRect == null)
        {
            Debug.LogWarning("DashStaminaUI: _visualRect is null");
            return;
        }
        float newWidth = _stats.maxDashStamina * widthPerStaminaUnit;
        Debug.Log($"DashStaminaUI: setting width to {newWidth} (maxStamina={_stats.maxDashStamina})");
        _visualRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }
}
