using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI dashText;
    [SerializeField] private TextMeshProUGUI knockbackText;
    [SerializeField] private TextMeshProUGUI vitalityText;

    private void OnEnable()
    {
        Refresh();
        if (CraftingSystem.Instance != null)
            CraftingSystem.Instance.OnFuseSuccess += OnFuseSuccess;
    }

    private void OnDisable()
    {
        if (CraftingSystem.Instance != null)
            CraftingSystem.Instance.OnFuseSuccess -= OnFuseSuccess;
    }

    private void OnFuseSuccess(string _) => Refresh();

    private void Refresh()
    {
        var stats = PlayerStats.Instance;
        if (stats == null) return;

        var health = stats.GetComponent<Health>();
        int displayVitality = health != null
            ? Mathf.Max(1, 1 + health.MaxHP - stats.baseMaxHP)
            : 1;

        if (speedText    != null) speedText.text    = $"Speed: {stats.DisplaySpeed}";
        if (strengthText != null) strengthText.text = $"Strength: {stats.DisplayStrength}";
        if (dashText     != null) dashText.text     = $"Dash: {stats.DisplayDash}";
        if (knockbackText!= null) knockbackText.text= $"Knockback: {stats.DisplayKnockback}";
        if (vitalityText != null) vitalityText.text = $"Vitality: {displayVitality}";
    }
}
