using UnityEngine;
using UnityEditor;

namespace BiangLibrary.GridBasedRectSnapper
{
    /// <summary>
    /// This editor script of the RectGridSnapper contains the main logic of this tool.
    /// </summary>
    [CanEditMultipleObjects, CustomEditor(typeof(RectGridSnapper))]
    public class RectGridSnapperEditor : Editor
    {
        /// <summary>
        /// if the number of selected gameObjects is larger than this number, the handles won't be displayed, but moving and scaling still work.
        /// </summary>
        private const int MAXIMUM_DRAWN_HANDLES = 10; 

        /// <summary>
        /// This is the size of each cylindrical handle.
        /// </summary>
        private const float CAP_SIZE = 0.14f;

        RectGridSnapper my_RectGridSnapper;

        void OnEnable()
        {
            my_RectGridSnapper = target as RectGridSnapper;
            SnapToGrid(my_RectGridSnapper, my_RectGridSnapper.transform.localScale, my_RectGridSnapper.transform.localPosition, true);
            Axis_Colors = new[] {new Color(1, 0.4f, 0.4f), new Color(0.4f, 1, 0.4f), new Color(0.4f, 0.4f, 1),}; // XYZ axes colors for handles
        }

        private Color[] Axis_Colors;

        private void OnSceneGUI()
        {
            bool drawHandles = Selection.gameObjects.Length < MAXIMUM_DRAWN_HANDLES;
            foreach (GameObject targetGO in Selection.gameObjects)
            {
                RectGridSnapper r = targetGO.GetComponent<RectGridSnapper>();
                if (r != null)
                {
                    if (drawHandles)
                    {
                        // calculate the handles' positions
                        Vector3 center = r.transform.localPosition;
                        Vector3 size = r.transform.localScale;
                        Vector3[] handlesLocalPos = new[]
                        {
                            new Vector3(center.x - size.x / 2, center.y, center.z), // left
                            new Vector3(center.x + size.x / 2, center.y, center.z), // right
                            new Vector3(center.x, center.y - size.y / 2, center.z), // down
                            new Vector3(center.x, center.y + size.y / 2, center.z), // up
                            new Vector3(center.x, center.y, center.z - size.z / 2), // back
                            new Vector3(center.x, center.y, center.z + size.z / 2) // front
                        };
                        Vector3[] handlesWorldPos = new Vector3[6];

                        for (int i = 0; i < 6; i++)
                        {
                            Vector3 localPos = handlesLocalPos[i];
                            if (r.transform.parent != null)
                            {
                                handlesWorldPos[i] = r.transform.parent.TransformPoint(localPos);
                            }
                            else
                            {
                                handlesWorldPos[i] = r.transform.localRotation * (localPos - r.transform.position) + r.transform.position;
                            }
                        }

                        EditorGUI.BeginChangeCheck();

                        // draw the 6 handles
                        float[] handleMovements = new float[6];
                        for (int dimension = 0; dimension < 3; dimension++)
                        {
                            Handles.color = Axis_Colors[dimension];
                            for (int sign = 0; sign < 2; sign++)
                            {
                                Vector3 direction = Vector3.zero;
                                direction[dimension] = sign == 0 ? -1 : 1;
                                int index = dimension * 2 + sign;
                                Vector3 handleWorldPos = handlesWorldPos[index];
                                Vector3 dirHandle = Handles.Slider(handleWorldPos, r.transform.rotation * direction, HandleUtility.GetHandleSize(handleWorldPos) * CAP_SIZE, Handles.CylinderHandleCap, 0.1f) - handleWorldPos;
                                handleMovements[index] = (Quaternion.Inverse(r.transform.rotation) * dirHandle)[dimension] / (r.transform.lossyScale[dimension] / r.transform.localScale[dimension]);
                            }
                        }

                        if (EditorGUI.EndChangeCheck()) // if user dragged the handles, calculate the new size and position for this gameObject
                        {
                            Vector3 newSize = new Vector3(
                                Mathf.Max(.1f, r.Bounds_GridUnit_ReadOnly.size.x * r.SnapperGridSize.x - handleMovements[0] + handleMovements[1]),
                                Mathf.Max(.1f, r.Bounds_GridUnit_ReadOnly.size.y * r.SnapperGridSize.y - handleMovements[2] + handleMovements[3]),
                                Mathf.Max(.1f, r.Bounds_GridUnit_ReadOnly.size.z * r.SnapperGridSize.z - handleMovements[4] + handleMovements[5]));
                            Vector3 newCenter = new Vector3(
                                r.Bounds_GridUnit_ReadOnly.center.x + handleMovements[0] / 2f + handleMovements[1] / 2f,
                                r.Bounds_GridUnit_ReadOnly.center.y + handleMovements[2] / 2f + handleMovements[3] / 2f,
                                r.Bounds_GridUnit_ReadOnly.center.z + handleMovements[4] / 2f + handleMovements[5] / 2f);

                            SnapToGrid(r, newSize, newCenter, true);
                        }
                        else
                        {
                            SnapToGrid(r, r.transform.localScale, r.transform.localPosition, true);
                        }
                    }
                    else
                    {
                        SnapToGrid(r, r.transform.localScale, r.transform.localPosition, true);
                    }
                }
            }
        }

        /// <summary>
        /// This is the core function for "Grid-based" objects.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="newSize"></param>
        /// <param name="newCenter"></param>
        /// <param name="recorded"></param>
        private void SnapToGrid(RectGridSnapper r, Vector3 newSize, Vector3 newCenter, bool recorded)
        {
            Bounds bounds = new Bounds(newCenter, newSize);
            int widthUnit = Mathf.Max(1, Mathf.RoundToInt(bounds.size.x / r.SnapperGridSize.x));
            int heightUnit = Mathf.Max(1, Mathf.RoundToInt(bounds.size.y / r.SnapperGridSize.y));
            int depthUnit = Mathf.Max(1, Mathf.RoundToInt(bounds.size.z / r.SnapperGridSize.z));

            Vector3 snappedCenter = Vector3.zero;
            snappedCenter.x = widthUnit % 2 == 0 ? Mathf.RoundToInt(newCenter.x / r.SnapperGridSize.x) * r.SnapperGridSize.x : (Mathf.Sign(newCenter.x) * (Mathf.FloorToInt(Mathf.Abs(newCenter.x) / r.SnapperGridSize.x) + 0.5f)) * r.SnapperGridSize.x;
            snappedCenter.y = heightUnit % 2 == 0 ? Mathf.RoundToInt(newCenter.y / r.SnapperGridSize.y) * r.SnapperGridSize.y : (Mathf.Sign(newCenter.y) * (Mathf.FloorToInt(Mathf.Abs(newCenter.y) / r.SnapperGridSize.y) + 0.5f)) * r.SnapperGridSize.y;
            snappedCenter.z = depthUnit % 2 == 0 ? Mathf.RoundToInt(newCenter.z / r.SnapperGridSize.z) * r.SnapperGridSize.z : (Mathf.Sign(newCenter.z) * (Mathf.FloorToInt(Mathf.Abs(newCenter.z) / r.SnapperGridSize.z) + 0.5f)) * r.SnapperGridSize.z;

            r.Bounds_GridUnit_ReadOnly = new Bounds(snappedCenter, new Vector3(widthUnit, heightUnit, depthUnit));

            if (recorded)
            {
                Undo.RecordObject(r, "BiangLibrary.RectGridSnapper");
                Undo.RecordObject(r.transform, "BiangLibrary.RectGridSnapper");
            }

            r.transform.localScale = Vector3.Scale(r.Bounds_GridUnit_ReadOnly.size, r.SnapperGridSize);
            r.transform.localPosition = r.Bounds_GridUnit_ReadOnly.center;
        }
    }
}