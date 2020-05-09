using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    public GameObject Player;
    bool isPaused = false;
    // Start is called before the first frame update
    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.P))
      {
        Debug.Log("P pressed");
        if (isPaused)
        {
          DeactivateMenu();
        }
        else
        {
          ActivateMenu();
        }
      }
    }

    public void QuitGame()
    {
      Debug.Log("QUIT");
      Time.timeScale = 1;
      isPaused = !isPaused;
      SceneManager.LoadScene("Menu");
    }

    void ActivateMenu()
    {
      Debug.Log("ActivateMenu");
      Time.timeScale = 0;
      Screen.lockCursor = false;
      pauseMenuUI.SetActive(true);
      isPaused = !isPaused;
      Player.GetComponent<CameraMovement>().enabled = false;
    }

    public void DeactivateMenu()
    {
      Debug.Log("DeactivateMenu");
      Time.timeScale = 1;
      Screen.lockCursor = true;
      pauseMenuUI.SetActive(false);
      isPaused = !isPaused;
      Player.GetComponent<CameraMovement>().enabled = true;
    }
}
