using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class rivalDisplay : MonoBehaviour
//public class rivalDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject nameObj;
    public GameObject descriptionObj;
    public GameObject scoreObj;
    public int idNum;
    public bool isLoseDisplay; //true if this is the obj displaying on the loss screen

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    if (isLoseDisplay) return;
    //    //update and show rival name, description, and score
    //    Course course = GameObject.Find("CourseManager").GetComponent<Course>();
    //    nameObj.GetComponent<TextMeshProUGUI>().text = course.rivalNames[idNum];
    //    descriptionObj.GetComponent<TextMeshProUGUI>().text = course.rivalDescriptions[idNum];
    //    scoreObj.GetComponent<TextMeshProUGUI>().text = course.rivalScores[idNum].ToString();
    //    GetComponent<Image>().sprite = course.rivalImages[idNum];
    //    descriptionObj.SetActive(true);
    //    nameObj.SetActive(true);
    //    scoreObj.SetActive(true);
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    if (isLoseDisplay) return;
    //    //hide name, description, and score
    //    descriptionObj.SetActive(false);
    //    nameObj.SetActive(false);
    //    scoreObj.SetActive(false);
    //}

    public void UpdateView()
    {
        Course course = GameObject.Find("CourseManager").GetComponent<Course>();
        nameObj.GetComponent<TextMeshProUGUI>().text = course.rivalNames[idNum];
        descriptionObj.GetComponent<TextMeshProUGUI>().text = course.rivalDescriptions[idNum];
        string mod = course.rivalScores[idNum] > 0 ? "(+" : "(";
        scoreObj.GetComponent<TextMeshProUGUI>().text = mod + course.rivalScores[idNum] + ")";
        GetComponent<Image>().sprite = course.rivalImages[idNum];
    }
}
