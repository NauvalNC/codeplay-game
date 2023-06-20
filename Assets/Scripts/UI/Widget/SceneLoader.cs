using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneLoader : BaseWidget
{
    private static readonly string LOADING_SCENE = "Loading";
    private static readonly string LEVEL_TO_LOAD = "LEVEL_TO_LOAD";

    [SerializeField] private Image imgLoadingBar;

    private void Start()
    {
        string tLevelToLoad = PlayerPrefs.GetString(LEVEL_TO_LOAD);
        if (tLevelToLoad == string.Empty)
        {
            Debug.LogWarning("Cannot load level. Level to load is empty.");
            return;
        }

        StartCoroutine(LoadScene_Internal(tLevelToLoad));
    }

    public static void LoadScene(string levelToLoad)
    {
        PlayerPrefs.SetString(LEVEL_TO_LOAD, levelToLoad);
        SceneManager.LoadScene(LOADING_SCENE);
    }

    private IEnumerator LoadScene_Internal(string levelToLoad)
    {
        AsyncOperation tOperation = SceneManager.LoadSceneAsync(levelToLoad);
        
        while(!tOperation.isDone)
        {
            float tProgress = Mathf.Clamp01(tOperation.progress / 0.9f);
            imgLoadingBar.fillAmount = tProgress;
            yield return null;
        }
    }
}
