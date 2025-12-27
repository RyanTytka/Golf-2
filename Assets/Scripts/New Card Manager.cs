using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewCardManager : MonoBehaviour
{
    public GameObject newCardCanvas;
    public GameObject addTeesText;
    public GameObject rerollCostText;
    public GameObject cardOption;
    public GameObject rerollButton;
    public GameObject yourTees;
    public int currentRerollCost;
    private int currentTeeReward;

    public void ShowUI(int addTees, GameObject newCard)
    {
        currentRerollCost = 1;
        //update ui elements
        newCardCanvas.SetActive(true);
        rerollCostText.GetComponent<TextMeshProUGUI>().text = "Cost: " + currentRerollCost.ToString();
        addTeesText.GetComponent<TextMeshProUGUI>().text = addTees.ToString();
        currentTeeReward = addTees;
        rerollButton.GetComponent<Button>().interactable = GameObject.Find("CourseManager").GetComponent<Course>().tees > currentRerollCost;
        yourTees.GetComponent<TextMeshProUGUI>().text = GameObject.Find("CourseManager").GetComponent<Course>().tees.ToString();
        //create card option
        cardOption = Instantiate(newCard);
        cardOption.transform.position = new Vector3(3.75f, 9, 0);
        cardOption.GetComponent<Draggable>().isUpgradeOption = true;
        cardOption.GetComponent<Draggable>().UpdateCard();
        cardOption.transform.parent = gameObject.transform;
    }

    //move upgrade card to the deck obj and hide it.then continue to next hole/shop
    public void AddCardToDeck()
    {
        GetComponent<Hand>().baseDeck.Add(cardOption);
        cardOption.transform.parent = GameObject.Find("BaseDeck").transform;
        GetComponent<Hand>().ClearUpgrades();
        cardOption.GetComponent<Draggable>().isUpgradeOption = false;
        cardOption.SetActive(false);
        if (GameObject.Find("CourseManager").GetComponent<Course>().holeNum == 9)
        {
            //Go to the shop
            SceneManager.LoadScene("Shop");
        }
        else
        {
            //clear last hole
            foreach (GameObject go in GameObject.Find("CourseManager").GetComponent<Course>().courseLayout)
            {
                Destroy(go);
            }
            GameObject.Find("CourseManager").GetComponent<Course>().courseLayout.Clear();
            //reset shot highlight
            GameObject.Find("CourseManager").GetComponent<LineRenderer>().positionCount = 0;
            if (GameObject.Find("CourseManager").GetComponent<Course>().currentDot != null)
                Destroy(GameObject.Find("CourseManager").GetComponent<Course>().currentDot);
            //clear continue text/button
            Destroy(GameObject.Find("CourseManager").GetComponent<Course>().continueObj);
            //clear bg elements
            GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().RemoveSprites();
            //new hole
            GameObject.Find("CourseManager").GetComponent<Course>().NewHole();
            //updatestatus effects
            GameObject.Find("CourseManager").GetComponent<Course>().UpdateStatusEffectDisplay();
            //reset deck
            GetComponent<Hand>().NewDeck();
            //scroll down
            GetComponent<mainMenuUI>().ScrollDown();
        }
    }

    public void Reroll()
    {
        //spend tees
        GameObject.Find("CourseManager").GetComponent<Course>().tees -= currentRerollCost;
        yourTees.GetComponent<TextMeshProUGUI>().text = GameObject.Find("CourseManager").GetComponent<Course>().tees.ToString();
        //update reroll cost
        currentRerollCost++;
        rerollButton.GetComponent<Button>().interactable = GameObject.Find("CourseManager").GetComponent<Course>().tees > currentRerollCost;
        rerollCostText.GetComponent<TextMeshProUGUI>().text = "Cost: " + currentRerollCost.ToString();
        //delete old card
        Destroy(cardOption);
        //create new card option
        GameObject newCard = GetComponent<Hand>().RandomUpgrade();
        cardOption = Instantiate(newCard);
        cardOption.transform.position = new Vector3(3.75f, 9, 0);
        cardOption.GetComponent<Draggable>().isUpgradeOption = true;
        cardOption.GetComponent<Draggable>().UpdateCard();
        cardOption.transform.parent = gameObject.transform;
    }

    public void TakeTees()
    {
        //add tees
        GameObject.Find("CourseManager").GetComponent<Course>().tees += currentTeeReward;
        //delete upgrade option
        Destroy(cardOption);
        cardOption = null;
        //continue
        if (GameObject.Find("CourseManager").GetComponent<Course>().holeNum == 9)
        {
            //Go to the shop
            SceneManager.LoadScene("Shop");
        }
        else
        {
            //clear last hole
            foreach (GameObject go in GameObject.Find("CourseManager").GetComponent<Course>().courseLayout)
            {
                Destroy(go);
            }
            GameObject.Find("CourseManager").GetComponent<Course>().courseLayout.Clear();
            //reset shot highlight
            GameObject.Find("CourseManager").GetComponent<LineRenderer>().positionCount = 0;
            if (GameObject.Find("CourseManager").GetComponent<Course>().currentDot != null)
                Destroy(GameObject.Find("CourseManager").GetComponent<Course>().currentDot);
            //clear continue text/button
            Destroy(GameObject.Find("CourseManager").GetComponent<Course>().continueObj);
            //clear bg elements
            GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().RemoveSprites();
            //new hole
            GameObject.Find("CourseManager").GetComponent<Course>().NewHole();
            //updatestatus effects
            GameObject.Find("CourseManager").GetComponent<Course>().UpdateStatusEffectDisplay();
            //reset deck
            GetComponent<Hand>().NewDeck();
            //scroll down
            GetComponent<mainMenuUI>().ScrollDown();
        }
    }
}
