using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour
{
    public void OnScore()
    {
        if (!LevelManager.Instance.CurrentLevel.IsGameEnd)
        {
            LevelManager.Instance.Score();
        }

        AudioManager.Instance.SoundPlay("sfx/Score", 0.2f);
        Destroy(gameObject);
    }
}