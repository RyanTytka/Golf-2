using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mainMenuUI : MonoBehaviour
{
    public void StartNewGame()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Course");
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //start new course
        GameObject.Find("CourseManager").GetComponent<Course>().courseNum = 0;
        GameObject.Find("CourseManager").GetComponent<Course>().tees = 0;
        GameObject.Find("CourseManager").GetComponent<Course>().NewCourse();
        GameObject.Find("CourseManager").GetComponent<Course>().comingFromShop = false;

        //set up hand and UI
        GameObject.Find("GameManager").GetComponent<Hand>().RemoveDeck();
        GameObject.Find("GameManager").GetComponent<Hand>().NewDeck();
        GameObject.Find("SwingButton").GetComponent<Button>().onClick.AddListener
            (GameObject.Find("CourseManager").GetComponent<Course>().Swing);
        GameObject.Find("MulliganButton").GetComponent<Button>().onClick.AddListener
            (GameObject.Find("CourseManager").GetComponent<Course>().Mulligan);
        // Unsubscribe to avoid duplicate calls in the future
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ToMainMenu()
    {
        Destroy(GameObject.Find("GameManager"));
        Destroy(GameObject.Find("CourseManager"));
        SceneManager.LoadScene("Main Menu");
    }
}
