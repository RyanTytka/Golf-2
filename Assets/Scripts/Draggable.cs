using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Draggable : MonoBehaviour
{
    //Card Stats
    public string cardName;
    public string description;
    public CardTypes cardType;
    public ClubTypes clubType;
    [SerializeField]
    int carry; //How far ball travels in air
    [SerializeField]
    int roll; //How far ball travels on ground
    public int Carry
    {
        get
        {
            GetComponent<upgrades>().UpdateView();
            if (cardName == "Lone Ranger")
                carry -= GameObject.Find("GameManager").GetComponent<Hand>().hand.Count - 1;
            return carry + GetComponent<upgrades>().Stats()[0];
        }
    }
    public int Roll
    {
        get
        {
            GetComponent<upgrades>().UpdateView();
            return roll + GetComponent<upgrades>().Stats()[1];
        }
    }
    public GameObject titleObj; //reference to the card's name obj
    public GameObject descObj; //reference to the card's description obj
    public GameObject rollTextObj, carryTextObj;
    public GameObject clubStatBlock; //top of description box that shows carry and roll icons for clubs
    public GameObject typeIconObj;
    public Sprite[] typeIcons; //list of icons for each type of card
    public bool selected; //If I am currently selected to be played
    public bool draggable; //True = Drag to play. False = Click to select
    public bool isUpgradeOption = false; //set to true when this is being displayed as an upgrade option
    public bool isShopOption = false; //true if in shop able to buy
    public int myCost; //how many tees this costs to buy
    public GameObject costObj; //the price that is displayed while I am in the shop. Delete it when I am bought
    public bool isDriver = false;
    public bool isDeckView = false;
    public bool isRemoveView = false; //true if selecting a card to remove from deck
    public bool isUpgradeView = false; //true if selecting a card to upgrade
    public bool isPreviewing = false; //true if this is the card you are looking at to remove
    public int cardId; //index of the deck list. used to identify cards
    public GameObject baseReference; //reference to the obj this is cloned from in the base deck
    public GameObject cardBack; //card back that is shown when card is face down and disabled when face up
    public bool isHoverable = false; //do not enlarge card when hovered if this is false
    public Sprite caddieIcon; //if this is a caddie and is played, this image will be the caddie icon

    //Internal Helper Variables
    //these are used for when dragging
    private bool dragging = false;
    private float distance;
    private Vector3 startDist;
    private Vector3 startPos;
    //private float returnDuration = 0.25f;
    private Coroutine returnCoroutine;
    public bool onDisplay = false; //true if this card is being displayed and not interactable
    private Vector3 displayPos; //where this should be displayed if onDisplay is true
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool indicatingPlay = false;
    private Tween wiggleTween;

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


    public void ClickCard()
    {
        if (GameObject.Find("CourseManager").GetComponent<Course>().paused) return;

        if (isRemoveView || isUpgradeView)
        {
            if (!isPreviewing)
            {
                if (GameObject.Find("ShopManager").GetComponent<shopManager>().isPreviewing) return;
                // Prevent multiple previews
                isPreviewing = true;
                //Display this card
                originalPosition = transform.position;
                originalScale = transform.localScale;
                // Start animation to center
                Vector3 centerScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                centerScreen.z = 0;
                StartCoroutine(AnimateToPoint(centerScreen, transform.localScale * 1.5f, false, true));
                GameObject.Find("ShopManager").GetComponent<shopManager>().OpenPreview(this.gameObject, isRemoveView, isUpgradeView);
            }
        }


        if (isUpgradeOption || isShopOption || isDeckView)
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
                if (!GameObject.Find("CourseManager").GetComponent<Course>().canPlayBall)
                    return;
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
        GameObject.Find("GameManager").GetComponent<Hand>().DisplayHand();
    }

    public void MouseReleased()
    {
        if (GameObject.Find("CourseManager").GetComponent<Course>().paused) return;
        if (GameObject.Find("GameManager").GetComponent<Hand>().paused) return;
        if (isDeckView) return;
        if (isUpgradeOption) return;

        if (isShopOption)
        {
            if (GameObject.Find("CourseManager").GetComponent<Course>().tees >= myCost)
            {
                //pay the cost
                GameObject.Find("CourseManager").GetComponent<Course>().tees -= myCost;
                //add this to your deck
                GameObject.Find("GameManager").GetComponent<Hand>().baseDeck.Add(this.gameObject);
                gameObject.transform.parent = GameObject.Find("GameManager").transform.Find("BaseDeck");
                gameObject.SetActive(false);
                isShopOption = false;
                GameObject.Find("ShopManager").GetComponent<shopManager>().shopOptions.Remove(this.gameObject);
                //update shop UI
                GameObject.Find("ShopManager").GetComponent<shopManager>().UpdateUI();
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
        //returnCoroutine = StartCoroutine(ReturnToStart());
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
                //if (cardType != CardTypes.Caddie ||
                //GameObject.Find("GameManager").GetComponent<Hand>().playedCaddie == false) //one caddie per turn
                //{
                //    if (cardType != CardTypes.Ability || GameObject.Find("GameManager").GetComponent<Hand>().playedAbility == false ||
                //GameObject.Find("CourseManager").GetComponent<Course>().currentRival != 3)
                //{
                //play this
                StartCoroutine(PlayCard());
                //}
                //} 
            }
        }
        GameObject.Find("GameManager").GetComponent<Hand>().DisplayHand();
    }

    void Update()
    {
        if (onDisplay)
        {
            transform.position = displayPos;
            return;
        }
        if (dragging && (draggable || GameObject.Find("GameManager").GetComponent<Hand>().waitingForDiscard))
        {
            //snap card to mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);
            transform.position = rayPoint + startDist;
            //indicate that this can be played (or stop indicating)
            if (GameObject.Find("GameManager").GetComponent<Hand>().waitingForDiscard == false)
            {
                bool inPlayZone = transform.position.y > 0;
                if (inPlayZone && !indicatingPlay)
                {
                    indicatingPlay = true;
                    transform.DOKill(false);
                    wiggleTween = transform.DOPunchRotation(
                        new Vector3(0, 0, 8f),   // wiggle angle
                        0.4f,                    // duration
                        6                        //Vibrato
                    ).SetLoops(-1, LoopType.Restart);
                }
                else if (!inPlayZone && indicatingPlay)
                {
                    indicatingPlay = false;
                    if (wiggleTween != null)
                        wiggleTween.Kill();
                    transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad);
                }
            }
        }
    }

    // private IEnumerator ReturnToStart()
    // {
    //     Vector3 currentPos = transform.position;
    //     float elapsed = 0f;

    //     while (elapsed < returnDuration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = Mathf.Clamp01(elapsed / returnDuration);
    //         transform.position = Vector3.Lerp(currentPos, startPos, t);
    //         yield return null;
    //     }

    //     transform.position = startPos;
    //     returnCoroutine = null;
    // }

    //Update Text and icons on card
    public void UpdateCard()
    {
        //get upgrade info
        GetComponent<upgrades>().UpdateView();

        //set text
        titleObj.GetComponent<TextMeshProUGUI>().text = cardName;
        descObj.GetComponent<TextMeshProUGUI>().text = description;
        clubStatBlock.SetActive(cardType == CardTypes.Club);
        carryTextObj.SetActive(cardType == CardTypes.Club);
        rollTextObj.SetActive(cardType == CardTypes.Club);
        typeIconObj.GetComponent<Image>().sprite = typeIcons[(int)cardType];
        if (cardType == CardTypes.Club)
        {
            carryTextObj.GetComponent<TextMeshProUGUI>().text = Carry.ToString();
            rollTextObj.GetComponent<TextMeshProUGUI>().text = Roll.ToString();
        }
    }

    //set the sort order of the card and each image in it
    public void SetSortOrder(int so)
    {
        GetComponent<SpriteRenderer>().sortingOrder = so; //put this in front of black background
        GetComponentInChildren<Canvas>().sortingOrder = so;
    }

    public IEnumerator AnimateToPoint(Vector3 destination, Vector3 endScale, bool isLocal, bool setPreview)
    {
        Vector3 startPos = isLocal ? transform.localPosition : transform.position;
        Vector3 startScale = transform.localScale;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            if (isLocal)
                transform.localPosition = Vector3.Lerp(startPos, destination, t);
            else
                transform.position = Vector3.Lerp(startPos, destination, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        if (isLocal)
            transform.localPosition = destination;
        else
            transform.position = destination;
        transform.localScale = endScale;
        isPreviewing = setPreview;
    }

    //Play me
    public IEnumerator PlayCard()
    {
        //Debug.Log(cardName + " played");
        bool isStillActive = true; //turn to false if this card will be destroyed instead of discarded
        Course c = GameObject.Find("CourseManager").GetComponent<Course>();
        Hand h = GameObject.Find("GameManager").GetComponent<Hand>();
        //stop play indication animation
        indicatingPlay = false;
        if (wiggleTween != null)
        {
            wiggleTween.Kill();
            transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad);
        }
        if (cardName == "Voodoo Doll")
        {
            //voodoo doll cannot be played from hand
            yield return null;
        }
        //do effect
        if (cardType == CardTypes.Caddie)
        {
            if (h.HasCaddie("Caddie 2") > 0)
                h.DrawCard(1);
            h.caddies.Add(this.gameObject);
            h.playedCaddie = true;
            //Caddies do not go into the discard
            PlayCaddieAnimation();
            h.hand.Remove(this.gameObject);
            // this.gameObject.SetActive(false);
        }
        if (cardType == CardTypes.Ability)
        {
            h.playedAbility = true;
            if (c.currentRival == 7)
            {
                c.power -= 10;
                c.DisplayCourse();
                c.UpdateStatusEffectDisplay();
            }
        }
        switch (cardName)
        {
            case "Rangefinder":
                //set this card to middle of screen and stop shaking
                onDisplay = true;
                displayPos = new Vector3(-2, 1.5f, -2);
                indicatingPlay = false;
                if (wiggleTween != null)
                    wiggleTween.Kill();
                transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad);
                // If it's currently returning, stop that coroutine
                // if (returnCoroutine != null)
                // {
                //     StopCoroutine(returnCoroutine);
                //     returnCoroutine = null;
                // }
                // Wait for user to select a card to discard
                yield return h.WaitForDiscard();
                onDisplay = false;
                // Now draw 2 cards after discarding
                h.DrawCard(2);
                break;
            case "Backspin":
                c.pinpoint += 30;
                break;
            case "Chip Johnson":
                //deselect selected club if its a woods
                if (c.selectedClub != null &&
                    c.selectedClub.GetComponent<Draggable>().clubType == ClubTypes.Wood)
                {
                    c.selectedClub.GetComponent<Draggable>().selected = false;
                    c.selectedClub.GetComponent<SpriteRenderer>().color = Color.white;
                    c.selectedClub = null;
                }
                break;
            case "Dig Through Your Bag":
                //draw 3 then discard all non-clubs 
                List<GameObject> hand = h.hand;
                h.DrawCard(3, () =>
                {
                    for (int i = hand.Count - 1; i >= 0; i--)
                    {
                        if (hand[i].GetComponent<Draggable>().cardType != CardTypes.Club)
                        {
                            h.DiscardCard(hand[i]);
                        }
                    }
                });
                break;
            case "Find a Ball":
                // Draw 2 random ball cards from the deck
                List<GameObject> balls = new();
                foreach (GameObject go in h.currentDeck)
                {
                    if (go.GetComponent<Draggable>().cardType == CardTypes.Ball)
                        balls.Add(go);
                }
                List<GameObject> toDraw = new();
                int drawCount = Mathf.Min(2, balls.Count);
                // Shuffle the list
                for (int i = balls.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (balls[i], balls[j]) = (balls[j], balls[i]);
                }
                // Draw N balls from shuffled list
                for (int i = 0; i < drawCount; i++)
                {
                    toDraw.Add(balls[i]);
                }
                //draw the selected balls
                foreach (GameObject go in toDraw)
                {
                    h.DrawCard(go);
                }
                break;
            case "Golf Glove":
                //Gain 2 Luck 
                c.luck += 2;
                break;
            case "Phone a Friend":
                //add a random caddie to your hand
                List<GameObject> caddies = new();
                foreach (GameObject go in h.upgradeCards)
                {
                    //find all caddies available
                    if (go.GetComponent<Draggable>().cardType == CardTypes.Caddie)
                        caddies.Add(go);
                }
                //create new caddie card
                GameObject newCaddie = Instantiate(caddies[Random.Range(0, caddies.Count - 1)], GameObject.Find("GameManager").transform);
                h.hand.Add(newCaddie);
                break;
            case "Tee Up":
                //if in fairway, add all drivers to hand
                if (c.courseLayout[c.ballPos].GetComponent<CoursePiece>().myType == 0)
                    foreach (GameObject go in h.baseDeck)
                    {
                        if (go.GetComponent<Draggable>().isDriver)
                        {
                            GameObject newObj = Instantiate(go, GameObject.Find("GameManager").transform);
                            h.hand.Add(newObj);
                        }
                    }
                break;
            case "Back to Basics":
                //draw 2. disable balls until next swing
                h.DrawCard(2);
                if (c.selectedBall != null)
                {
                    c.selectedBall.GetComponent<Draggable>().selected = false;
                    c.selectedBall.GetComponent<SpriteRenderer>().color = Color.white;
                    c.selectedBall = null;
                }
                c.canPlayBall = false;
                break;
            case "Old Scorecard":
                h.DrawCard(c.strokeCount);
                break;
            case "Reckless Swing":
                List<GameObject> otherCards = h.hand.Where(card => card != this.gameObject).ToList();
                if (otherCards.Count == 0)
                    break;
                GameObject cardToDiscard = otherCards[Random.Range(0, otherCards.Count)];
                h.DiscardCard(cardToDiscard);
                c.power += 30;
                break;
            case "Practice Swing":
                if (h.hand.Count > 6)
                {
                    c.strokeCount--;
                    h.Toss(this.gameObject);
                    isStillActive = false;
                }
                break;
            // following are not yet implemented:
            case "Pocket Aces":
                //Toss 2 cards from your deck then gain 2 luck
                // h.Toss(h.currentDeck[0]);
                c.luck += 2;
                break;
            case "Unburden":
                //toss each card left in your deck. Gain 10 power for each
                // while(h.currentDeck.Count > 0)
                // {
                // h.Toss(h.currentDeck[0]);
                // c.power += 10;
                // }
                break;
            case "Recycle":
                //toss your hand. then draw your deck
                // while(h.hand.Count > 0)
                // {
                //     h.Toss(h.hand[0]);
                // }
                // h.DrawCard(h.currentDeck.Count);
                break;
        }
        //Update view
        c.DisplayCourse();
        c.UpdateStatusEffectDisplay();
        // Discard this card (caddies dont go to discard) (if it didnt toss itself)
        if (cardType != CardTypes.Caddie && isStillActive)
            h.DiscardCard(this.gameObject);
        //h.DisplayHand();
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
    }

    //returns null if cant take upgrade, or the matching upgrade if it will be replaced, or the passed upgrade if it is new
    public upgradeBuy CanUpgrade(upgradeBuy newUpgrade)
    {
        if ((int)newUpgrade.type < 3) //shaft/clubhead/grip
        {
            if (cardType == CardTypes.Club)
            {
                foreach (upgradeBuy myUpgrade in GetComponentsInChildren<upgradeBuy>())
                {
                    if (myUpgrade.type == newUpgrade.type)
                        return myUpgrade; //already has this type of upgrade
                }
                return newUpgrade; //can take this upgrade (new)
            }
        }
        return null; //cant take this upgrade
    }

    //animate this card being drawn from bag to hand
    //drawAmount: if > 1, draw another card after done drawing this one
    public void AnimateDraw(int drawAmount, System.Action completeCallback = null)
    {
        isHoverable = false;
        Vector3 startPosition = new(-8f, -8f);
        Vector3 handPosition = new(-2f, 0f);
        float moveDuration = 0.5f;
        float flipDuration = 0.25f;
        float arcHeight = 2f;
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        Vector3 controlPoint = (startPosition + handPosition) / 2f + Vector3.up * arcHeight;
        Sequence seq = DOTween.Sequence();
        // Set sort order so it is in front of everything
        GetComponentInChildren<Canvas>().sortingOrder = 1000;
        GetComponent<SpriteRenderer>().sortingOrder = 1000;
        //start face down
        cardBack.SetActive(true);
        typeIconObj.SetActive(false);
        // Move in arc
        seq.Append(
            transform.DOPath(
                new Vector3[] { startPosition, controlPoint, handPosition },
                moveDuration,
                PathType.CatmullRom
            ).SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    //add this to the hand list then display hand
                    GameObject.Find("GameManager").GetComponent<Hand>().hand.Add(this.gameObject);
                    GameObject.Find("GameManager").GetComponent<Hand>().DisplayHand();
                    //start drawing another card if necessary
                    if (drawAmount > 1)
                        GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(drawAmount - 1);
                    else if (completeCallback != null)
                        completeCallback.Invoke();

                })
        );
        // Scale up during front half of the arc
        seq.Insert(
            0f,
            transform.DOScale(Vector3.one, moveDuration * 0.5f)
                .SetEase(Ease.OutBack)
        );
        // Flip midway
        seq.Insert(
            moveDuration * 0.6f,
            transform.DOScaleX(0f, flipDuration / 2f).OnComplete(() =>
            {
                cardBack.SetActive(false);
                typeIconObj.SetActive(true);
                transform.DOScaleX(1f, flipDuration / 2f);
            }
        ));
    }

    //animate this card being discarded
    //if toss = true, then this tosses instead of discard (animate to left)
    //completeCallback will call a function when complete
    public void AnimateDiscard(bool toss = false, System.Action completeCallback = null)
    {
        isHoverable = false;
        //remove this now so and hand displays while moving wont touch this
        GameObject.Find("GameManager").GetComponent<Hand>().hand.Remove(this.gameObject);
        //animation variables
        float moveDuration = 1.5f;
        float rotateDuration = 0.75f;
        float arcHeight = 3f;
        Vector3 rotation = toss ? new Vector3(0, 0, 180f) : new Vector3(0, 0, -180f);
        Vector3 endPos = toss ? new(-10f, -10f) : new(10f, -10f);
        Vector3 controlPoint = (transform.position + endPos) / 2f;
        controlPoint.y = transform.position.y + arcHeight;
        // Set sort order so it is in front of everything
        GetComponentInChildren<Canvas>().sortingOrder = 1000;
        GetComponent<SpriteRenderer>().sortingOrder = 1000;
        // Move in arc
        Sequence seq = DOTween.Sequence();
        seq.Append(
            transform.DOPath(
                new Vector3[] { transform.position, controlPoint, endPos },
                moveDuration,
                PathType.CatmullRom
            ).SetEase(Ease.OutCubic)
        );

        // Rotate upside down WHILE moving
        seq.Join(
            transform.DORotate(
                rotation,
                rotateDuration,
                RotateMode.FastBeyond360
            ).SetEase(Ease.InOutQuad)
        );

        seq.OnComplete(() =>
        {
            if (toss)
            {
                GameObject.Find("GameManager").GetComponent<Hand>().Toss(this.gameObject);
            }
            else
            {
                this.gameObject.SetActive(false);
                GameObject.Find("GameManager").GetComponent<Hand>().discardPile.Add(this.gameObject);
            }
            GameObject.Find("GameManager").GetComponent<Hand>().DisplayHand();
            if (completeCallback != null)
                completeCallback.Invoke();
        });
    }

    //sends this to the caddie area while shrinking it, then pop up a caddie icon
    public void PlayCaddieAnimation()
    {
        isHoverable = false;
        Hand h = GameObject.Find("GameManager").GetComponent<Hand>();
        //animation variables
        float moveDuration = 1.0f;
        Vector3 endPos = new(h.caddieDisplays.Count * 1 - 5, 2f);
        // Set sort order so it is in front of everything
        GetComponentInChildren<Canvas>().sortingOrder = 1000;
        GetComponent<SpriteRenderer>().sortingOrder = 1000;
        //move the card while scaling it down
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMove(endPos, moveDuration));
        seq.Join(transform.DOScale(new Vector3(0, 0), moveDuration).SetEase(Ease.InQuad));
        // seq.Play();
        //create the caddie icon when done
        seq.OnComplete(() =>
        {
            GameObject newCaddie = Instantiate(h.caddieDisplayObj, GameObject.Find("MainCanvas").transform);
            newCaddie.GetComponent<RectTransform>().position = endPos;
            newCaddie.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
            newCaddie.GetComponent<caddieDisplay>().caddieRef = this.gameObject;
            newCaddie.GetComponent<Image>().sprite = caddieIcon;
            h.caddieDisplays.Add(newCaddie);
            this.gameObject.SetActive(false);
            //animate caddie appearing
            newCaddie.GetComponent<RectTransform>().DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic);
            // refresh hand
            GameObject.Find("GameManager").GetComponent<Hand>().DisplayHand();
        });
    }
}
