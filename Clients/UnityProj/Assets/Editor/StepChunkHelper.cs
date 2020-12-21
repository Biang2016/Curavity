using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using UnityEditor;
using UnityEngine;

public class StepChunkHelper
{
    [MenuItem("Tools/CreateStepChunk")]
    public static void CreateStepChunk()
    {
        if (Selection.gameObjects.Length != 0)
        {
            InputDialogWindow inputWindow = new InputDialogWindow("创建StepChunk", "确定", "取消",
                confirm: OnConfirm,
                cancel: null
            );
            inputWindow.ShowUtility();
        }
    }

    [MenuItem("Tools/QuickGroundChunk &L")]
    private static void QuickGroundChunk()
    {
        OnConfirm("");
    }

    private static void OnConfirm(string chunkName)
    {
        List<Step> StepList = new List<Step>();
        Level level = null;
        foreach (GameObject go in Selection.gameObjects)
        {
            Step step = go.GetComponent<Step>();
            if (step && !StepList.Contains(step))
            {
                if (level == null)
                {
                    level = step.GetComponentInParent<Level>();
                }

                StepList.Add(step);
            }
        }

        if (StepList.Count == 0) return;

        if (level != null)
        {
            foreach (StepGroup stepGroup in level.StepGroupList)
            {
                stepGroup.StepList.RemoveNull();
                List<Step> removeStepList = new List<Step>();
                foreach (Step step in stepGroup.StepList)
                {
                    if (StepList.Contains(step))
                    {
                        removeStepList.Add(step);
                    }
                }

                foreach (Step step in removeStepList)
                {
                    if (step.PreviousStep && step.NextStep)
                    {
                        step.PreviousStep.NextStep = step.NextStep;
                        step.NextStep.PreviousStep = step.PreviousStep;
                    }

                    if (step.PreviousStep) step.PreviousStep.NextStep = null;
                    if (step.NextStep) step.NextStep.PreviousStep = null;
                    step.PreviousStep = null;
                    step.NextStep = null;
                    stepGroup.StepList.Remove(step);
                }
            }

            StepGroup newStepGroup = new StepGroup();
            if (string.IsNullOrWhiteSpace(chunkName))
            {
                for (int i = 1; i < 999; i++)
                {
                    chunkName = $"Chunk_{i}";
                    bool found = true;
                    foreach (StepGroup sg in level.StepGroupList)
                    {
                        if (sg.GroupName.Equals(chunkName))
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found) break;
                }
            }

            newStepGroup.GroupName = chunkName;
            newStepGroup.Material = level.DefaultStepMaterial;
            level.StepGroupList.Add(newStepGroup);
            newStepGroup.StepList = StepList;
            newStepGroup.ColorGradient = StepList[0].ColorGradient;
            newStepGroup.Initialize();

            level.RefreshStep();
        }
    }
}