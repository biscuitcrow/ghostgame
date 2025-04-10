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


    public Animator transitionAnimator;


    /*
    private void Start()
    {
        //Play main menu music
        AudioManager.instance.Play("Main Menu Music");
        AudioManager.instance.SetVolume("Main Menu Music", 0f);
        AudioManager.instance.FadeVolume("Main Menu Music", 1f, 5f);
    }
    */




    /*
    // Loads the main game level
    IEnumerator LoadLevel()
    {
        transitionAnimator.SetTrigger("triggerSceneTransitionStart");
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene("GameScene");
        gameSceneTransitionAnimator = GameObject.FindWithTag("Main Game Transition Animator").GetComponent<Animator>();
    }
    */

    // Loads the main game scene
    public void LoadGameScene()
    {
        // Fades out main menu music then loads the main game
        StartCoroutine("FadeOutMusic", "Main Menu Music");
        StartCoroutine("LoadLevel", "GameScene");
    }

    public void LoadStartMenuScene()
    {
        // Fades out game level music then loads the main game
        StartCoroutine("FadeOutMusic", GameManager.Instance.listOfAllLevelThemesNames[GameManager.Instance.currentThemeIndex]);
        SceneManager.LoadScene("StartMenu");
        Time.timeScale = 1;
    }

    // Loads a level with specified name
    IEnumerator LoadLevel(string sceneName)
    {
        //Sets the transition animator to that in the current scene
        transitionAnimator = GameObject.FindWithTag("Main Game Transition Animator").GetComponent<Animator>();
        transitionAnimator.SetTrigger("triggerSceneTransitionStart");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);

    }

    IEnumerator FadeOutMusic(string musicName)
    {
        AudioManager.instance.FadeVolume(musicName, 0f, 1f);
        yield return new WaitForSeconds(1f);
        AudioManager.instance.Stop(musicName);
    }

}


