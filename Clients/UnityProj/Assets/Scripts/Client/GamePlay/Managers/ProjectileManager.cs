using System.Collections.Generic;
using BiangStudio.ObjectPool;
using BiangStudio.Singleton;
using UnityEngine;

public class ProjectileManager : TSingletonBaseManager<ProjectileManager>
{
    public Transform Root;

    public void Init(Transform root)
    {
        Root = root;
    }

    public void EmitProjectile(Transform shooterDummy, ProjectileType projectileType, float velocity, float projectileScale)
    {
        ShootProjectile(shooterDummy.position, shooterDummy.forward, shooterDummy, projectileType, velocity, projectileScale);
    }

    public Projectile ShootProjectile(Vector3 from, Vector3 dir, Transform dummyPos, ProjectileType projectileType, float velocity, float projectileScale)
    {
        Projectile projectile = GameObjectPoolManager.Instance.ProjectileDict[projectileType].AllocateGameObject<Projectile>(Root);
        projectile.transform.position = from;
        projectile.transform.LookAt(from + dir);
        projectile.Initialize(velocity, dummyPos.forward, projectileType, projectileScale);
        projectile.Launch(dummyPos);
        return projectile;
    }

    public ProjectileHit PlayProjectileHit(ProjectileType projectType, Vector3 position)
    {
        if (GameObjectPoolManager.Instance.ProjectileHitDict.ContainsKey(projectType))
        {
            ProjectileHit hit = GameObjectPoolManager.Instance.ProjectileHitDict[projectType].AllocateGameObject<ProjectileHit>(Root);
            hit.transform.position = position;
            hit.transform.localScale = Vector3.one;
            hit.transform.rotation = Quaternion.identity;
            hit.Play();
            return hit;
        }

        return null;
    }

    public ProjectileFlash PlayProjectileFlash(ProjectileType projectType, Vector3 position)
    {
        if (GameObjectPoolManager.Instance.ProjectileFlashDict.ContainsKey(projectType))
        {
            ProjectileFlash flash = GameObjectPoolManager.Instance.ProjectileFlashDict[projectType].AllocateGameObject<ProjectileFlash>(Root);
            flash.transform.position = position;
            flash.transform.localScale = Vector3.one;
            flash.transform.rotation = Quaternion.identity;
            flash.Play();
            return flash;
        }

        return null;
    }
}