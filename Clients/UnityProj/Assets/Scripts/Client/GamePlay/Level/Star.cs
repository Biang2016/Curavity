using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour
{
    public void OnScore()
    {
        LevelManager.Instance.Score();
        Destroy(gameObject);
    }
}
