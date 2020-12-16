using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class Player : MonoBehaviour
{
    public bool IsGrounded = false;

    public MeshRenderer MeshRenderer;
    public SphereCollider SphereCollider;
    public Rigidbody Rigidbody;
    public GameObject Model;

    [LabelText("起跳速度增幅")]
    public float JumpVelocityFactor;

    [LabelText("起跳速度增量")]
    public float JumpVelocityAdd;

    [LabelText("下落力量")]
    public float DiveForce;

    [LabelText("左右移动力量")]
    public float MoveForce;

    [LabelText("最大左右移动速度")]
    public float MaxSideModeSpeed;

    [LabelText("最大跳起速度")]
    public float MaxYSpeed;

    [LabelText("球半径")]
    public float BallRadius;

    [LabelText("制动半径")]
    public float DamperRadius;

    public ProjectileType DieFX;
    public ProjectileType DieExplodeFX;
    public ProjectileType JumpFX;
    public ProjectileType DiveFX;
    public ProjectileType DiveHitFX;
    public ProjectileType ScoreFX;
    public ProjectileType DamperFX;

    public float DiveCollideFXVelocityThreshold = 10f;

    void Start()
    {
        Show();
        IsGrounded = false;
        CameraManager.Instance.FieldCamera.InitFocus();
    }

    public void Hide()
    {
        MeshRenderer.enabled = false;
        SphereCollider.enabled = false;
        CameraManager.Instance.FieldCamera.RemoveTargetActor(Rigidbody);
    }

    public void Show()
    {
        MeshRenderer.enabled = true;
        SphereCollider.enabled = true;
        CameraManager.Instance.FieldCamera.AddTargetActor(Rigidbody);
    }

    public void ResetToPosition(Vector3 position)
    {
        Show();
        transform.position = position;
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        IsGrounded = false;
    }

    void Update()
    {
        SphereCollider.radius = BallRadius;
        Model.transform.localScale = Vector3.one * BallRadius * 2;

        Collider[] colliders = Physics.OverlapSphere(transform.position - Vector3.up * (BallRadius / 2f + 0.05f), BallRadius / 2f, LayerManager.Instance.LayerMask_Step);
        IsGrounded = colliders.Length != 0;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrounded)
            {
                if (Rigidbody.velocity.y <= 0)
                {
                    Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, JumpVelocityAdd, Rigidbody.velocity.z);
                }
                else
                {
                    float velY = Mathf.Max(Rigidbody.velocity.y * JumpVelocityFactor, Rigidbody.velocity.y + JumpVelocityAdd);
                    Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, velY, Rigidbody.velocity.z);
                }

                ProjectileManager.Instance.PlayProjectileFlash(JumpFX, transform.position);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (!IsGrounded)
            {
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, 0, Rigidbody.velocity.z);
                Rigidbody.AddForce(Vector3.down * DiveForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
                ProjectileManager.Instance.PlayProjectileFlash(DiveFX, transform.position);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            AudioManager.Instance.SoundPlay("sfx/FreezeSkill", 0.2f);
            AudioManager.Instance.SoundPlay("sfx/Freeze", 0.2f);
            ProjectileHit hit = ProjectileManager.Instance.PlayProjectileHit(DamperFX, transform.position);
            hit.transform.localScale = Vector3.one * 3f;
            hit.transform.parent = transform;

            Collider[] stepColliders = Physics.OverlapSphere(transform.position, DamperRadius, LayerManager.Instance.LayerMask_Step);
            foreach (Collider sc in stepColliders)
            {
                Step step = sc.GetComponentInParent<Step>();
                step.IsDamped = true;
            }
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (LevelManager.Instance.CurrentLevel.TransportCheckPointIndex_Cheat + 1 < LevelManager.Instance.CurrentLevel.CheckPoints.Count)
                {
                    LevelManager.Instance.CurrentLevel.TransportCheckPointIndex_Cheat++;
                    CheckPoint cp = LevelManager.Instance.CurrentLevel.CheckPoints[LevelManager.Instance.CurrentLevel.TransportCheckPointIndex_Cheat];
                    LevelManager.Instance.Player.ResetToPosition(cp.transform.position + Vector3.up * 2f);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (LevelManager.Instance.CurrentLevel.TransportCheckPointIndex_Cheat - 1 >= 0)
                {
                    LevelManager.Instance.CurrentLevel.TransportCheckPointIndex_Cheat--;
                    CheckPoint cp = LevelManager.Instance.CurrentLevel.CheckPoints[LevelManager.Instance.CurrentLevel.TransportCheckPointIndex_Cheat];
                    LevelManager.Instance.Player.ResetToPosition(cp.transform.position + Vector3.up * 2f);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.D))
        {
            Rigidbody.AddForce(Vector3.right * MoveForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        if (Input.GetKey(KeyCode.A))
        {
            Rigidbody.AddForce(-Vector3.right * MoveForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        float sideMoveMaxSpeed = Mathf.Clamp(MaxSideModeSpeed * Mathf.Abs(Rigidbody.velocity.y) / 5f, MaxSideModeSpeed, MaxSideModeSpeed * 1.5f);
        Rigidbody.velocity = new Vector3(
            Mathf.Clamp(Rigidbody.velocity.x, -sideMoveMaxSpeed, sideMoveMaxSpeed),
            Mathf.Clamp(Rigidbody.velocity.y, float.MinValue, MaxYSpeed),
            Mathf.Clamp(Rigidbody.velocity.z, -sideMoveMaxSpeed, sideMoveMaxSpeed));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerManager.Instance.Layer_Spike)
        {
            ProjectileManager.Instance.PlayProjectileHit(DieFX, collision.GetContact(0).point);
            Die();
        }
        else if (collision.collider.gameObject.layer == LayerManager.Instance.Layer_Step && collision.relativeVelocity.magnitude > DiveCollideFXVelocityThreshold)
        {
            ProjectileManager.Instance.PlayProjectileHit(DiveHitFX, collision.GetContact(0).point);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_CheckPoint)
        {
            CheckPoint checkPoint = collider.gameObject.GetComponentInParent<CheckPoint>();
            if (checkPoint)
            {
                checkPoint.OnPlayerEnter();
            }
        }
        else if (collider.gameObject.layer == LayerManager.Instance.Layer_DeadZone)
        {
            ProjectileManager.Instance.PlayProjectileHit(DieFX, transform.position);
            Die();
        }
        else if (collider.gameObject.layer == LayerManager.Instance.Layer_Star)
        {
            Star star = collider.gameObject.GetComponentInParent<Star>();
            if (star)
            {
                star.OnScore();
                ProjectileManager.Instance.PlayProjectileHit(ScoreFX, transform.position);
            }
        }
    }

    void Die()
    {
        AudioManager.Instance.SoundPlay("sfx/DeadPong", 0.35f);
        LevelManager.Instance.Die(
            () =>
            {
                AudioManager.Instance.SoundPlay("sfx/Dead", 0.35f);
                ProjectileManager.Instance.PlayProjectileHit(DieExplodeFX, transform.position);
                Hide();
            }
        );
    }
}