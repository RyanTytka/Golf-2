using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Runtime.InteropServices;

public class NewCardManager : MonoBehaviour
{
    public GameObject newCardCanvas;
    public GameObject addTeesText;
    public GameObject rerollCostText;
    public GameObject rerollButton;
    public GameObject yourTees;
    public GameObject takeTeesButton;
    public GameObject takeCardButton;
    public GameObject cardOption;
    public int currentRerollCost;
    private int currentTeeReward;

    public void ShowUI(int addTees, GameObject newCard)
    {
        currentRerollCost = 1;
        //rebind references
        newCardCanvas = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().newCardCanvas;
        addTeesText = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().addTeesText;
        rerollCostText = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().rerollCostText;
        rerollButton = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().rerollButton;
        yourTees = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().yourTees;
        takeCardButton = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().takeCardButton;
        takeTeesButton = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().takeTeesButton;
        //rebind onclicks for buttons
        rerollButton.GetComponent<Button>().onClick.RemoveAllListeners();
        rerollButton.GetComponent<Button>().onClick.AddListener(Reroll);
        takeCardButton.GetComponent<Button>().onClick.RemoveAllListeners();
        takeCardButton.GetComponent<Button>().onClick.AddListener(AddCardToDeck);
        takeTeesButton.GetComponent<Button>().onClick.RemoveAllListeners();
        takeTeesButton.GetComponent<Button>().onClick.AddListener(TakeTees);
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
        //make sure we have finished scrolling up before doing anything
        if (GameObject.Find("CourseManager").GetComponent<Course>().gamestate != Course.NEW_CARD_SELECT) return;
        GetComponent<Hand>().baseDeck.Add(cardOption);
        cardOption.transform.parent = GameObject.Find("BaseDeck").transform;
        GetComponent<Hand>().ClearUpgrades();
        cardOption.GetComponent<Draggable>().isUpgradeOption = false;
        cardOption.SetActive(false);
        Continue();
    }

    public void Reroll()
    {
        //make sure we have finished scrolling up before doing anything
        if (GameObject.Find("CourseManager").GetComponent<Course>().gamestate != Course.NEW_CARD_SELECT) return;
        //flip current card over then flip new card over
        Sequence seq = DOTween.Sequence();
        seq.Append(
            cardOption.transform.DOScaleX(0f, 0.5f).OnComplete(() =>
            {
                //delete old card
                Destroy(cardOption);
                //create new card
                GameObject newCard = GetComponent<Hand>().RandomUpgrade();
                cardOption = Instantiate(newCard);
                cardOption.transform.position = new Vector3(3.75f, 9, 0);
                cardOption.GetComponent<Draggable>().isUpgradeOption = true;
                cardOption.GetComponent<Draggable>().UpdateCard();
                cardOption.transform.parent = gameObject.transform;
                //start face down
                //cardOption.GetComponent<Draggable>().cardBack.SetActive(true);
                //cardOption.GetComponent<Draggable>().typeIconObj.SetActive(false);
                cardOption.transform.localScale = new Vector3(0, 1, 1);
                cardOption.transform.DOScaleX(1f, 0.5f).OnComplete(() =>
                {
                    //spend tees
                    GameObject.Find("CourseManager").GetComponent<Course>().tees -= currentRerollCost;
                    yourTees.GetComponent<TextMeshProUGUI>().text = GameObject.Find("CourseManager").GetComponent<Course>().tees.ToString();
                    //update reroll cost
                    currentRerollCost++;
                    rerollButton.GetComponent<Button>().interactable = GameObject.Find("CourseManager").GetComponent<Course>().tees > currentRerollCost;
                    rerollCostText.GetComponent<TextMeshProUGUI>().text = "Cost: " + currentRerollCost.ToString();
                });
            }
        ));
    }

    public void TakeTees()
    {
        //make sure we have finished scrolling up before doing anything
        if (GameObject.Find("CourseManager").GetComponent<Course>().gamestate != Course.NEW_CARD_SELECT) return;
        //add tees
        GameObject.Find("CourseManager").GetComponent<Course>().tees += currentTeeReward;
        //delete upgrade option
        Destroy(cardOption);
        cardOption = null;
        //continue
        Continue();
    }

    //helper method that cleans up and goes to next hole/shop
    private void Continue()
    {
        Course c = GameObject.Find("CourseManager").GetComponent<Course>();
        //If just finished a course go to the shop
        if (c.holeNum >= 9)
        {
            c.gameState = Course.GameState.SHOP;
            //clear last hole
            foreach (GameObject go in c.courseLayout)
            {
                Destroy(go);
            }
            c.courseLayout.Clear();
            c.ballObj.SetActive(false);
            //reset shot highlight
            GameObject.Find("CourseManager").GetComponent<LineRenderer>().positionCount = 0;
            if (c.currentDot != null)
                Destroy(c.currentDot);
            //clear continue text/button
            Destroy(c.continueObj);
            //clear bg elements
            GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().RemoveSprites();
            //go to shop
            SceneManager.LoadScene("Shop");
        }
        else
        {
            c.gameState = Course.GameState.PLAYING;
            //otherwise, go to next hole
            //clear last hole
            foreach (GameObject go in c.courseLayout)
            {
                Destroy(go);
            }
            c.courseLayout.Clear();
            //reset shot highlight
            GameObject.Find("CourseManager").GetComponent<LineRenderer>().positionCount = 0;
            if (c.currentDot != null)
                Destroy(c.currentDot);
            //clear continue text/button
            Destroy(c.continueObj);
            //clear bg elements
            GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().RemoveSprites();
            //new hole
            c.NewHole();
            //updatestatus effects
            c.UpdateStatusEffectDisplay();
            //reset deck
            GetComponent<Hand>().NewDeck();
            //scroll down
            GetComponent<mainMenuUI>().ScrollDown();
        }
    }
}
