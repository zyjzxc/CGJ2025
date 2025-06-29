using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinLoseTitle : MonoBehaviour
{
    public Text titleText;
    
    bool firstWin = true;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (GameContext.GameWin)
        {
            titleText.text = "Now. Everything is Alive.";
            titleText.gameObject.SetActive(true);
            
            if(firstWin)
                GreenAreaMgr.Instance.GreenAll();
            firstWin = false;
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
