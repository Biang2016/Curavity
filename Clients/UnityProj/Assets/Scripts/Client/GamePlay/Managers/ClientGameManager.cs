using System.Collections.Generic;
using System.Diagnostics;
using BiangStudio.GamePlay;
using BiangStudio.GamePlay.UI;
using BiangStudio.Log;
using BiangStudio.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class ClientGameManager : MonoSingleton<ClientGameManager>
{
    #region Managers

    #region Mono

    private AudioManager AudioManager => AudioManager.Instance;
    private CameraManager CameraManager => CameraManager.Instance;
    private UIManager UIManager => UIManager.Instance;

    #endregion

    #region TSingletonBaseManager

    #region Resources

    private LayerManager LayerManager => LayerManager.Instance;
    private PrefabManager PrefabManager => PrefabManager.Instance;
    private GameObjectPoolManager GameObjectPoolManager => GameObjectPoolManager.Instance;

    #endregion

    #region Framework

    private GameStateManager GameStateManager => GameStateManager.Instance;
    private RoutineManager RoutineManager => RoutineManager.Instance;

    #endregion

    #region GamePlay

    #region Level

    private LevelManager LevelManager => LevelManager.Instance;
    private FXManager FXManager => FXManager.Instance;

    #endregion

    #endregion

    #endregion

    #endregion

    #region DebugLoadLevel

    [LabelText("开局关卡")]
    [ValueDropdown("LevelList")]
    public string StartWorldName;

    public static string DebugChangeLevelName = null;

    public List<string> LevelList = new List<string>();

    #endregion

    private void Awake()
    {
        UIManager.Init(
            (prefabName) => Instantiate(PrefabManager.GetPrefab(prefabName)),
            Debug.LogError,
            () => Input.GetMouseButtonDown(0),
            () => Input.GetMouseButtonDown(1),
            () => Input.GetKeyDown(KeyCode.Escape),
            () => Input.GetKeyDown(KeyCode.Return),
            () => Input.GetKeyDown(KeyCode.Tab)
        );

        LayerManager.Awake();
        PrefabManager.Awake();
        if (!GameObjectPoolManager.IsInit)
        {
            Transform root = new GameObject("GameObjectPool").transform;
            DontDestroyOnLoad(root.gameObject);
            GameObjectPoolManager.Init(root);
            GameObjectPoolManager.Awake();
        }

        RoutineManager.LogErrorHandler = Debug.LogError;
        RoutineManager.Awake();
        GameStateManager.Awake();

        FXManager.Awake();
        LevelManager.Awake();
    }

    private void Start()
    {
        LayerManager.Start();
        PrefabManager.Start();
        GameObjectPoolManager.Start();

        RoutineManager.Start();
        GameStateManager.Start();

        FXManager.Start();
        LevelManager.Start();

        UIManager.Instance.ShowUIForms<DebugPanel>();
#if !DEBUG
        UIManager.Instance.CloseUIForm<DebugPanel>();
#endif
        StartGame();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(0);
            return;
        }

        LayerManager.Update(Time.deltaTime);
        PrefabManager.Update(Time.deltaTime);
        GameObjectPoolManager.Update(Time.deltaTime);

        RoutineManager.Update(Time.deltaTime, Time.frameCount);
        GameStateManager.Update(Time.deltaTime);

        FXManager.Update(Time.deltaTime);
        LevelManager.Update(Time.deltaTime);
    }

    void LateUpdate()
    {
        LayerManager.LateUpdate(Time.deltaTime);
        PrefabManager.LateUpdate(Time.deltaTime);
        GameObjectPoolManager.LateUpdate(Time.deltaTime);

        RoutineManager.LateUpdate(Time.deltaTime);
        GameStateManager.LateUpdate(Time.deltaTime);

        FXManager.LateUpdate(Time.deltaTime);
        LevelManager.LateUpdate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        LayerManager.FixedUpdate(Time.fixedDeltaTime);
        PrefabManager.FixedUpdate(Time.fixedDeltaTime);
        GameObjectPoolManager.FixedUpdate(Time.fixedDeltaTime);

        RoutineManager.FixedUpdate(Time.fixedDeltaTime);
        GameStateManager.FixedUpdate(Time.fixedDeltaTime);

        FXManager.FixedUpdate(Time.fixedDeltaTime);
        LevelManager.FixedUpdate(Time.fixedDeltaTime);
    }

    private void StartGame()
    {
        if (DebugChangeLevelName == null)
        {
            LevelManager.Instance.LoadLevel(StartWorldName);
        }
        else
        {
            LevelManager.Instance.LoadLevel(DebugChangeLevelName);
        }
    }

    public void ReloadGame()
    {
        ShutDownGame();
        SceneManager.LoadScene("MainScene");
    }

    private void ShutDownGame()
    {
        LevelManager.ShutDown();
        FXManager.ShutDown();

        GameStateManager.ShutDown();
        RoutineManager.ShutDown();

        GameObjectPoolManager.ShutDown();
        PrefabManager.ShutDown();
        LayerManager.ShutDown();
    }
}