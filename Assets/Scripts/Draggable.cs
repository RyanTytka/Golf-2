using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class Draggable : MonoBehaviour
{
    //Card Stats
    public string cardName;
    public string description;
    public CardTypes cardType;
    public ClubTypes clubType;
    public int carry; //How far ball travels in air
    public int roll; //How far ball travels on ground
    public List<GameObject> upgrades;
    public GameObject titleObj; //reference to the card's name obj
    public GameObject descObj; //reference to the card's description obj
    public bool selected; //If I am currently selected to be played
    public bool draggable; //True = Drag to play. False = Click to select
    public bool isUpgradeOption = false; //set to true when this is being displayed as an upgrade option
    public bool isShopOption = false; //true if in shop able to buy
    public int myCost; //how many tees this costs to buy
    public GameObject costObj; //the price that is displayed while I am in the shop. Delete it when I am bought
    public bool isDriver = false;

    //Internal Helper Variables
    //these are used for when dragging
    private bool dragging = false;
    private float distance;
    private Vector3 startDist;
    private Vector3 startPos;
    private float returnDuration = 0.25f;
    private Coroutine returnCoroutine;
    private bool onDisplay = false; //true if this card is being displayed and not interactable
    private Vector3 displayPos; //where this should be displayed if onDisplay is true

    public enum CardTypes
    {
        Club,
        Ball,
        Caddie,
        Ability
    }
    public enum ClubTypes
    {
        Wood,
        Iron,
        Wedge,
        Putter
    }

    void OnMouseDown()
    {
        if (isUpgradeOption || isShopOption)
            return; 

        //start dragging this obj
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        dragging = true;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = ray.GetPoint(distance);
        startDist = transform.position - rayPoint;

        if (returnCoroutine == null)
            startPos = transform.position;

        // If it's currently returning, stop that coroutine
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        //below is only for select/deselect
        if (draggable || GameObject.Find("GameManager").GetComponent<Hand>().waitingForDiscard)
            return;
        //Chip Johnson cant use woods
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Chip Johnson") > 0 &&
            clubType == ClubTypes.Wood)
            return;

        selected = !selected;
        if (selected)
        {
            //If I am a club
            if (cardType == CardTypes.Club)
            {
                //deselect any current club
                if (GameObject.Find("CourseManager").GetComponent<Course>().selectedClub != null)
                {
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedClub.GetComponent<Draggable>().selected = false;
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedClub.GetComponent<SpriteRenderer>().color = Color.white;
                }
                //set me to be the current club
                GameObject.Find("CourseManager").GetComponent<Course>().selectedClub = this.gameObject;
            }
            //If I am a ball
            else if (cardType == CardTypes.Ball)
            {
                //deselect any current club
                if (GameObject.Find("CourseManager").GetComponent<Course>().selectedBall != null)
                {
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedBall.GetComponent<Draggable>().selected = false;
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedBall.GetComponent<SpriteRenderer>().color = Color.white;
                }
                //set me to be the current ball
                GameObject.Find("CourseManager").GetComponent<Course>().selectedBall = this.gameObject;
            }
            //Highlight me
            GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else
        {
            //I am unselected
            GetComponent<SpriteRenderer>().color = Color.white;
            if (cardType == CardTypes.Club)
            {
                GameObject.Find("CourseManager").GetComponent<Course>().selectedClub = null;
            }
            else if (cardType == CardTypes.Ball)
            {
                GameObject.Find("CourseManager").GetComponent<Course>().selectedBall = null;
            }
        }
        GameObject.Find("CourseManager").GetComponent<Course>().DisplayCourse();
    }

    void OnMouseUp()
    {
        if (isUpgradeOption)
        {
            //add this to your deck
            GameObject.Find("GameManager").GetComponent<Hand>().baseDeck.Add(this.gameObject);
            //delete other upgrade options
            GameObject.Find("GameManager").GetComponent<Hand>().DeleteOptions(this.gameObject);
            gameObject.SetActive(false);
            if (GameObject.Find("CourseManager").GetComponent<Course>().holeNum == 9)
            {
                //Go to the shop
                isUpgradeOption = false;
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene("Shop");
            }
            else
            {
                //Go back to the course, then create a new deck and hole
                isUpgradeOption = false;
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene("Course");
            }
            return;
        }
        if(isShopOption)
        {
            if (GameObject.Find("CourseManager").GetComponent<Course>().tees >= myCost)
            {
                //pay the cost
                GameObject.Find("CourseManager").GetComponent<Course>().tees -= myCost;
                //add this to your deck
                GameObject.Find("GameManager").GetComponent<Hand>().baseDeck.Add(this.gameObject);
                gameObject.SetActive(false);
                isShopOption = false;
                GameObject.Find("GameManager").GetComponent<Hand>().shopOptions.Remove(this.gameObject);
                //update shop UI
                GameObject.Find("TeesCount").GetComponent<TextMeshProUGUI>().text = 
                    GameObject.Find("CourseManager").GetComponent<Course>().tees.ToString();
                Destroy(costObj);
                return;
            }
        }
        dragging = false;
        if (draggable == false && GameObject.Find("GameManager").GetComponent<Hand>().waitingForDiscard == false)
            return;
        if (onDisplay)
            return;

        // Start coroutine to return to start position
        returnCoroutine = StartCoroutine(ReturnToStart());
        var hand = GameObject.Find("GameManager").GetComponent<Hand>();
        if (hand.waitingForDiscard)
        {
            //Cannot play cards while waiting for a discard
            //Check if in discard area
            if (transform.position.x > 4)
            {
                // If it's currently returning, stop that coroutine
                if (returnCoroutine != null)
                {
                    StopCoroutine(returnCoroutine);
                    returnCoroutine = null;
                }
                //discard this
                //if this is the active club or ball, deselect it first
                if (cardType == CardTypes.Club && selected)
                {
                    selected = false;
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedClub = null;
                }
                if (cardType == CardTypes.Ball && selected)
                {
                    selected = false;
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedBall = null;
                }
                hand.DiscardCard(this.gameObject);
                hand.waitingForDiscard = false;
            }
        }
        else
        {
            // If the card is dragged into the play area
            if (transform.position.y > 0)
            {
                if (cardType != CardTypes.Caddie ||
                    GameObject.Find("GameManager").GetComponent<Hand>().playedCaddie == false) //one caddie per turn
                {
                    if (cardType != CardTypes.Ability || GameObject.Find("GameManager").GetComponent<Hand>().playedAbility == false ||
                        GameObject.Find("CourseManager").GetComponent<Course>().currentRival != 3)
                    {
                        //play this
                        StartCoroutine(PlayCard());
                    }
                } 
            }
        }
    }

    void Update()
    {
        if(onDisplay)
        {
            transform.position = displayPos;
            return;
        }
        if (dragging && (draggable || GameObject.Find("GameManager").GetComponent<Hand>().waitingForDiscard))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);
            transform.position = rayPoint + startDist;
        }
    }

    private IEnumerator ReturnToStart()
    {
        Vector3 currentPos = transform.position;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            transform.position = Vector3.Lerp(currentPos, startPos, t);
            yield return null;
        }

        transform.position = startPos;
        returnCoroutine = null;
    }

    //Update Text on card
    public void UpdateCard()
    {
        titleObj.GetComponent<TextMeshProUGUI>().text = cardName;
        if(cardType == CardTypes.Club)
        {
            string s = "Carry: " + carry + "\nRoll: " + roll + "\n" + description;
            descObj.GetComponent<TextMeshProUGUI>().text = s;
        }
        else
        {
            descObj.GetComponent<TextMeshProUGUI>().text = description;
        }
    }

    //Play me
    public IEnumerator PlayCard()
    {
        //Debug.Log(cardName + " played");
        if (cardType == CardTypes.Caddie)
        {
            if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Caddie 2") > 0)
                GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(1);
            GameObject.Find("GameManager").GetComponent<Hand>().caddies.Add(this.gameObject);
            GameObject.Find("GameManager").GetComponent<Hand>().playedCaddie = true;
            //Caddies do not go into the discard
            GameObject.Find("GameManager").GetComponent<Hand>().hand.Remove(this.gameObject);
            this.gameObject.SetActive(false);
        }
        if (cardType == CardTypes.Ability)
            GameObject.Find("GameManager").GetComponent<Hand>().playedAbility = true;
        switch (cardName)
        {
            case "Rangefinder":
                // Wait for user to select a card to discard
                onDisplay = true;
                displayPos = new Vector3(-2, 1.5f, -2);
                // If it's currently returning, stop that coroutine
                if (returnCoroutine != null)
                {
                    StopCoroutine(returnCoroutine);
                    returnCoroutine = null;
                }
                //transform.localScale = Vector3.zero;
                yield return GameObject.Find("GameManager").GetComponent<Hand>().WaitForDiscard();
                //transform.localScale = Vector3.one;
                onDisplay = false;
                // Now draw 2 cards after discarding
                GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(2);
                break;
            case "Backspin":
                GameObject.Find("CourseManager").GetComponent<Course>().pinpoint += 30;
                break;
            case "Chip Johnson":
                //deselect selected club if its a woods
                if (GameObject.Find("CourseManager").GetComponent<Course>().selectedClub != null &&
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedClub.GetComponent<Draggable>().clubType == ClubTypes.Wood)
                {
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedClub.GetComponent<Draggable>().selected = false;
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedClub.GetComponent<SpriteRenderer>().color = Color.white;
                    GameObject.Find("CourseManager").GetComponent<Course>().selectedClub = null;
                }
                break;
            case "Dig Through Your Bag":
                //discard all non-clubs then draw 3
                List<GameObject> hand = GameObject.Find("GameManager").GetComponent<Hand>().hand;
                GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(3);
                for(int i = hand.Count - 1; i >= 0; i--)
                {
                    if(hand[i].GetComponent<Draggable>().cardType != CardTypes.Club)
                    {
                        GameObject.Find("GameManager").GetComponent<Hand>().DiscardCard(hand[i]);
                    }
                }
                break;
            case "Find a Ball":
                //draw 2 random balls from your deck
                List<GameObject> balls = new();
                foreach(GameObject go in GameObject.Find("GameManager").GetComponent<Hand>().currentDeck)
                {
                    //find all balls in player deck
                    if (go.GetComponent<Draggable>().cardType == CardTypes.Ball)
                        balls.Add(go);
                }
                List<GameObject> toDraw = new();
                while (toDraw.Count < 2 && toDraw.Count < balls.Count)
                {
                    //get 2 balls to draw
                    GameObject go = balls[Random.Range(0, balls.Count - 1)];
                    if (toDraw.Contains(go) == false)
                    {
                        toDraw.Add(go);
                    }
                }
                //draw the selected balls
                foreach (GameObject go in toDraw)
                {
                    GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(go);
                }
                break;
            case "Golf Glove":
                //Gain 1 Luck and draw a card
                GameObject.Find("CourseManager").GetComponent<Course>().luck++;
                GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(1);
                break;
            case "Phone a Friend":
                //add a random caddie to your hand
                List<GameObject> caddies = new();
                foreach (GameObject go in GameObject.Find("GameManager").GetComponent<Hand>().upgradeCards)
                {
                    //find all caddies available
                    if (go.GetComponent<Draggable>().cardType == CardTypes.Caddie)
                        caddies.Add(go);
                }
                //create new caddie card
                GameObject newCaddie = Instantiate(caddies[Random.Range(0, caddies.Count - 1)], GameObject.Find("GameManager").transform);
                GameObject.Find("GameManager").GetComponent<Hand>().hand.Add(newCaddie);
                break;
            case "Tee Up":
                foreach (GameObject go in GameObject.Find("GameManager").GetComponent<Hand>().drivers)
                {
                    //Create copy to add to hand
                    GameObject driver = Instantiate(go, GameObject.Find("GameManager").transform);
                    GameObject.Find("GameManager").GetComponent<Hand>().hand.Add(driver);
                }
                break;
        }
        //Update view
        GameObject.Find("CourseManager").GetComponent<Course>().DisplayCourse();
        GameObject.Find("CourseManager").GetComponent<Course>().UpdateStatusEffectDisplay();
        // Discard this card (caddies dont go to discard)
        if (cardType != CardTypes.Caddie)
            GameObject.Find("GameManager").GetComponent<Hand>().DiscardCard(this.gameObject);
        GameObject.Find("GameManager").GetComponent<Hand>().DisplayHand();
    }

    //This is called once the course scene is finished loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only run logic if it's the "Course" scene
        if (scene.name == "Course")
        {
            GameObject.Find("CourseManager").GetComponent<Course>().NewHole();
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
            Hand hand = GameObject.Find("GameManager").GetComponent<Hand>();
            List<GameObject> availableCards = hand.shopCards.OrderBy(x => Random.value).Take(3).ToList();
            GameObject costObj = GameObject.Find("TeesCost");
            int index = 0;
            foreach (GameObject card in availableCards)
            {
                //craete card obj
                GameObject newCard = Instantiate(card);
                hand.shopOptions.Add(newCard);
                newCard.transform.position = new Vector3(index * 5 - 5, 0, 0);
                newCard.GetComponent<Draggable>().isShopOption = true;
                newCard.GetComponent<Draggable>().UpdateCard();
                newCard.transform.parent = hand.gameObject.transform;
                //create cost obj
                GameObject newCost = Instantiate(costObj, GameObject.Find("MainCanvas").transform);
                newCost.GetComponent<RectTransform>().position = new Vector3(index * 5 - 5.25f, 2.25f, 0);
                newCost.GetComponentInChildren<TextMeshProUGUI>().text = newCard.GetComponent<Draggable>().myCost.ToString();
                newCard.GetComponent<Draggable>().costObj = newCost;
                index++;
            }
            Destroy(costObj);
            GameObject.Find("TeesCount").GetComponent<TextMeshProUGUI>().text =
                GameObject.Find("CourseManager").GetComponent<Course>().tees.ToString();
            GameObject.Find("ContinueButton").GetComponent<Button>().onClick.AddListener(hand.ToNewHole);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
