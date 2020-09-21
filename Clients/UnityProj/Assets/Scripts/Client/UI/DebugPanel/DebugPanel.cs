using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BiangStudio.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class DebugPanel : BaseUIPanel
{
    public Button DebugToggleButton;
    public Gradient FrameRateGradient;
    public Text fpsText;
    public Text velYText;
    private float deltaTime;

    private Dictionary<string, DebugPanelComponent> DebugComponentDictTree = new Dictionary<string, DebugPanelComponent>();
    private List<DebugPanelColumn> DebugButtonColumns = new List<DebugPanelColumn>();
    public Transform ColumnContainer;

    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Fixed,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    void Start()
    {
        IsButtonsShow = false;
        Type type = typeof(DebugPanel);
        List<(MethodInfo, DebugControllerAttribute)> dcas = new List<(MethodInfo, DebugControllerAttribute)>();
        foreach (MethodInfo m in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in m.GetCustomAttributes(false))
            {
                if (a is DebugControllerAttribute dca)
                {
                    dcas.Add((m, dca));
                }
            }
        }

        dcas.Sort((pair1, pair2) => { return pair1.Item2.Priority.CompareTo(pair2.Item2.Priority); });

        foreach ((MethodInfo, DebugControllerAttribute) pair in dcas)
        {
            MethodInfo m = pair.Item1;
            switch (pair.Item2)
            {
                case DebugButtonAttribute dba:
                {
                    if (string.IsNullOrEmpty(dba.MethodName))
                    {
                        AddButton(dba.ButtonName, 0, DebugComponentDictTree, () => { m.Invoke(this, new object[] { }); }, true);
                    }
                    else
                    {
                        MethodInfo method = null;
                        MethodInfo method_2 = null;
                        foreach (MethodInfo mt in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (mt.Name.Equals(dba.MethodName)) method = mt;
                            if (mt.Name.Equals(dba.MethodName_2)) method_2 = mt;
                        }

                        if (method == null && !string.IsNullOrWhiteSpace(dba.MethodName))
                        {
                            Debug.LogError($"[DebugPanel] 无法找到名为{dba.MethodName}的函数");
                            return;
                        }

                        if (method_2 == null && !string.IsNullOrWhiteSpace(dba.MethodName_2))
                        {
                            Debug.LogError($"[DebugPanel] 无法找到名为{dba.MethodName_2}的函数");
                            return;
                        }

                        try
                        {
                            if (method != null)
                            {
                                List<string> strList = (List<string>) method.Invoke(this, new object[] { });

                                if (method_2 == null)
                                {
                                    foreach (string s in strList)
                                    {
                                        string buttonName = string.Format(dba.ButtonName, s);
                                        AddButton(buttonName, 0, DebugComponentDictTree, () => { m.Invoke(this, new object[] {s}); }, true);
                                    }
                                }
                                else
                                {
                                    IList objList = (IList) method_2.Invoke(this, new object[] { });
                                    for (int index = 0; index < strList.Count; index++)
                                    {
                                        string s = strList[index];
                                        object obj = objList[index];
                                        string buttonName = string.Format(dba.ButtonName, s);
                                        AddButton(buttonName, 0, DebugComponentDictTree, () => { m.Invoke(this, new object[] {s, obj}); }, true);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            throw;
                        }
                    }

                    break;
                }
                case DebugToggleButtonAttribute dtba:
                {
                    AddButton(dtba.ButtonName, 0, DebugComponentDictTree, () => { m.Invoke(this, new object[] { }); }, true);
                    break;
                }
                case DebugSliderAttribute dsa:
                {
                    AddSlider(dsa.SliderName, 0, dsa.DefaultValue, dsa.Min, dsa.Max, DebugComponentDictTree, (float value) => { m.Invoke(this, new object[] {value}); });
                    break;
                }
            }
        }
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = fps.ToString("###");
        DebugToggleButton.image.color = FrameRateGradient.Evaluate(fps / 144f);

        if (LevelManager.Instance.Player != null)
        {
            velYText.text = LevelManager.Instance.Player.Rigidbody.velocity.y.ToString("F1");
        }
    }

    private bool isButtonsShow = false;

    private bool IsButtonsShow
    {
        get { return isButtonsShow; }
        set
        {
            isButtonsShow = value;
            ColumnContainer.gameObject.SetActive(value);
        }
    }

    public void ToggleDebugPanel()
    {
        IsButtonsShow = !IsButtonsShow;
    }

    private void AddButton(string buttonName, int layerDepth, Dictionary<string, DebugPanelComponent> currentTree, UnityAction action, bool functional)
    {
        if (layerDepth >= DebugButtonColumns.Count)
        {
            DebugPanelColumn column = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelColumn].AllocateGameObject<DebugPanelColumn>(ColumnContainer);
            DebugButtonColumns.Add(column);
        }

        string[] paths = buttonName.Split('/');

        DebugPanelColumn currentColumn = DebugButtonColumns[layerDepth];
        if (!currentTree.ContainsKey(paths[0]))
        {
            DebugPanelButton btn = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelButton].AllocateGameObject<DebugPanelButton>(currentColumn.transform);
            btn.Initialize(paths[0], (paths.Length == 1 && functional)
                ? action
                : ToggleSubColumn(btn, currentTree));
            btn.gameObject.SetActive(layerDepth == 0);
            currentTree.Add(paths[0], btn);
        }

        if (paths.Length > 1)
        {
            string remainingPath = buttonName.Replace(paths[0] + "/", "");
            AddButton(remainingPath, layerDepth + 1, currentTree[paths[0]].DebugComponentDictTree, action, functional);
        }
    }

    private static UnityAction ToggleSubColumn(DebugPanelComponent cpm, Dictionary<string, DebugPanelComponent> currentTree)
    {
        return () =>
        {
            cpm.IsOpen = !cpm.IsOpen;
            foreach (KeyValuePair<string, DebugPanelComponent> kv in currentTree)
            {
                if (kv.Value != cpm && cpm.IsOpen)
                {
                    kv.Value.IsOpen = false;
                }
            }
        };
    }

    private void AddSlider(string sliderName, int layerDepth, float defaultValue, float min, float max, Dictionary<string, DebugPanelComponent> currentTree, UnityAction<float> action)
    {
        if (layerDepth >= DebugButtonColumns.Count)
        {
            DebugPanelColumn column = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelColumn].AllocateGameObject<DebugPanelColumn>(ColumnContainer);
            DebugButtonColumns.Add(column);
        }

        string[] paths = sliderName.Split('/');
        DebugPanelColumn currentColumn = DebugButtonColumns[layerDepth];
        if (paths.Length == 0)
        {
            return;
        }
        else if (paths.Length == 1)
        {
            if (!currentTree.ContainsKey(paths[0]))
            {
                DebugPanelSlider slider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelSlider].AllocateGameObject<DebugPanelSlider>(currentColumn.transform);
                slider.Initialize(paths[0], defaultValue, min, max, action);
                slider.gameObject.SetActive(layerDepth == 0);
                currentTree.Add(paths[0], slider);
            }
        }
        else
        {
            if (currentTree.ContainsKey(paths[0]))
            {
                string remainingPath = sliderName.Replace(paths[0] + "/", "");
                AddSlider(remainingPath, layerDepth + 1, defaultValue, min, max, currentTree[paths[0]].DebugComponentDictTree, action);
                return;
            }
            else
            {
                string buttonPath = sliderName.Replace("/" + paths[paths.Length - 1], "");
                AddButton(buttonPath, layerDepth, currentTree, null, false);
                AddSlider(sliderName, layerDepth, defaultValue, min, max, currentTree, action);
                return;
            }
        }
    }

    [DebugButton("SwitchLevel/{0}", "GetLevelNames", -10)]
    public void ChangeLevel(string levelName)
    {
        ClientGameManager.DebugChangeLevelName = levelName;
        ClientGameManager.Instance.ReloadGame();
    }

    public List<string> GetLevelNames()
    {
        return ClientGameManager.Instance.LevelList;
    }

    [DebugButton("SwitchCheckPoint/{0}", "GetCheckPointNames", "GetCheckPoints", -10)]
    public void ChangeCheckPoint(string checkPointName, CheckPoint checkPoint)
    {
        LevelManager.Instance.Player.ResetToPosition(checkPoint.transform.position + Vector3.up * 2f);
    }

    public List<string> GetCheckPointNames()
    {
        List<string> res = new List<string>();
        foreach (CheckPoint cp in LevelManager.Instance.CurrentLevel.CheckPoints)
        {
            res.Add(cp.name);
        }

        return res;
    }

    public List<CheckPoint> GetCheckPoints()
    {
        return LevelManager.Instance.CurrentLevel.CheckPoints;
    }
}