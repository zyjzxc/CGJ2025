using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinLoseTitle : MonoBehaviour
{
    public Text titleText;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (GameContext.GameWin)
        {
            titleText.text = "Liven All!";
            titleText.gameObject.SetActive(true);
        }

        else if (GameContext.GameOver)
        {
            titleText.text = "You Die";
            titleText.gameObject.SetActive(true);
        }
    }

    public static void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
