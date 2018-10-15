using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string sceneName;

    private void Start()
    {
        OnLevelWasLoad(0);
    }

    void OnLevelWasLoad(int sceneIndex)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        
        if(newSceneName != sceneName){
            sceneName = newSceneName;
            Invoke("PlayMusic", 1f);
        }
    }


    void PlayMusic()
    {
        AudioClip clipToplay = null;
        if (sceneName == "Menu")
            clipToplay = menuTheme;
        else if (sceneName == "Game")
            clipToplay = mainTheme;

        if (clipToplay != null)
        {
            AudioManager.instance.PlayMusic(clipToplay, 2);
            Invoke("PlayMusic", clipToplay.length);
        }
    }

}
