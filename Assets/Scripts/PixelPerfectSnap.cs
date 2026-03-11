using Cinemachine;
using UnityEngine;

/// Rounds the Main Camera position to pixel boundaries after Cinemachine moves it.
[RequireComponent(typeof(Camera))]
public class PixelPerfectSnap : MonoBehaviour
{
    [SerializeField] private int pixelsPerUnit = 16;

    private CinemachineBrain _brain;

    private void Awake()
    {
        _brain = GetComponent<CinemachineBrain>();
    }

    private void LateUpdate()
    {
        if (_brain != null && _brain.ActiveVirtualCamera == null) return;

        float ppu = pixelsPerUnit;
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x * ppu) / ppu;
        pos.y = Mathf.Round(pos.y * ppu) / ppu;
        transform.position = pos;
    }
}
