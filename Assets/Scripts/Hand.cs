using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;


public class Hand : MonoBehaviour
{
    //prevent duplicates of this obj
    public static GameObject handManagerObj;
    public void Awake()
    {
        if (handManagerObj == null)
        {
            handManagerObj = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public List<GameObject> hand = new();
    public List<GameObject> baseDeck = new(); //Master copy of your deck
    public List<GameObject> currentDeck = new(); //Deck during the hole that is manipulated and used
    public List<GameObject> discardPile = new();
    public List<GameObject> upgradeCards; //cards that can be picked as upgrades
    public List<GameObject> shopCards; //cards that can be bought in the shop
    public List<GameObject> shopOptions;
    public GameObject upgradePrefab; //button to buy upgrade created in shop
    public List<GameObject> drivers;
    public List<GameObject> putters;
    public List<GameObject> caddies; //played caddie cards
    public bool playedCaddie = false; //can only play 1 per turn
    public GameObject caddieDisplayObj; //prefab to be instantiated
    public List<GameObject> caddieDisplays; //list of current objs
    public bool playedAbility;

    public void StartGame()
    {
        RemoveDeck();
        NewDeck();
    }

    //create new currentDeck and draw starting hand
    public void NewDeck()
    {
        //copy base deck into a new deck for player to use
        foreach (GameObject go in baseDeck)
        {
            GameObject newObj = Instantiate(go, GameObject.Find("GameManager").transform);
            currentDeck.Add(newObj);
        }
        ShuffleDeck();
        //Add Drivers to hand
        foreach (GameObject driver in drivers)
        {
            GameObject go = Instantiate(driver, GameObject.Find("GameManager").transform);
            hand.Add(go);
        }
        //Draw starting hand
        if (GameObject.Find("CourseManager").GetComponent<Course>().currentRival == 2)
            DrawCard(3);
        else
            DrawCard(4);
        DisplayHand();
    }

    //Delete all cards in hand, deck, and discard
    public void RemoveDeck()
    {
        foreach (GameObject go in currentDeck)
        {
            Destroy(go);
        }
        foreach (GameObject go in hand)
        {
            Destroy(go);
        }
        foreach (GameObject go in discardPile)
        {
            Destroy(go);
        }
        foreach (GameObject go in caddies)
        {
            Destroy(go);
        }
        currentDeck = new List<GameObject>();
        hand = new List<GameObject>();
        discardPile = new List<GameObject>();
        caddies = new List<GameObject>();
    }

    public void DisplayHand()
    {
        //Hide deck and discard
        foreach(GameObject go in currentDeck)
        {
            go.transform.position = new Vector3(-99, 99, 0);
        }
        foreach (GameObject go in discardPile)
        {
            go.transform.position = new Vector3(99, 99, 0);
        }
        //draw hand
        for (int i = 0; i < hand.Count; i++)
        {
            GameObject go = hand[i];
            go.transform.position = new Vector3(i * 2.15f - 5.75f, -2.5f, -2);
            go.transform.localScale = new Vector3(.85f,.85f,.85f);
            go.GetComponent<Draggable>().UpdateCard();
        }
        //clear current caddies
        foreach(GameObject go in caddieDisplays)
        {
            Destroy (go);
        }
        caddieDisplays.Clear();
        //draw caddies
        foreach (GameObject go in caddies)
        {
            GameObject newCaddie = Instantiate(caddieDisplayObj, GameObject.Find("MainCanvas").transform);
            newCaddie.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300 + caddieDisplays.Count * 75, 150, 0);
            newCaddie.GetComponent<caddieDisplay>().caddieRef = go;
            caddieDisplays.Add(newCaddie);
        }
        //Update deck count
        if(GameObject.Find("DeckCount") != null)
            GameObject.Find("DeckCount").GetComponent<TextMeshProUGUI>().text = currentDeck.Count.ToString();
    }

    //Draw 'amount' cards to your hand
    public void DrawCard(int amount)
    {
        //draw card
        for (int i = 0; i < amount; i++)
        {
            //if deck is empty, reshuffle
            if (currentDeck.Count == 0)
            {
                foreach (GameObject card in discardPile)
                {
                    currentDeck.Add(card);
                }
                discardPile.Clear();
                ShuffleDeck();
            }
            //draw a card
            if (currentDeck.Count == 0)
                return; //if both deck and discard are empty, do nothing
            hand.Add(currentDeck[0]);
            currentDeck[0].SetActive(true);
            currentDeck.RemoveAt(0);
        }
        DisplayHand();
    }

    //draws a specific card
    public void DrawCard(GameObject card)
    {
        if(currentDeck.Contains(card))
        {
            currentDeck.Remove(card);
            hand.Add(card);
            card.SetActive(true);
        }
        DisplayHand();
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < currentDeck.Count; i++)
        {
            int randIndex = Random.Range(i, currentDeck.Count);
            GameObject temp = currentDeck[i];
            currentDeck[i] = currentDeck[randIndex];
            currentDeck[randIndex] = temp;
        }
    }

    public void DiscardCard(GameObject card)
    {
        Debug.Log("discarded" + card.name);
        card.SetActive(false);
        hand.Remove(card);
        discardPile.Add(card);
        DisplayHand();
    }

    //private bool cardChosen = false;
    //private GameObject selectedToDiscard = null;
    public bool waitingForDiscard = false;
    public GameObject instructionTextObj;

    public IEnumerator WaitForDiscard()
    {
        GameObject go = Instantiate(instructionTextObj, GameObject.Find("MainCanvas").transform);
        go.SetActive(true);
        go.GetComponentInChildren<TextMeshProUGUI>().text = "Drag a card here to discard it";
        waitingForDiscard = true;
        while (waitingForDiscard)
            yield return null;
        go.SetActive(false);
    }

    private List<GameObject> upgradeOptions = new();
    public void CreateNewCardOptions()
    {
        for (int i = 0; i < 3; i++)
        {
            int choice = Random.Range(0, upgradeCards.Count - 1);
            GameObject go = Instantiate(upgradeCards[choice]);
            upgradeOptions.Add(go);
            go.transform.position = new Vector3(i * 5 - 5, 0, 0);
            go.GetComponent<Draggable>().isUpgradeOption = true;
            go.GetComponent<Draggable>().UpdateCard();
            go.transform.parent = gameObject.transform;
        }
    }

    //delete all upgrade options besides pickedObj
    public void DeleteOptions(GameObject pickedObj)
    {
        foreach(GameObject go in upgradeOptions)
        {
            if(go != pickedObj)
                Destroy(go);
        }
        upgradeOptions.Clear();
    }

    public void SkipUpgrade()
    {
        //get tees for skipping
        Course course = GameObject.Find("CourseManager").GetComponent<Course>();
        course.tees += Mathf.Max(course.pars[course.holeNum - 1] - course.strokeCount + 3, 1);
        GameObject.Find("GameManager").GetComponent<Hand>().DeleteOptions(this.gameObject);
        //Go back to the course and create a new deck and hole, or go to shop if hole 9
        SceneManager.sceneLoaded += OnSceneLoaded;
        if(GameObject.Find("CourseManager").GetComponent<Course>().holeNum >= 9)
        {
            //check if player won or lost
            if(true)
                SceneManager.LoadScene("Shop");
            else
                SceneManager.LoadScene("Shop");
        }
        else
            SceneManager.LoadScene("Course");
    }

    //This is called once the scene is finished loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only run logic if it's the "Course" scene
        if (scene.name == "Course")
        {
            if(GameObject.Find("CourseManager").GetComponent<Course>().comingFromShop)
            {
                //start new course
                GameObject.Find("CourseManager").GetComponent<Course>().NewCourse();
                GameObject.Find("CourseManager").GetComponent<Course>().comingFromShop = false;
            }
            else
            {
                //just need a new hole
                GameObject.Find("CourseManager").GetComponent<Course>().NewHole();
            }
            //set up hand and UI
            GameObject.Find("GameManager").GetComponent<Hand>().NewDeck();
            GameObject.Find("SwingButton").GetComponent<Button>().onClick.AddListener
                (GameObject.Find("CourseManager").GetComponent<Course>().Swing);
            GameObject.Find("MulliganButton").GetComponent<Button>().onClick.AddListener
                (GameObject.Find("CourseManager").GetComponent<Course>().Mulligan);
            // Unsubscribe to avoid duplicate calls in the future
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        if (scene.name == "Shop")
        {
            //generate shop items
            List<GameObject> availableCards = shopCards.OrderBy(x => Random.value).Take(3).ToList();
            GameObject costObj = GameObject.Find("TeesCost");
            int index = 0;
            //new cards
            foreach (GameObject card in availableCards)
            {
                //create card objs
                GameObject newCard = Instantiate(card);
                shopOptions.Add(newCard);
                newCard.transform.position = new Vector3(index * 5 - 5, -1.5f, 0);
                newCard.GetComponent<Draggable>().isShopOption = true;
                newCard.GetComponent<Draggable>().UpdateCard();
                newCard.transform.parent = gameObject.transform;
                //create cost objs
                GameObject newCost = Instantiate(costObj, GameObject.Find("MainCanvas").transform);
                newCost.GetComponent<RectTransform>().position = new Vector3(index * 5 - 5.25f, 0.75f, 0);
                newCost.GetComponentInChildren<TextMeshProUGUI>().text = newCard.GetComponent<Draggable>().myCost.ToString();
                newCard.GetComponent<Draggable>().costObj = newCost;
                index++;
            }
            //upgrades
            for (int i = 0; i < 3; i++)
            {
                //create upgrade options
                GameObject newUpgrade = Instantiate(upgradePrefab, GameObject.Find("MainCanvas").transform);
                newUpgrade.GetComponent<RectTransform>().position = new Vector3(i * 5 - 5.0f, 2.5f, 0);
                newUpgrade.GetComponent<upgradeBuy>().type = (upgradeBuy.upgradeType)Random.Range(0, 3);
                newUpgrade.GetComponent<upgradeBuy>().ID = Random.Range(0, 4);
            }
            Destroy(costObj);
            GameObject.Find("TeesCount").GetComponent<TextMeshProUGUI>().text =
                GameObject.Find("CourseManager").GetComponent<Course>().tees.ToString();
            GameObject.Find("ContinueButton").GetComponent<Button>().onClick.AddListener(ToNewHole);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void ToNewHole()
    {
        //Destroy Shop Items
        foreach(GameObject go in shopOptions)
        {
            Destroy(go);
        }
        shopOptions.Clear();
        //Load course scene
        GameObject.Find("CourseManager").GetComponent<Course>().comingFromShop = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Course");
    }

    //returns # of caddies with "name" in play 
    public int HasCaddie(string name)
    {
        int amount = 0;
        foreach(GameObject go in caddies)
        {
            if(go.GetComponent<Draggable>().cardName == name)
                amount++;
        }    
        return amount;
    }
}
