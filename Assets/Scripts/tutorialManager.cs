using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class tutorialManager : MonoBehaviour
{
    public int tutorialState; //increments as player progesses through tutorial
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

    public GameObject msgPanel; //black background
    public GameObject msgTextObj;
    public GameObject continueButton;
    public bool disableHandSelection = false;

    //when the user clicks the tutorial button
    public void StartTutorial()
    {
        tutorialState = 1;
    }

    //opens a black screen that displays a msg
    public void DisplayMessage(string msg)
    {
        msgPanel.SetActive(true);
        //continueButton.SetActive(true);
        msgTextObj.GetComponent<TextMeshProUGUI>().text = msg;
    }

    //disable black panel
    public void HideMessage()
    {
        disableHandSelection = false;
        msgPanel.SetActive(false);
        //continueButton.SetActive(false);
    }

    public void Update()
    {
        if(tutorialState == 1)
        {
            if(gameObject.GetComponent<Course>().gameState == Course.GameState.PLAYING)
            {
                disableHandSelection = true;
                DisplayMessage("Drag the course or use arrow keys to look around the course");
                //msgPanel.GetComponent<Canvas>().sortingOrder = 200;
                continueButton.GetComponent<Button>().onClick.RemoveAllListeners();
                continueButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    tutorialState = 2;
                });
            }
        }
        if(tutorialState == 2)
        {
            DisplayMessage("Click a club in your hand to select it. Then click the SWING button to hit the ball.");
            continueButton.GetComponent<Button>().onClick.RemoveAllListeners();
            continueButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                tutorialState = 3;
            });
        }
        if (tutorialState == 3)
        {
            HideMessage();
        }
    }
}
