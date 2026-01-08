using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class endScreenManager : MonoBehaviour
{

    void Start()
    {
        //update end screen UI
        Course c = GameObject.Find("CourseManager").GetComponent<Course>();
        if (c.currentPlaythrough[c.currentPlaythrough.Count - 1].lostRun)
        {
            GameObject.Find("EndMessage").GetComponent<TextMeshProUGUI>().text = "You Lost";
        }
        else
        {
            GameObject.Find("EndMessage").GetComponent<TextMeshProUGUI>().text = "You Won";
        }
        int index = 0;
        foreach (scorecard sc in GameObject.Find("RecapParent").GetComponentsInChildren<scorecard>())
        {
            //update recap for each course played through
            if (index < c.currentPlaythrough.Count)
            {
                sc.gameObject.SetActive(true);
                sc.ShowRecap(c.currentPlaythrough[index]);
            }
            else
            {
                sc.gameObject.SetActive(false);
            }
            index++;
        }
        //c.courseDisplay.SetActive(false);
        //GameObject.Find("GameManager").GetComponent<Hand>().RemoveDeck();
        //SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
