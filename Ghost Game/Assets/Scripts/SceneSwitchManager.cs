using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchManager : MonoBehaviour
{
    #region // <------- SINGLETON PATTERN -------> //
    private static SceneSwitchManager _instance;
    public static SceneSwitchManager Instance
    {
        get
        {
            // Create logic to create the instance
            if (_instance == null)
            {
                GameObject obj = new GameObject("Scene Switch Manager");
                obj.AddComponent<SceneSwitchManager>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion


    private void Start()
    {
        //Play main menu music
        AudioManager.instance.Play("Main Menu Music");
        AudioManager.instance.SetVolume("Main Menu Music", 0f);
        AudioManager.instance.FadeVolume("Main Menu Music", 1f, 5f);
    }

    public void LoadGameScene()
    {
        StartCoroutine("FadeOutMainMenuMusic");
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator FadeOutMainMenuMusic()
    {
        AudioManager.instance.FadeVolume("Main Menu Music", 0f, 2f);
        yield return new WaitForSeconds(2f);
        AudioManager.instance.Stop("Main Menu Music");
    }
}


