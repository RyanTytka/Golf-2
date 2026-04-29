using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class tutorialManager : MonoBehaviour
{
    public GameObject[] panels; //Each panel is a page of the tutorial
    public int pageNum;
    public GameObject prevButton, nextButton, bgPanel, settingsPanel, feedbackPanel, feedbackSentPanel;

    public void OpenTutorial()
    {
        //set to first page
        gameObject.SetActive(true);
        pageNum = 0;
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
        panels[0].SetActive(true);
        settingsPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        feedbackSentPanel.SetActive(false);
        nextButton.GetComponent<Button>().interactable = true;
        prevButton.GetComponent<Button>().interactable = false;
        nextButton.SetActive(true);
        prevButton.SetActive(true);
        //animate opening
        bgPanel.GetComponent<RectTransform>().transform.localScale = new Vector3(0, 0, 0);
        bgPanel.GetComponent<RectTransform>().transform.DOScale(new Vector3(1, 1, 1), 0.15f);//.SetEase(Ease.OutBounce);
    }

    public void OpenSettings()
    {
        gameObject.SetActive(true);
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
        feedbackPanel.SetActive(false);
        settingsPanel.SetActive(true);
        feedbackSentPanel.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        GameObject.Find("Music Manager").GetComponent<musicmanager>().musicSlider = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().musicSlider;
        GameObject.Find("Music Manager").GetComponent<musicmanager>().sfxSlider = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().sfxSlider;
        GameObject.Find("Music Manager").GetComponent<musicmanager>().musicSlider.onValueChanged.AddListener(GameObject.Find("Music Manager").GetComponent<musicmanager>().SetMusicVolume);
        GameObject.Find("Music Manager").GetComponent<musicmanager>().sfxSlider.onValueChanged.AddListener(GameObject.Find("Music Manager").GetComponent<musicmanager>().SetSFXVolume);
        GameObject.Find("Music Manager").GetComponent<musicmanager>().LoadVolumes();
        //animate opening
        bgPanel.GetComponent<RectTransform>().transform.localScale = new Vector3(0, 0, 0);
        bgPanel.GetComponent<RectTransform>().transform.DOScale(new Vector3(1, 1, 1), 0.15f);//.SetEase(Ease.OutBounce);
    }

    public void OpenFeedback()
    {
        gameObject.SetActive(true);
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
        settingsPanel.SetActive(false);
        feedbackPanel.SetActive(true);
        feedbackSentPanel.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        //animate opening
        bgPanel.GetComponent<RectTransform>().transform.localScale = new Vector3(0, 0, 0);
        bgPanel.GetComponent<RectTransform>().transform.DOScale(new Vector3(1, 1, 1), 0.15f);//.SetEase(Ease.OutBounce);
    }

    public void CloseTutorial()
    {
        bgPanel.GetComponent<RectTransform>().transform.DOScale(new Vector3(0,0,0), 0.15f).onComplete = () =>
        {
            gameObject.SetActive(false);
        };//.SetEase(Ease.OutBounce);
    }

    public void NextPage()
    {
        panels[pageNum].SetActive(false);
        pageNum++;
        panels[pageNum].SetActive(true);
        if (pageNum >= panels.Length - 1)
            nextButton.GetComponent<Button>().interactable = false;
        prevButton.GetComponent<Button>().interactable = true;
    }

    public void PrevPage()
    {
        panels[pageNum].SetActive(false);
        pageNum--;
        panels[pageNum].SetActive(true);
        if (pageNum <= 0)
            prevButton.GetComponent<Button>().interactable = false;
        nextButton.GetComponent<Button>().interactable = true;
    }
}
