﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitScript : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {

  }
  public void QuitGame()
  {
    Debug.Log("QUIT");
//      Application.Quit();
    SceneManager.LoadScene("Menu");
  }
  // Update is called once per frame
  void Update()
  {

  }
}
