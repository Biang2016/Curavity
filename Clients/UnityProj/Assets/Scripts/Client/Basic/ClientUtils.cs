using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;


public static class ClientUtils
{
    public static GridPos3D ToGridPos3D(this Vector3 vector3)
    {
        return new GridPos3D(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y), Mathf.RoundToInt(vector3.z));
    }

    public static int AStarHeuristicsDistance(GridPos3D start, GridPos3D end)
    {
        GridPos3D diff = start - end;
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.z);
    }

    public static void InGameUIFaceToCamera(Transform transform)
    {
        Vector3 diff = transform.position - CameraManager.Instance.MainCamera.transform.position;
        Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        float distance = Vector3.Dot(ray.direction, diff);
        Vector3 cameraCenter = CameraManager.Instance.MainCamera.transform.position + ray.direction * distance;
        Vector3 offset = transform.position - cameraCenter;
        transform.rotation = Quaternion.LookRotation(transform.position - (CameraManager.Instance.MainCamera.transform.position + offset));
    }
}