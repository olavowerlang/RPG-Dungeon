using UnityEngine;

public class YSortSetup : MonoBehaviour
{
    void Awake()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = Vector3.up;
    }
}
