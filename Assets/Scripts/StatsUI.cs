using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private TextMeshProUGUI maxHpText;
    [SerializeField] private TextMeshProUGUI dashForceText;
    [SerializeField] private TextMeshProUGUI knockbackText;

    private static string Format(float value) =>
        value == Mathf.Floor(value) ? ((int)value).ToString() : $"{value:F1}";

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

        if (speedText != null)     speedText.text     = $"Speed: {Format(stats.speed)}";
        if (powerText != null)     powerText.text     = $"Power: {stats.damage}";
        if (maxHpText != null)     maxHpText.text     = $"Max HP: {(health != null ? health.MaxHP : 0)}";
        if (dashForceText != null) dashForceText.text = $"Dash Force: {Format(stats.dashForce)}";
        if (knockbackText != null) knockbackText.text = $"Knockback: {Format(stats.knockbackForce)}";
    }
}
