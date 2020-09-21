using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class Step : MonoBehaviour
{
    public float ModelWidth = 1;
    public float PivotModelWidth = 1;
    public float Width = 1;
    public int Height = 5;
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

    [Button("改变组颜色")]
    private void ChangeStepGroupColorGradient()
    {
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
    public MeshRenderer PivotMeshRenderer;
    public BoxCollider BoxCollider;

    public Step PreviousStep;
    public Step NextStep;

    public Transform SpikeTransform;

    void Awake()
    {
        Initialize();
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
        float sign = 1;
        switch (StepType)
        {
            case StepType.Normal:
            case StepType.Floor:
            case StepType.Wall:
            case StepType.CheckPoint:
            case StepType.DownSpikeRoot:
            {
                sign = -1;
                break;
            }
            case StepType.Ceiling:
            case StepType.UpSpikeRoot:
            {
                sign = 1;
                break;
            }
        }

        BoxCollider.center = new Vector3(0, sign * Height / 2f, 0);
        BoxCollider.size = new Vector3(Width, Height, 1);

        PivotMeshRenderer.transform.localScale = new Vector3(PivotModelWidth, 0.1f, 1);
        float scaleX = ModelWidth - 0.05f;
        MeshRenderer.transform.localPosition = new Vector3(0, sign * Height / 2f, 0);
        MeshRenderer.transform.localScale = new Vector3(scaleX, Height, 1);

        if (SpikeTransform)
        {
            switch (StepType)
            {
                case StepType.DownSpikeRoot:
                {
                    SpikeTransform.localPosition = new Vector3(0, -Height, 0);
                    SpikeTransform.localRotation = Quaternion.Euler(0, 0, 180);
                    break;
                }
                case StepType.UpSpikeRoot:
                {
                    SpikeTransform.localPosition = new Vector3(0, Height, 0);
                    SpikeTransform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                }
            }
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