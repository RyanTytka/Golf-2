using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

//public class rivalDisplay : MonoBehaviour
public class rivalDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject nameObj;
    public GameObject descriptionObj;
    public GameObject scoreObj;
    public GameObject bgPanel;
    public int idNum;
    public bool isLoseDisplay; //if true, do not display extra info on hover
    private Tween scaleTween;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLoseDisplay) return;
        //update and show rival name, description, and score
        UpdateView();
        descriptionObj.SetActive(true);
        nameObj.SetActive(true);
        scoreObj.SetActive(true);
        bgPanel.SetActive(true);
        //scale this obj up on hover
        scaleTween?.Kill();
        scaleTween = transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLoseDisplay) return;
        //hide name, description, and score
        descriptionObj.SetActive(false);
        nameObj.SetActive(false);
        scoreObj.SetActive(false);
        bgPanel.SetActive(false);
        //stop scaling this up
        scaleTween?.Kill();
        scaleTween = transform.DOScale(Vector3.one, 0.1f);
    }

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
