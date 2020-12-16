using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class Step : MonoBehaviour
{
    public StepType StepType;

    [BoxGroup("力学特征")]
    [LabelText("弹力")]
    public float Spring = 15;

    [BoxGroup("力学特征")]
    [LabelText("质量")]
    public float Mass = 5;

    [BoxGroup("力学特征")]
    [LabelText("阻尼")]
    public float Damp = 0.2f;

    [BoxGroup("力学特征")]
    [LabelText("力传递系数")]
    public float ForcePassRatio;

    [OnValueChanged("ChangeStepGroupColorGradient")]
    public Gradient ColorGradient;

    public bool UseDefaultColor = true;

    [Button("改变组颜色")]
    private void ChangeStepGroupColorGradient()
    {
        if (UseDefaultColor) return;
        Level level = GetComponentInParent<Level>();
        if (level)
        {
            foreach (StepGroup sg in level.StepGroupList)
            {
                if (sg.StepList.Contains(this))
                {
                    sg.ColorGradient = ColorGradient;
                    break;
                }
            }

            level.RefreshStep();
        }
    }

    public SpringJoint SpringJoint;
    public Rigidbody Rigidbody;
    public MeshRenderer MeshRenderer;
    public MeshRenderer MeshRenderer_Damped;
    public MeshRenderer PivotMeshRenderer;

    public Step PreviousStep;
    public Step NextStep;

    public Transform SpikeTransform;
    public Transform CheckpointTransform;

    public float DampedDuration = 3f;
    private float DampedTick = 0f;
    private bool isDamped;

    public bool IsDamped
    {
        get { return isDamped; }
        set
        {
            if (isDamped != value)
            {
                if (value)
                {
                    Rigidbody.drag = 10f;
                    MeshRenderer.enabled = false;
                    MeshRenderer_Damped.enabled = true;
                }
                else
                {
                    Rigidbody.drag = 0f;
                    MeshRenderer.enabled = true;
                    MeshRenderer_Damped.enabled = false;
                }

                DampedTick = 0f;
                isDamped = value;
            }
            else
            {
                if (isDamped)
                {
                    DampedTick = 0;
                }
            }
        }
    }

    void Awake()
    {
        Initialize();
        MeshRenderer_Damped.enabled = false;
    }

    void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            Vector3 force = SpringJoint.currentForce;
            SpringJoint.spring = Spring;
            SpringJoint.damper = Damp;
            Rigidbody.mass = Mass;
            PreviousStep?.Rigidbody.AddForce(-force * ForcePassRatio);
            NextStep?.Rigidbody.AddForce(-force * ForcePassRatio);

            if (IsDamped)
            {
                DampedTick += Time.fixedDeltaTime;
                if (DampedTick >= DampedDuration)
                {
                    IsDamped = false;
                }
            }
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        PivotMeshRenderer.transform.localScale = transform.InverseTransformVector(new Vector3(transform.localScale.x, 0.1f, 1));
        PivotMeshRenderer.transform.localPosition = new Vector3(0, 0.5f, 0);
        MeshRenderer.transform.localScale = new Vector3((transform.localScale.x - 0.1f) / transform.localScale.x, 1, 1);
        MeshRenderer.transform.localPosition = Vector3.zero;
        MeshRenderer_Damped.transform.localScale = new Vector3((transform.localScale.x - 0.1f) / transform.localScale.x, 1, 1);
        MeshRenderer_Damped.transform.localPosition = Vector3.zero;

        if (SpikeTransform)
        {
            switch (StepType)
            {
                case StepType.DownSpikeRoot:
                {
                    SpikeTransform.localPosition = new Vector3(0, -0.5f, 0);
                    SpikeTransform.localRotation = Quaternion.Euler(0, 0, 180);
                    SpikeTransform.localScale = transform.InverseTransformVector(Vector3.one);
                    break;
                }
                case StepType.UpSpikeRoot:
                {
                    SpikeTransform.localPosition = new Vector3(0, 0.5f, 0);
                    SpikeTransform.localRotation = Quaternion.Euler(0, 0, 0);
                    SpikeTransform.localScale = transform.InverseTransformVector(Vector3.one);
                    break;
                }
            }
        }

        if (CheckpointTransform)
        {
            CheckpointTransform.localPosition = new Vector3(CheckpointTransform.localPosition.x, 0.5f, 0);
            CheckpointTransform.position += Vector3.up * 0.5f;
            CheckpointTransform.localScale = transform.InverseTransformVector(Vector3.one);
        }
    }
}

public enum StepType
{
    Normal,
    Floor,
    Ceiling,
    Wall,
    CheckPoint,
    DownSpikeRoot,
    UpSpikeRoot,
}