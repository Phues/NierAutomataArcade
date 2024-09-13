using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string sceneName;
    private AudioSource audioSource;
    public AudioClip[] bgmClips;
    public float timeScale = 1.0f;
    
    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        NewGame();
    }
    
    void Update()
    {
       Time.timeScale = timeScale;
    }

    public void NewGame()
    {
        SceneManager.LoadScene(sceneName);
        Time.timeScale = timeScale;
        //select a random bgm clip
        int randomClip = Random.Range(0, bgmClips.Length);
        audioSource.clip = bgmClips[randomClip];
        audioSource.Play();
    }
}
