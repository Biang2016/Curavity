using UnityEngine;

[ExecuteInEditMode]
public class RectGridSnapper : MonoBehaviour
{
    public float SnapperGridSize = 1;

    public Bounds Bounds_GridUnit;

    void Reset()
    {
        Bounds_GridUnit.center = transform.localPosition;
        Bounds_GridUnit.size = transform.localScale;
    }
}