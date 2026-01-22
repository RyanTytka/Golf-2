using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mainMenuUI : MonoBehaviour
{
    public float scrollDistance = 5f;
    public float duration = 1.5f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Camera mainCamera;

    private bool isMoving = false;

    public void ScrollDown()
    {
        scrollDistance = 10;
        if (!isMoving)
            StartCoroutine(ScrollRoutine());
    }

    public void ScrollDown(System.Action action)
    {
        scrollDistance = 10;
        if (!isMoving)
            StartCoroutine(ScrollRoutine(action));
    }

    public void ScrollUp(System.Action action = null)
    {
        scrollDistance = -10;
        if (!isMoving)
            StartCoroutine(ScrollRoutine(action));
    }

    private IEnumerator ScrollRoutine(System.Action action = null)
    {
        isMoving = true;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = startPos + Vector3.down * scrollDistance;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float easedT = easeCurve.Evaluate(t);

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        isMoving = false;

        if(action != null)
        {
            action.Invoke();
        }
    }

    public void HideMainMenu()
    {
        if (GameObject.Find("Title Image") != null)
        {
            GameObject.Find("Title Image").SetActive(false);
            GameObject.Find("StartButton").SetActive(false);
        }
    }


    //public void StartNewGame()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //    SceneManager.LoadScene("Course");
    //}
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        ////start new course
        //GameObject.Find("CourseManager").GetComponent<Course>().courseNum = 0;
        //GameObject.Find("CourseManager").GetComponent<Course>().tees = 0;
        //GameObject.Find("CourseManager").GetComponent<Course>().NewCourse();
        //GameObject.Find("CourseManager").GetComponent<Course>().comingFromShop = false;

        ////set up hand and UI
        //GameObject.Find("GameManager").GetComponent<Hand>().StartGame();
        //GameObject.Find("SwingButton").GetComponent<Button>().onClick.AddListener
        //    (GameObject.Find("CourseManager").GetComponent<Course>().Swing);
        //GameObject.Find("MulliganButton").GetComponent<Button>().onClick.AddListener
        //    (GameObject.Find("CourseManager").GetComponent<Course>().Mulligan);
        //// Unsubscribe to avoid duplicate calls in the future
        //SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ToMainMenu()
    {
        Destroy(GameObject.Find("GameManager"));
        Destroy(GameObject.Find("CourseManager"));
        Destroy(GameObject.Find("Music Manager"));
        SceneManager.LoadScene("Course");
    }
}
