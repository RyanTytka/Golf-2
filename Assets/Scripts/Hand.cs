using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public List<GameObject> starterCards; //prefabs for cards in starter deck
    public List<GameObject> baseDeck = new(); //Master copy of your deck
    public List<GameObject> currentDeck = new(); //Deck during the hole that is manipulated and used
    public List<GameObject> discardPile = new();
    public List<GameObject> upgradeCards; //cards that can be picked as upgrades
    public List<GameObject> putters;
    public List<GameObject> caddies; //played caddie cards
    public bool playedCaddie = false; //can only play 1 per turn
    public GameObject caddieDisplayObj; //prefab to be instantiated
    public List<GameObject> caddieDisplays; //list of current objs
    public bool playedAbility;
    public bool paused = false;

    public void StartGame()
    {
        //create instances of starter cards
        foreach (GameObject go in starterCards)
        {
            GameObject newObj = Instantiate(go, GameObject.Find("GameManager").transform.Find("BaseDeck").transform);
            baseDeck.Add(newObj);
            newObj.SetActive(false);
        }
        //set up initial hand
        RemoveDeck();
        NewDeck();
    }

    //create new currentDeck and draw starting hand
    public void NewDeck()
    {
        //draw drivers seperately so it can be done one at a time
        List<GameObject> drivers = new();
        //copy base deck into a new deck for player to use
        foreach (GameObject go in baseDeck)
        {
            GameObject newObj = Instantiate(go, GameObject.Find("GameManager").transform.Find("CurrentDeck").transform);
            newObj.GetComponent<Draggable>().baseReference = go;
            if (newObj.GetComponent<Draggable>().isDriver)
            {
                if (GameObject.Find("CourseManager").GetComponent<Course>().pars[GameObject.Find("CourseManager").GetComponent<Course>().holeNum - 1] == 3)
                {
                    //par 3s do not get drivers
                    Destroy(newObj);
                }
                else
                {
                    //drivers are set aside then put onto deck
                    drivers.Add(newObj);
                }
            }
            else
            {
                currentDeck.Add(newObj);
            }
        }
        ShuffleDeck();
        //add drivers to the top of the deck. Not included instarting hand size
            currentDeck.InsertRange(0, drivers);
        foreach(GameObject driver in drivers)
        {
        }
        //Draw starting hand
        if (GameObject.Find("CourseManager").GetComponent<Course>().currentRival == 2)
            DrawCard(3 + drivers.Count);
        else
            DrawCard(4 + drivers.Count);
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
        //control variables
        float cardWidth = 2f;
        float handWidth = 10f;
        float maxFanAngle = 5f;
        float curveHeight = 0.25f;
        float moveDuration = 0.35f;
        float xPos = -4f;
        float yPos = -3.0f;

        //display cards fanned out
        int count = hand.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            hand[i].SetActive(true);
            hand[i].GetComponent<Draggable>().UpdateCard();
            float t = (count == 1) ? 0f : (float)i / (count - 1);
            // Horizontal spread
            float x = i * cardWidth;
            if (cardWidth * count > handWidth)
            {
                x = t * handWidth;
            }
            // Fan rotation
            float angle = Mathf.Lerp(maxFanAngle, -maxFanAngle, t);
            // Vertical curve (parabola)
            float y = Mathf.Sin(t * Mathf.PI) * curveHeight;
            if (hand[i].GetComponent<Draggable>().selected)
            {
                y += 1f;
            }
            Vector3 targetPos = new(x + xPos, y + yPos, 0);
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            Transform card = hand[i].transform;
            //move cards
            card.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad);
            card.DORotateQuaternion(targetRot, moveDuration);
            // Sorting so middle cards appear on top
            card.GetComponentInChildren<Canvas>().sortingOrder = i + 100;
            card.GetComponent<SpriteRenderer>().sortingOrder = i + 100;
        }
        //clear current caddies
        foreach (GameObject go in caddieDisplays)
        {
            Destroy(go);
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
        if (GameObject.Find("DeckCount") != null)
            GameObject.Find("DeckCount").GetComponent<TextMeshProUGUI>().text = currentDeck.Count.ToString();
    }

    //Draw 'amount' cards to your hand
    public void DrawCard(int amount)
    {
        //draw card
        //for (int i = 0; i < amount; i++)
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
            GameObject drawnCard = currentDeck[0];
            drawnCard.GetComponent<Draggable>().UpdateCard();
            currentDeck[0].SetActive(true);
            currentDeck.RemoveAt(0);
            if (currentDeck.Count == 0 && GameObject.Find("CourseManager").GetComponent<Course>().currentRival == 5)
                GameObject.Find("CourseManager").GetComponent<Course>().strokeCount++;
            //animate drawing the card
            drawnCard.GetComponent<Draggable>().AnimateDraw(amount);
        }
        //DisplayHand(); this is done when the draw animation completes
    }

    //draws a specific card
    public void DrawCard(GameObject card)
    {
        if (currentDeck.Contains(card))
        {
            currentDeck.Remove(card);
        }
        //animate drawing the card
        card.GetComponent<Draggable>().AnimateDraw(1);
    }

    //removes a card from your deck (not the base deck)
    public void Toss(GameObject card)
    {
        if (currentDeck.Contains(card))
        {
            currentDeck.Remove(card);
        }
        if (hand.Contains(card))
        {
            hand.Remove(card);
        }
        if (discardPile.Contains(card))
        {
            discardPile.Remove(card);
        }
        baseDeck.Remove(card.GetComponent<Draggable>().baseReference);
        Destroy(card.GetComponent<Draggable>().baseReference);
        Destroy(card);
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
        if (card == null) return;
        card.GetComponent<Draggable>().AnimateDiscard();
    }

    //private bool cardChosen = false;
    //private GameObject selectedToDiscard = null;
    public bool waitingForDiscard = false;
    public GameObject instructionTextObj;

    public IEnumerator WaitForDiscard()
    {
        GameObject go = Instantiate(instructionTextObj, GameObject.Find("MainCanvas").transform);
        go.transform.position = new Vector3(-75, 50, 100);
        go.SetActive(true);
        go.GetComponentInChildren<TextMeshProUGUI>().text = "Drag a card here to discard it";
        waitingForDiscard = true;
        while (waitingForDiscard)
            yield return null;
        go.SetActive(false);
    }

    //returns a random upgrade card
    private List<int> upgradeOptions = new();
    public GameObject RandomUpgrade()
    {
        //create new order of upgrades if it is currently empty
        if (upgradeOptions.Count == 0)
            upgradeOptions = Enumerable.Range(0, upgradeCards.Count)
                        .OrderBy(i => Random.value)
                        .ToList();
        //pop the last upgrade and return it
        int num = upgradeOptions[0];
        upgradeOptions.RemoveAt(0);
        return upgradeCards[num];
    }

    //resets the list of cards that have been presented to the player
    public void ClearUpgrades()
    {
        upgradeOptions.Clear();
    }

    //private List<GameObject> upgradeOptions = new();
    //public void CreateNewCardOptions()
    //{
    //    for (int i = 0; i < 3; i++)
    //    {
    //        int choice = Random.Range(0, upgradeCards.Count );
    //        GameObject go = Instantiate(upgradeCards[choice]);
    //        upgradeOptions.Add(go);
    //        go.transform.position = new Vector3(i * 5 - 5, 0, 0);
    //        go.GetComponent<Draggable>().isUpgradeOption = true;
    //        go.GetComponent<Draggable>().UpdateCard();
    //        go.transform.parent = gameObject.transform;
    //    }
    //}

    ////delete all upgrade options besides pickedObj
    //public void DeleteOptions(GameObject pickedObj)
    //{
    //    foreach(GameObject go in upgradeOptions)
    //    {
    //        if(go != pickedObj)
    //            Destroy(go);
    //    }
    //    upgradeOptions.Clear();
    //}

    public void SkipUpgrade()
    {
        //get tees for skipping
        Course course = GameObject.Find("CourseManager").GetComponent<Course>();
        if (course.currentRival != 4 || course.pars[course.holeNum - 1] >= course.scores[course.holeNum - 1]) //must par or better to get tees against rival 4
            course.tees += Mathf.Max(course.pars[course.holeNum - 1] - course.strokeCount + 3, 1);
        //DeleteOptions(null);
        //Go back to the course and create a new deck and hole, or go to shop if hole 9
        if (GameObject.Find("CourseManager").GetComponent<Course>().holeNum >= 9)
        {
            SceneManager.LoadScene("Shop");
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Course");
        }
    }

    //This is called once the scene is finished loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only run logic if it's the "Course" scene
        if (scene.name == "Course")
        {
            Course course = GameObject.Find("CourseManager").GetComponent<Course>();
            if (course.comingFromShop)
            {
                //start new course
                course.NewCourse();
                course.comingFromShop = false;
            }
            else
            {
                //just need a new hole
                course.NewHole();
            }
            //set up hand and UI
            GameObject.Find("GameManager").GetComponent<Hand>().NewDeck();
            GameObject.Find("SwingButton").GetComponent<Button>().onClick.AddListener
                (course.Swing);
            GameObject.Find("MulliganButton").GetComponent<Button>().onClick.AddListener
                (course.Mulligan);
            // Unsubscribe to avoid duplicate calls in the future
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    //returns # of caddies with "name" in play 
    public int HasCaddie(string name)
    {
        int amount = 0;
        foreach (GameObject go in caddies)
        {
            if (go.GetComponent<Draggable>().cardName == name)
                amount++;
        }
        return amount;
    }

    //removes a card from the base deck based on id
    public void RemoveCardById(int id)
    {
        foreach (GameObject card in baseDeck)
        {
            if (card.GetComponent<Draggable>().cardId == id)
            {
                baseDeck.Remove(card);
                Destroy(card);
                return;
            }
        }
    }

    public GameObject GetCardById(int id)
    {
        foreach (GameObject card in baseDeck)
        {
            if (card.GetComponent<Draggable>().cardId == id)
            {
                return card;
            }
        }
        return null;
    }

    private GameObject hoveringCard;
    private GameObject draggingCard;
    void Update()
    {
        //check the topmost object the mouse is on
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorld, Vector2.zero);
        if (hits.Length == 0)
        {
            if (hoveringCard != null)
            {
                //stop hovering
                hoveringCard.transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
                hoveringCard = null;
                DisplayHand();
            }
            return;
        }
        RaycastHit2D topHit = hits
            .OrderByDescending(h =>
                h.collider.GetComponent<SpriteRenderer>() != null ?
                    h.collider.GetComponent<SpriteRenderer>().sortingOrder : int.MinValue
            )
            .First();
        if (hoveringCard != null && hoveringCard != topHit.collider.gameObject)
        {
            //stop hovering
            hoveringCard.transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
            hoveringCard = null;
            DisplayHand();
        }
        if (topHit.collider.GetComponent<Draggable>() != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //click the card
                draggingCard = topHit.collider.gameObject;
                draggingCard.GetComponent<Draggable>().ClickCard();
            }
            else
            {
                if (hoveringCard == null)
                {
                    if (topHit.collider.GetComponent<Draggable>().isUpgradeOption == false)
                    {
                        if (topHit.collider.gameObject.GetComponent<Draggable>().isHoverable)
                        {
                            //start hovering the card
                            hoveringCard = topHit.collider.gameObject;
                            hoveringCard.GetComponentInChildren<Canvas>().sortingOrder = 1000;
                            hoveringCard.GetComponent<SpriteRenderer>().sortingOrder = 1000;
                            hoveringCard.transform.DOKill();
                            hoveringCard.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutQuad);
                            hoveringCard.transform.DOLocalMoveY(hoveringCard.transform.localPosition.y + 0.25f, 0.15f).SetEase(Ease.OutQuad);
                        }
                    }
                }
            }
        }
        //check if we need to release dragging a card
        if (Input.GetMouseButtonUp(0))
        {
            if (draggingCard != null)
            {
                draggingCard.GetComponent<Draggable>().MouseReleased();
                draggingCard = null;
            }
        }
    }
}
