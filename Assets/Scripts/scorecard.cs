using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class scorecard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject scorecardObj; //the actual scorecard that is shown when hovered
    public List<GameObject> parTextObjs;
    public List<GameObject> scoreTextObjs;
    public GameObject courseTypeImageObj, rivalImageObj, rivalNameObj, rivalScoreObj;
    public bool hoverDisplay;
    public List<Sprite> courseImages;

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
            int score = course.scores[i] - course.pars[i];
            string mod = score > 0 ? "+" : "";
            scoreTextObjs[i].GetComponent<TextMeshProUGUI>().text = mod + score;
        }
        //total scores
        int total = 0;
        for (int i = 0; i < course.scores.Count; i++)
        {
            total += course.scores[i] - course.pars[i];
        }
        string totalMod = total > 0 ? "+" : "";
        scoreTextObjs[9].GetComponent<TextMeshProUGUI>().text = totalMod + total;
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        if (!hoverDisplay) return;
        scorecardObj.SetActive(false);
    }

    //update this obj to show the recap for specified course data
    public void ShowRecap(Course.CourseData courseData)
    {
        Course c = GameObject.Find("CourseManager").GetComponent<Course>();
        courseTypeImageObj.GetComponent<Image>().sprite = courseImages[courseData.courseType];
        rivalImageObj.GetComponent<Image>().sprite = c.rivalImages[courseData.rival];
        rivalNameObj.GetComponent<TextMeshProUGUI>().text = c.rivalNames[courseData.rival];
        string scoreStringMod = c.rivalScores[courseData.rival] > 0 ? "+" : "";
        rivalScoreObj.GetComponent<TextMeshProUGUI>().text = scoreStringMod + c.rivalScores[courseData.rival];
        for (int i = 0; i < parTextObjs.Count; i++)
        {
            parTextObjs[i].GetComponent<TextMeshProUGUI>().text = courseData.pars[i].ToString();
        }
        for (int i = 0; i < courseData.scores.Count; i++)
        {
            int score = courseData.scores[i] - courseData.pars[i];
            string mod = score > 0 ? "+" : "";
            scoreTextObjs[i].GetComponent<TextMeshProUGUI>().text = mod + score;
        }
        //total scores
        int total = 0;
        for (int i = 0; i < courseData.scores.Count; i++)
        {
            total += courseData.scores[i] - courseData.pars[i];
        }
        string totalMod = total > 0 ? "+" : "";
        scoreTextObjs[9].GetComponent<TextMeshProUGUI>().text = totalMod + total;
    }
}
