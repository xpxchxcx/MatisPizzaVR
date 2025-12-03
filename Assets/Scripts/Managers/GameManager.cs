using UnityEngine;
using AudioSystem;
public class GameManager : Singleton<GameManager>
{
    public GameObject toppingsDebugUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM("BGM", fadeTime: 0.5f);
        SoundManager.Instance.PlayLoopSFX("Crowd");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
