using UnityEngine;

namespace BiangLibrary.GridBasedRectSnapper
{
    public class RectGridSnapper : MonoBehaviour
    {
        public Vector3 SnapperGridSize = Vector3.one;

        public Bounds Bounds_GridUnit_ReadOnly;

        void Reset()
        {
            Bounds_GridUnit_ReadOnly.center = transform.localPosition;
            Bounds_GridUnit_ReadOnly.size = transform.localScale;
        }
    }
}