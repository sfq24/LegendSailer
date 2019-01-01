using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace LegendSailer
{

    public class LevelManager : MonoBehaviour
    {
        public float autoLoadNextLevel = 2.5f;

        void Start()
        {
            if (SceneManager.GetActiveScene().name != "Start")
            {
                if (autoLoadNextLevel <= 0)
                {
                    Debug.LogWarning("auto load next level disabled");
                }
                else
                {
                    Invoke("LoadNextLevel", autoLoadNextLevel);
                }

            }

        }
        public void LoadLevel(string name)
        {
            Debug.Log("New Level load: " + name);
            SceneManager.LoadScene(name);
            
        }

        public void QuitRequest()
        {
            Debug.Log("Quit requested");
            Application.Quit();
        }

        public void LoadNextLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        }

        public void PlayButton()
        {
            LoadLevel("Play");
        }

    }

}