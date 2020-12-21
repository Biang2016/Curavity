using System.Collections;
using BiangLibrary.GamePlay;
using BiangLibrary.GamePlay.UI;
using BiangLibrary.Singleton;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : TSingletonBaseManager<LevelManager>
{
    public Transform LevelRoot;
    private GameObject CurrentLevelGO;

    public Level CurrentLevel;
    public Player Player;

    private int PlayerLife;
    private int PlayerScore;

    private HUDPanel HUDPanel;

    public override void Awake()
    {
        base.Awake();
        LevelRoot = new GameObject("LevelRoot").transform;
    }

    public void LoadLevel(string levelName)
    {
        if (CurrentLevelGO) Object.Destroy(CurrentLevelGO);
        GameObject prefab = PrefabManager.Instance.GetPrefab(levelName);
        CurrentLevelGO = Object.Instantiate(prefab, LevelRoot);
        CurrentLevel = CurrentLevelGO.GetComponent<Level>();
        Player = CurrentLevelGO.GetComponentInChildren<Player>();

        HUDPanel = UIManager.Instance.ShowUIForms<HUDPanel>();
        PlayerLife = 100;
        HUDPanel.SetPlayerLife(PlayerLife);
        PlayerScore = 0;
        HUDPanel.SetPlayerScore(PlayerScore);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
    }

    public void Die(UnityAction fxAction)
    {
        ClientGameManager.Instance.StartCoroutine(Co_ReloadGame(fxAction));
    }

    public void Score()
    {
        PlayerScore++;
        HUDPanel.SetPlayerScore(PlayerScore);
    }

    IEnumerator Co_ReloadGame(UnityAction fxAction)
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSeconds(0.15f);
        fxAction.Invoke();
        Time.timeScale = 1f;
        yield return new WaitForSeconds(1f);
        PlayerLife--;
        HUDPanel.SetPlayerLife(PlayerLife);
        Player.ResetToPosition(CurrentLevel.PlayerRebornPosition);
    }
}