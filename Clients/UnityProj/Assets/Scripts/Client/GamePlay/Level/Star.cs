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

        WwiseAudioManager.Instance.Score.Post(gameObject);
        Destroy(gameObject);
    }
}