using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScrene;

    private void Awake()
    {
        gameOverScrene.SetActive(false);
    }
    private void Update()
    {

    }
    public void GameOver()
    {
        Debug.Log("Game over on");
        if (gameOverScrene != null)
        {
            gameOverScrene.SetActive(true);
        } else
        {
            Debug.Log("No object found");
        }
    }
    public void Restart()
    {
        gameOverScrene.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
