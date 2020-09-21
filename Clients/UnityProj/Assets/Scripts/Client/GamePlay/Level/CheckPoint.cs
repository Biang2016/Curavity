using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    internal Collider Collider;
    internal MeshRenderer MeshRenderer;
    public ProjectileType ObtainFX;

    void Awake()
    {
        Collider = GetComponentInChildren<Collider>();
        MeshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void OnPlayerEnter()
    {
        Collider.enabled = false;
        MeshRenderer.enabled = false;
        ProjectileManager.Instance.PlayProjectileFlash(ObtainFX, Collider.transform.position);
        LevelManager.Instance.CurrentLevel.PlayerRebornPosition = transform.position;
    }
}