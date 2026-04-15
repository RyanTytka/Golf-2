using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class tutorialManager : MonoBehaviour
{
    //1 Select a club(use driver since it goes away)
    //2 This is path ball will take.Watch out for hazards
    //3 Play an ability/select a ball
    //4 Here is the flag, try to make it onto the green in as few shots as you can
    //5 This is putting meter. Putt is auto once you land on the green
    //6 This is your rival, each course has one. score and ability.
    //7 after done with hole: add new card or tees.Show rarities.Shop after each course

    //1 - "Play through 5 courses of 9 holes each. If you dont score lower than your rival on each course, you lose."
    //2 - "You start each hole with 5 cards in hand, plus your drivers. Each time you swing, you draw a card."
    //3 - "There are 4 types of card. Clubs, Balls, Abilities, Caddies.
    //      Clubs - You need a club to hit the ball. Click a club to select it for your next swing.
    //      Balls - Click a ball


    public GameObject[] panels; //Each panel is a page of the tutorial
    public int pageNum;
    public GameObject prevButton, nextButton, bgPanel;

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
        nextButton.GetComponent<Button>().interactable = true;
        prevButton.GetComponent<Button>().interactable = false;
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
        if (pageNum <= 1)
            prevButton.GetComponent<Button>().interactable = false;
        nextButton.GetComponent<Button>().interactable = true;
    }
}
