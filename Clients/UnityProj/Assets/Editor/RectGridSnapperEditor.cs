using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RectGridSnapper))]
public class RectGridSnapperEditor : Editor
{
    RectGridSnapper r;

    void OnEnable()
    {
        r = target as RectGridSnapper;
        SnapToGrid(r.transform.localScale, r.transform.localPosition, true);
    }

    private void OnSceneGUI()
    {
        Vector3 center = r.transform.localPosition;
        Vector3 size = r.transform.localScale;
        Vector3[] handlesPos = new[]
        {
            new Vector3(center.x + size.x / 2, center.y, center.z),
            new Vector3(center.x - size.x / 2, center.y, center.z),
            new Vector3(center.x, center.y + size.y / 2, center.z),
            new Vector3(center.x, center.y - size.y / 2, center.z),
            new Vector3(center.x, center.y, center.z - size.z / 2),
            new Vector3(center.x, center.y, center.z + size.z / 2)
        };

        EditorGUI.BeginChangeCheck();
        Handles.color = Color.cyan;
        float rightHandle = Handles.Slider(handlesPos[0], Vector3.right, HandleUtility.GetHandleSize(handlesPos[0]) * 0.1f, Handles.CubeCap, 0.1f).x - handlesPos[0].x;
        float leftHandle = Handles.Slider(handlesPos[1], Vector3.left, HandleUtility.GetHandleSize(handlesPos[1]) * 0.1f, Handles.CubeCap, 0.1f).x - handlesPos[1].x;
        float topHandle = Handles.Slider(handlesPos[2], Vector3.up, HandleUtility.GetHandleSize(handlesPos[2]) * 0.1f, Handles.CubeCap, 0.1f).y - handlesPos[2].y;
        float bottomHandle = Handles.Slider(handlesPos[3], Vector3.down, HandleUtility.GetHandleSize(handlesPos[3]) * 0.1f, Handles.CubeCap, 0.1f).y - handlesPos[3].y;
        float frontHandle = Handles.Slider(handlesPos[4], Vector3.forward, HandleUtility.GetHandleSize(handlesPos[4]) * 0.1f, Handles.CubeCap, 0.1f).z - handlesPos[4].z;
        float backHandle = Handles.Slider(handlesPos[5], Vector3.back, HandleUtility.GetHandleSize(handlesPos[5]) * 0.1f, Handles.CubeCap, 0.1f).z - handlesPos[5].z;

        if (EditorGUI.EndChangeCheck())
        {
            Vector3 newSize = new Vector3(
                Mathf.Max(.1f, r.Bounds_GridUnit.size.x - leftHandle + rightHandle),
                Mathf.Max(.1f, r.Bounds_GridUnit.size.y + topHandle - bottomHandle),
                Mathf.Max(.1f, r.Bounds_GridUnit.size.z + backHandle - frontHandle));
            Vector3 newCenter = new Vector3(
                r.Bounds_GridUnit.center.x + leftHandle / 2f + rightHandle / 2f,
                r.Bounds_GridUnit.center.y + topHandle / 2f + bottomHandle / 2f,
                r.Bounds_GridUnit.center.z + backHandle / 2f + frontHandle / 2f);

            SnapToGrid(newSize, newCenter, true);
        }
        else
        {
            SnapToGrid(r.transform.localScale, r.transform.localPosition, true);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Refresh Snap to Grid"))
        {
            SnapToGrid(r.transform.localScale, r.transform.localPosition, true);
        }
    }

    private void SnapToGrid(Vector3 newSize, Vector3 newCenter, bool recorded)
    {
        float gridSize = r.SnapperGridSize;
        Bounds bounds = new Bounds(newCenter, newSize);
        int leftUnit = Mathf.RoundToInt(bounds.min.x / gridSize);
        int rightUnit = Mathf.RoundToInt(bounds.max.x / gridSize);
        int topUnit = Mathf.RoundToInt(bounds.max.y / gridSize);
        int bottomUnit = Mathf.RoundToInt(bounds.min.y / gridSize);
        int frontUnit = Mathf.RoundToInt(bounds.min.z / gridSize);
        int backUnit = Mathf.RoundToInt(bounds.max.z / gridSize);
        int widthUnit = Mathf.Max(1, Mathf.RoundToInt(bounds.size.x / gridSize));
        int heightUnit = Mathf.Max(1, Mathf.RoundToInt(bounds.size.y / gridSize));
        int depthUnit = Mathf.Max(1, Mathf.RoundToInt(bounds.size.z / gridSize));

        Vector3 snappedCenter = Vector3.zero;
        snappedCenter.x = widthUnit % 2 == 0 ? Mathf.RoundToInt(newCenter.x / gridSize) * gridSize : (Mathf.Sign(newCenter.x) * (Mathf.FloorToInt(Mathf.Abs(newCenter.x) / gridSize) + 0.5f)) * gridSize;
        snappedCenter.y = heightUnit % 2 == 0 ? Mathf.RoundToInt(newCenter.y / gridSize) * gridSize : (Mathf.Sign(newCenter.y) * (Mathf.FloorToInt(Mathf.Abs(newCenter.y) / gridSize) + 0.5f)) * gridSize;
        snappedCenter.z = depthUnit % 2 == 0 ? Mathf.RoundToInt(newCenter.z / gridSize) * gridSize : (Mathf.Sign(newCenter.z) * (Mathf.FloorToInt(Mathf.Abs(newCenter.z) / gridSize) + 0.5f)) * gridSize;

        r.Bounds_GridUnit = new Bounds(snappedCenter, new Vector3(widthUnit, heightUnit, depthUnit));

        if (recorded)
        {
            Undo.RecordObject(r, "BiangStudio.RectGridSnapper");
            Undo.RecordObject(r.transform, "BiangStudio.RectGridSnapper");
        }

        r.transform.localScale = r.Bounds_GridUnit.size;
        r.transform.localPosition = r.Bounds_GridUnit.center;
    }
}