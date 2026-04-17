using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class tutorialManager : MonoBehaviour
{
    //"Play through 5 courses of 9 holes each. If you dont score lower than your rival on each course, you lose."
    //"You start each hole with 5 cards in hand, plus your drivers. Each time you swing, you draw a card. Click the Mulligan button to draw a card, but it will cost you a stroke. Get the ball into the hole in as few swings as you can."
    //    "After each hole you can collect tees, based on how well you did, or add a new card to your deck. After each course, you can spend tees at the Pro Shop."
    //"There are 4 types of card. Clubs, Balls, Abilities, Caddies.
    //      Clubs - You need a club to hit the ball. Click a club to select it for your next swing.
                //There are 3 types of clubs: Woods, Irons, and Wedges. Woods are used for distance, and wedges are used for close range accuracy.
                //Some Woods are Drivers, which means they start in your hand but can only be used on your first shot.
    //      Balls - Click a ball to select it and give this swing special effects. No more than 1 ball can be used each swing.
    //      Abilities - Drag them to the middle of the screen to play them. They give you a one time bonus then are discarded. You can play as many abilities per turn as you want.
    //      Caddies - Drag them to the middle of the screen to play them. They stay in play for the rest of the hole, providing beneficial effects while in play.
    //"Each card has a rarity: Rookie, Pro, Legend. Spend tees at the Pro Shop to upgrade a card's rarity for more powerful effects.
    //Watch out for hazards on the course:
        //Rolling over rough reduces the power of your next shot.
        //Sand stops your ball and discards a random card from your hand.
        //Water stops your ball and costs you an extra stroke, then puts your ball on the next dry spot.
        //Once you make it to the green, you will automatically putt it into the hole to finish the hole. 
            // The further you are form the hole, the more putts it will take. Hold tab to show ???. Look at Meter.
    //This is your rival, each course has one. score and ability.
    //toss, backspin, power

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
        if (pageNum <= 0)
            prevButton.GetComponent<Button>().interactable = false;
        nextButton.GetComponent<Button>().interactable = true;
    }
}
