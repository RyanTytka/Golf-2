using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public GameObject msgPanel; //black background
    public GameObject msgTextObj;
    public GameObject continueButton; 

    //when the user clicks the tutorial button
    public void StartTutorial()
    {
        tutorialState = 1;
        Course c = gameObject.GetComponent<Course>();
    }

    //opens a black screen that displays a msg
    public void DisplayMessage(string msg)
    {
        msgPanel.SetActive(true);
        continueButton.SetActive(true);
        msgTextObj.GetComponent<TextMeshProUGUI>().text = msg;
    }

    //disable black panel
    public void HideMessage()
    {
        msgPanel.SetActive(false);
        continueButton.SetActive(false);
    }

    public void Update()
    {
        if(tutorialState == 1)
        {
            if(gameObject.GetComponent<Course>().gameState == Course.GameState.PLAYING)
            {
                tutorialState = 2;
                DisplayMessage("Drag the course or use arrow keys to look around the course");
            }
        }
    }
}
