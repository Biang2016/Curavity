using BiangLibrary.GamePlay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BaseUIPanel
{
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

    public TextMeshProUGUI LifeText;
    public TextMeshProUGUI ScoreText;

    public Image LifeIcon;
    public Animator LifeAnim;

    public Image ScoreIcon;
    public Animator ScoreAnim;

    public void SetPlayerLife(int life)
    {
        LifeText.text = $"x{life}";
        LifeAnim.SetTrigger("Jump");
    }

    public void SetPlayerScore(int score)
    {
        ScoreText.text = $"x{score}/{LevelManager.Instance.CurrentLevel.TotalStartCount}";
        ScoreAnim.SetTrigger("Jump");
    }
}