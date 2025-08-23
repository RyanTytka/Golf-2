using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.EventSystems;

public class scorecard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject scorecardObj; //the actual scorecard that is shown when hovered
    public List<GameObject> parTextObjs;
    public List<GameObject> scoreTextObjs;

    public bool hoverDisplay;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hoverDisplay) return;
        scorecardObj.SetActive(true);
        //update scorecard text
        Course course = GameObject.Find("CourseManager").GetComponent<Course>();
        for (int i = 0; i < parTextObjs.Count; i++)
        {
            parTextObjs[i].GetComponent<TextMeshProUGUI>().text = course.pars[i].ToString();
        }
        for (int i = 0; i < course.scores.Count; i++)
        {
            scoreTextObjs[i].GetComponent<TextMeshProUGUI>().text = course.scores[i].ToString();
        }
        //total scores
        int total = 0;
        for (int i = 0; i < course.scores.Count; i++)
        {
            total += course.scores[i];
        }
        scoreTextObjs[9].GetComponent<TextMeshProUGUI>().text = total.ToString();
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        if (!hoverDisplay) return;
        scorecardObj.SetActive(false);
    }

    //called during lose screen to show scores
    public void ShowScores()
    {
        Course course = GameObject.Find("CourseManager").GetComponent<Course>();
        for (int i = 0; i < parTextObjs.Count; i++)
        {
            parTextObjs[i].GetComponent<TextMeshProUGUI>().text = course.pars[i].ToString();
        }
        for (int i = 0; i < course.scores.Count; i++)
        {
            scoreTextObjs[i].GetComponent<TextMeshProUGUI>().text = course.scores[i].ToString();
        }
        //total scores
        int total = 0;
        for (int i = 0; i < course.scores.Count; i++)
        {
            total += course.scores[i];
        }
        scoreTextObjs[9].GetComponent<TextMeshProUGUI>().text = total.ToString();
    }
}
