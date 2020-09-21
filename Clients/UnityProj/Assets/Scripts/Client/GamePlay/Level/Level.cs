using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BiangStudio;
using Sirenix.OdinInspector;

public class Level : MonoBehaviour
{
    [ListDrawerSettings(ListElementLabelName = "GroupName", ShowIndexLabels = false)]
    public List<StepGroup> StepGroupList = new List<StepGroup>();

    public List<CheckPoint> CheckPoints = new List<CheckPoint>();

    public Vector3 PlayerRebornPosition;

    public Material DefaultStepMaterial;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        Player player = GetComponentInChildren<Player>();
        CheckPoints = GetComponentsInChildren<CheckPoint>().ToList();
        CheckPoints.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        for (int index = 0; index < CheckPoints.Count; index++)
        {
            CheckPoint cp = CheckPoints[index];
            cp.name = $"CheckPoint_{index}";
        }

        PlayerRebornPosition = player.transform.position;
        foreach (StepGroup group in StepGroupList)
        {
            group.Initialize();
        }
    }

    [Button("刷新整理Step", ButtonSizes.Large)]
    public void RefreshStep()
    {
        Step[] steps = GetComponentsInChildren<Step>();
        foreach (Step step in steps)
        {
            step.transform.parent = transform;
        }

        sortIntoGroup(StepType.DownSpikeRoot, "DownSpikeRoot");
        sortIntoGroup(StepType.UpSpikeRoot, "UpSpikeRoot");
        sortIntoGroup(StepType.CheckPoint, "CheckPointRoot");
        sortIntoGroup(StepType.Wall, "WallGroup");
        sortIntoGroup(StepType.Ceiling, "CeilingGroup");
        sortIntoGroup(StepType.Floor, "FloorGroup");
        sortIntoGroup(StepType.Normal, "OtherStepGroup");

        void clearGroupRoot(string groupTag)
        {
            List<GameObject> needToDestroy = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.CompareTag(groupTag))
                {
                    needToDestroy.Add(child.gameObject);
                }
            }

            foreach (GameObject go in needToDestroy)
            {
                DestroyImmediate(go);
            }
        }

        void sortIntoGroup(StepType stepType, string groupTag)
        {
            clearGroupRoot(groupTag);
            Transform root = new GameObject(groupTag).transform;
            root.transform.parent = transform;
            root.gameObject.tag = groupTag;
            foreach (Step step in steps)
            {
                if (step.StepType == stepType)
                {
                    step.transform.parent = root;
                }
            }
        }

        clearGroupRoot("LevelStepGroup");
        List<StepGroup> removeStepGroups = new List<StepGroup>();
        foreach (StepGroup group in StepGroupList)
        {
            if (group.StepList.Count == 0)
            {
                removeStepGroups.Add(group);
                continue;
            }

            group.Initialize();
            Transform levelStepGroup = new GameObject($"StepGroup_{group.GroupName}").transform;
            levelStepGroup.transform.parent = transform;
            levelStepGroup.gameObject.tag = "LevelStepGroup";
            int index = 0;
            foreach (Step step in group.StepList)
            {
                step.transform.parent = levelStepGroup;
                step.name = $"{step.StepType}_{index}";
                index++;
            }
        }

        foreach (StepGroup sg in removeStepGroups)
        {
            StepGroupList.Remove(sg);
        }

        foreach (Star star in GetComponentsInChildren<Star>())
        {
            star.transform.parent = transform;
        }

        clearGroupRoot("StarRoot");
        Transform starRoot = new GameObject("StarRoot").transform;
        starRoot.transform.parent = transform;
        starRoot.gameObject.tag = "StarRoot";
        foreach (Star star in GetComponentsInChildren<Star>())
        {
            star.transform.parent = starRoot;
        }
    }
}

[Serializable]
public class StepGroup
{
    public string GroupName;
    public float ForcePassRatio = 0.3f;

    [OnValueChanged("Initialize")]
    public Gradient ColorGradient;

    public List<Step> StepList = new List<Step>();
    public Material Material;

    public void Initialize()
    {
        StepList.RemoveNull();

        StepList.Sort((a, b) => { return a.transform.position.x.CompareTo(b.transform.position.x); });

        for (int index = 0; index < StepList.Count; index++)
        {
            Step step = StepList[index];
            step.ForcePassRatio = ForcePassRatio;
            step.MeshRenderer.material = Material;
            step.ColorGradient = ColorGradient;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            step.MeshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", ColorGradient.Evaluate((index + 0.5f) / StepList.Count));
            step.MeshRenderer.SetPropertyBlock(mpb);
        }

        for (int i = 1; i < StepList.Count; i++)
        {
            Step step = StepList[i];
            Step previousStep = StepList[i - 1];
            step.PreviousStep = previousStep;
            previousStep.NextStep = step;
        }
    }
}