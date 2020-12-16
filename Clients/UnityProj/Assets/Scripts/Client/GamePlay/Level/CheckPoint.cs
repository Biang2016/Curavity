using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    internal Collider Collider;
    internal MeshRenderer MeshRenderer;
    public ProjectileType ObtainFX;

    public int CheckPointIndex = 0;

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

        if (CheckPointIndex >= LevelManager.Instance.CurrentLevel.CurrentCheckPointIndex)
        {
            LevelManager.Instance.CurrentLevel.PlayerRebornPosition = transform.position;
            LevelManager.Instance.CurrentLevel.CurrentCheckPointIndex = CheckPointIndex;
            LevelManager.Instance.CurrentLevel.TransportCheckPointIndex_Cheat = CheckPointIndex;
        }

        AudioManager.Instance.SoundPlay("sfx/Checkpoint", 0.2f);
    }
}