using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Course : MonoBehaviour
{
    //prevent duplicates of this obj
    public static GameObject courseManagerObj;
    public void Awake()
    {
        if(courseManagerObj == null)
        {
            courseManagerObj = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private List<GameObject> courseLayout = new();
    public List<GameObject> coursePieces;
    public GameObject courseDisplay; //the child object that holds all of the course pieces
    public Hand handRef;
    public int courseNum = 0;
    public int holeNum = 0; //what number hole you are on
    public List<int> scores = new List<int>();
    public List<int> pars = new List<int>();
    private enum CoursePieces
    {
        FAIRWAY = 0,
        ROUGH,
        SAND,
        WATER,
        GREEN,
        HOLE
    }
    public List<Sprite> courseArt; //sprites for each course piece
    //0,1,2 - Fairway
    //3,4,5 - Rough
    //6,7,8 - Green
    //9,10,11 - Sand
    //12,13,14 - Water
    //16 - Hole
    public int ballPos;
    public int strokeCount; //how many times you hit the ball
    public GameObject selectedClub;
    public GameObject selectedBall;
    public GameObject dotPrefab; //dot obj for end of swing arc
    public GameObject boundsPrefab; //X obj for end of swing arc
    private GameObject currentDot;
    public GameObject ballObj;
    public int luck; //ignore a hazard then reduce luck by 1. Persists between shots but not holes
    public GameObject finishText; //the text obj that displays whether you got par/etc
    public int tees = 0;
    public bool comingFromShop; //set to true after shop so you know to create new course

    //dragging the course
    private bool isDragging = false;
    private float startMouseX;
    private float startChildX;
    private float minX;
    private float maxX;
    public float childWidth;

    //status effects
    public int power = 0;
    public int pinpoint = 0;

    //rivals
    public List<string> rivalNames = new List<string>();
    public List<string> rivalDescriptions = new List<string>();
    public List<int> rivalScores = new List<int>();
    public List<Sprite> rivalImages = new List<Sprite>();
    public int currentRival = 0;
    
    void Update()
    {
        //Animate shot arc          
        float scrollSpeed = 1.5f;
        float offset = Time.time * scrollSpeed;
        GetComponent<LineRenderer>().material.SetTextureOffset("_MainTex", new Vector2(-offset, 0));
        //Drag the course along x axis
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero);
            foreach (var hit in hits)
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    startMouseX = mouseWorldPos.x;
                    startChildX = courseDisplay.transform.localPosition.x;
                    float childCount = courseDisplay.transform.childCount;
                    // Define movement bounds based on number of children
                    minX = childCount * childWidth * -1f + 16.5f;
                    maxX = 0f;
                    break;
                }
            }
        }
        if (isDragging && Input.GetMouseButton(0))
        {
            float currentMouseX = mouseWorldPos.x;
            float deltaX = currentMouseX - startMouseX;
            float targetX = startChildX + deltaX;
            float clampedX = Mathf.Clamp(targetX, minX, maxX);
            Vector3 newLocalPos = courseDisplay.transform.localPosition;
            newLocalPos.x = clampedX;
            courseDisplay.transform.localPosition = newLocalPos;
            //Update shot arc
            //reset highlight
            GetComponent<LineRenderer>().positionCount = 0;
            Destroy(currentDot);
            if (selectedClub != null)
            {
                //Calculate hit with currently selected cards
                int carry = selectedClub.GetComponent<Draggable>().carry / 10;
                int roll = selectedClub.GetComponent<Draggable>().roll / 10;
                HighlightHit();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    public void NewCourse()
    {
        //set initial course data
        holeNum = 8; //note: this will be incremented once before each hole
        courseNum++;
        //set up rival
        currentRival = Random.Range(0, rivalNames.Count - 1);
        //generate par data
        scores.Clear();
        pars = new();
        for (int i = 0; i < 9; i++)
        {
            pars.Add(4);
        }
        int par3 = Random.Range(0, 9);
        int par5 = (par3 + Random.Range(1,9)) % 9;
        pars[par3] = 3;
        pars[par5] = 5;
        NewHole();
    }

    //Generate new hole
    public void NewHole()
    {
        // Reset state
        luck = 0;
        ballObj.SetActive(true);
        ballPos = 0;
        strokeCount = 0;
        holeNum++;
        courseLayout = new List<GameObject>();
        power = 0;
        pinpoint = 0;
        Vector3 newLocalPos = courseDisplay.transform.localPosition;
        newLocalPos.x = 0;
        courseDisplay.transform.localPosition = newLocalPos;
        //Generate Fairway
        int holeLength = 30 + courseNum * 6 + (pars[holeNum-1] - 4) * 10 + Random.Range(-2,3); //actual length
        for (int i = 0; i < holeLength; i++)
        {
            GameObject fairway = Instantiate(coursePieces[(int)CoursePieces.FAIRWAY], courseDisplay.transform);
            fairway.GetComponent<CoursePiece>().myIndex = i;
            courseLayout.Add(fairway);
        }
        //Create Hazards
        //Add some rough areas
        int currentPos = Random.Range(1,10);
        while(currentPos < courseLayout.Count)
        {
            int patchSize = Random.Range(3, 6);
            AddHazardPatch((int)CoursePieces.ROUGH, currentPos, patchSize);
            currentPos += Random.Range(patchSize + 3, patchSize + 8);
        }
        //Add in some sand/water pits
        int numSandWater = Random.Range(2, 4 + courseNum);
        for (int i = 0; i < numSandWater; i++)
        {
            AddHazardPatch(Random.Range(2,4), Random.Range(1,courseLayout.Count - 4), Random.Range(1, 4));
        }
        //Maybe add large pond (removed since you just always end up right in front of green after penalty
        //if(Random.Range(0, 3) == 0)
        //{
        //    AddHazardPatch((int)CoursePieces.WATER, Random.Range(3,courseLayout.Count - 10), Random.Range(5, 15));
        //}
        //Add in green
        int greenStartOffsetMin = 12;
        int greenStartOffsetMax = 18;
        int greenLengthMin = 6;
        int greenLengthMax = 10;
        int startOfGreen = holeLength - Random.Range(greenStartOffsetMin, greenStartOffsetMax);
        int greenLength = Random.Range(greenLengthMin, greenLengthMax);
        for (int i = startOfGreen; i < Mathf.Min(startOfGreen + greenLength, courseLayout.Count); i++)
        {
            ReplacePieceAt(i, (int)CoursePieces.GREEN);
        }
        ReplacePieceAt(startOfGreen - 1, Random.Range(0,2)); //Dont let there be water before the green
        //Place Hole on the Green
        int holePlacement = Mathf.Clamp(startOfGreen + Random.Range(2, 5), startOfGreen, courseLayout.Count - 1);
        ReplacePieceAt(holePlacement, (int)CoursePieces.HOLE);
        //make last piece of the course grass (so it doesnt end in water)
        ReplacePieceAt(courseLayout.Count - 1, (int)CoursePieces.FAIRWAY);
        DisplayCourse();
    }

    //helper method that inputs pieceType at index
    private void ReplacePieceAt(int index, int pieceType)
    {
        GameObject oldPiece = courseLayout[index];
        GameObject newPiece = Instantiate(coursePieces[pieceType], courseDisplay.transform);
        newPiece.GetComponent<CoursePiece>().myIndex = index;
        courseLayout[index] = newPiece;
        Destroy(oldPiece);
    }

    //helper method that creates a hazard at random spot
    private void AddHazardPatch(int hazardType, int startIndex, int patchSize)
    {
        for (int i = 0; i < patchSize; i++)
        {
            if(startIndex + i < courseLayout.Count)
                ReplacePieceAt(startIndex + i, hazardType);
        }
    }

    public void DisplayCourse()
    {
        GameObject.Find("RivalDisplay").GetComponent<rivalDisplay>().idNum = currentRival;
        GameObject.Find("Stroke").GetComponent<TextMeshProUGUI>().text = "Hole: " + holeNum + "\nStrokes: " + strokeCount;
        GameObject.Find("TeeCount").GetComponent<TextMeshProUGUI>().text = tees.ToString();
        //Put pieces in their place
        int prevPieceType = 0;
        for (int i = 0; i < courseLayout.Count; i++)
        {
            //set position
            GameObject go = courseLayout[i];
            go.transform.localPosition = new Vector2(i * childWidth - 7.5f, 1.0f);
            //set art
            int myType = go.GetComponent<CoursePiece>().myType;
            int offset = 1;
            if (prevPieceType != myType)
                offset = 0; // this is the first piece of this type
            else if (i < courseLayout.Count - 1 &&  myType != courseLayout[i+1].GetComponent<CoursePiece>().myType)
                offset = 2; //this is the last piece
            if (myType == 5) offset = 1; //hole is always middle piece
            if(myType == 4 && courseLayout[i + 1].GetComponent<CoursePiece>().myType == 5) offset = 1; //green piece before hole
            if(myType == 4 && courseLayout[i - 1].GetComponent<CoursePiece>().myType == 5) offset = 1; //green piece after hole
            go.GetComponent<SpriteRenderer>().sprite = courseArt[myType * 3 + offset];
            prevPieceType = myType;
        }
        //Put ball in its place
        ballObj.transform.localPosition = new Vector3(ballPos * childWidth - 7.5f, 1.25f, -1);
        //reset highlight
        GetComponent<LineRenderer>().positionCount = 0;
        Destroy(currentDot);
        if (selectedClub != null)
        {
            //Calculate hit with currently selected cards
            int carry = selectedClub.GetComponent<Draggable>().carry / 10;
            int roll = selectedClub.GetComponent<Draggable>().roll / 10;
            HighlightHit();
        }
    }

    // Returns the distance from given point to the hole
    public int DistanceToHole(int startIndex)
    {
        //Find where the hole is
        foreach (GameObject go in courseLayout)
        {
            if(go.GetComponent<CoursePiece>().pieceName == "Hole")
            {
                return go.GetComponent<CoursePiece>().myIndex - startIndex;
            }
        }
        return 0;
    }

    // Returns the distance from given point to the ball
    public int DistanceToBall(int startIndex)
    {
        return ballPos - startIndex;
    }

    //Move the ball its carry distance then the roll distance
    public void HitBall(int carry, int roll)
    {
        //card effects
        if (selectedClub.GetComponent<Draggable>().cardName == "Shovel Wedge")
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Sand")
                GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(3);
        //can play another caddie
        GameObject.Find("GameManager").GetComponent<Hand>().playedCaddie = false;
        GameObject.Find("GameManager").GetComponent<Hand>().playedAbility = false;
        //calculate swing
        SwingResult swing = CalculateSwing();
        string ballName = selectedBall != null ? selectedBall.GetComponent<Draggable>().cardName : "";
        luck -= swing.luckUsed;
        //add a stroke
        if(ballName != "Practice Ball" || GameObject.Find("GameManager").GetComponent<Hand>().hand.Count < 6)
            strokeCount++;
        //if out of bounds
        if (swing.endIndex >= courseLayout.Count)
        {
            //ball stays where it is and you lose a stroke
            strokeCount++;
            pinpoint = 0;
        }
        else
        {
            //set ball to where it lands
            ballPos = swing.endIndex;
            power = swing.roughHits * -10;
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Rough")
            {
                //lose 10 power
                if (luck > 0)
                {
                    luck--;
                }
                else
                {
                    power -= 10;
                }
            }
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Sand")
            {
                //discard a random card from your hand
                if (luck > 0)
                {
                    luck--;
                }
                else
                {
                    Hand handObj = GameObject.Find("GameManager").GetComponent<Hand>();
                    handObj.DiscardCard(handObj.hand[Random.Range(0, handObj.hand.Count)]);
                }
                //Item Effects
                if (ballName == "Beach Ball")
                {
                    //Draw 3
                    GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(3);
                }
            }
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Water")
            {
                //move up to next non water space and take stroke penalty (if not lucky)
                while (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Water")
                    ballPos++;
                //Rubber Duck Ball ignores water
                if (selectedBall == null || selectedBall.GetComponent<Draggable>().cardName != "Rubber Duck Ball")
                {
                    if (luck > 0)
                    {
                        luck--;
                    }
                    else
                    {
                        strokeCount++;
                        if(currentRival == 1)
                            strokeCount++;
                    }
                }
            }
            pinpoint = 0;
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Fairway")
            {
                if (ballName == "Ace Ball")
                {
                    //Draw 2
                    GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(2);
                }
            }
        }
        //deselect current club and ball and update hand
        if (selectedClub != null)
        {
            if(selectedClub.GetComponent<Draggable>().isDriver)
            {
                handRef.hand.Remove(selectedClub);
                Destroy(selectedClub);
            }
            else
            {
                handRef.DiscardCard(selectedClub);
            }
            selectedClub.GetComponent<Draggable>().selected = false;
            selectedClub.GetComponent<SpriteRenderer>().color = Color.white;
            selectedClub = null;
        }
        if (selectedBall != null)
        {
            handRef.DiscardCard(selectedBall);
            selectedBall.GetComponent<Draggable>().selected = false;
            selectedBall.GetComponent<SpriteRenderer>().color = Color.white;
            selectedBall = null;
        }
        //Remove all drivers from hand
        for (int i = handRef.hand.Count - 1; i >= 0; i--)
        {
            GameObject go = handRef.hand[i];
            if (go.GetComponent<Draggable>().isDriver)
            {
                handRef.hand.RemoveAt(i);
                Destroy(go);
            }
        }
        handRef.DrawCard(1);
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Caddie 4") > 0)
            luck++;
        //If on green, perform a putt
        if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Green")
        {
            //TO DO: take distance and putter into account
            strokeCount++;
            GoToNextHole();
            return;
        }
        //if in the hole, go to next hole
        if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Hole")
        { 
            GoToNextHole();
            return;
        }
        //Update scene graphics
        UpdateStatusEffectDisplay();
        DisplayCourse();
    }

    //Highlight the part of the course that a hit would land on
    public void HighlightHit()
    {
        //calculate how far the ball rolls
        SwingResult swing = CalculateSwing();
        //Draw the calculated swing
        if (swing.endIndex >= courseLayout.Count)
        {
            //lands out of bounds
            Vector3 startPos = courseLayout[swing.startIndex].transform.position;
            Vector3 localOutOfBoundsPos = new Vector3(swing.landIndex * childWidth - 10, 0, 0);
            Vector3 worldOutOfBoundsPos = courseDisplay.transform.TransformPoint(localOutOfBoundsPos);

            DrawArc(startPos, worldOutOfBoundsPos, worldOutOfBoundsPos.x, boundsPrefab);
        }
        else
        {
            //Draw it normally
            DrawArc(courseLayout[swing.startIndex].transform.position, courseLayout[swing.landIndex].transform.position,
                courseLayout[swing.endIndex].transform.position.x, dotPrefab);
        }
    }

    public struct SwingResult
    {
        public int startIndex; //where the ball is being hit from
        public int landIndex; //where the carry ends and roll starts
        public int endIndex; //where the roll ends
        public int roughHits; //how many times the ball went through the rough
        public int luckUsed;
    }
    //called by both hitball and highlight hit to calculate the swing
    private SwingResult CalculateSwing()
    {
        SwingResult swing = new();
        swing.roughHits = 0;
        //get base distances
        if (selectedClub == null)
            return swing; //cant swing without a club!
        int carry = selectedClub.GetComponent<Draggable>().carry/10;
        int roll = selectedClub.GetComponent<Draggable>().roll/10;
        bool hasRoll = true; //false if an effect makes this shot have 0 roll
        //card effects
        //clubs
        if (selectedClub.GetComponent<Draggable>().cardName == "Big Iron")
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Rough")
                carry += 4;
        if (selectedClub.GetComponent<Draggable>().cardName == "Big Betty")
            carry += GameObject.Find("GameManager").GetComponent<Hand>().hand.Count;
        //caddies
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Chip Johnson") > 0 &&
            selectedClub.GetComponent<Draggable>().clubType == Draggable.ClubTypes.Iron)
            carry *= 2;
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Caddie 3") > 0)
            carry += GameObject.Find("GameManager").GetComponent<Hand>().caddies.Count;
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Lion Forest") > 0)
        {
            if (selectedClub.GetComponent<Draggable>().clubType == Draggable.ClubTypes.Iron)
            {
                carry += 2;
                roll -= 2;
            }
        }
        //rivals
        if(currentRival == 0)
        {
            if (selectedClub.GetComponent<Draggable>().isDriver)
                carry -= 3;
        }
        //balls
        if (selectedBall != null)
        {
            string ball = selectedBall.GetComponent<Draggable>().cardName;
            switch (ball)
            {
                case "Distance Ball":
                    carry += 2;
                    break;
                case "Breakfast Ball":
                    if(selectedClub.GetComponent<Draggable>().clubType == Draggable.ClubTypes.Wood)
                        hasRoll = false;
                    break;
                case "Ice Ball":
                    roll *= 2;
                    break;
                case "Square Ball":
                    carry -= 2;
                    hasRoll = false;
                    break;
            }
        }
        int direction = 1;
        if(DistanceToHole(ballPos) < 0)
        {
            direction = -1;
        }
        int luckUsed = 0; //temporary count for using luck
        int rollAmount = 0;
        int start = ballPos;
        swing.startIndex = start;
        swing.landIndex = start + (carry + power/10) * direction;
        int end = swing.landIndex;
        if(swing.landIndex >= courseLayout.Count || swing.landIndex < 0)
        {
            //Is landing out of bounds
            hasRoll = false;
        }
        if (hasRoll)
            end = start + (carry + power/10 + roll - pinpoint/10) * direction;
        GameObject piece;
        for (int i = swing.landIndex; direction == 1 ? i < end : i > end; i += direction)
        {
            //Check to see if it rolls out of bounds
            if (i >= courseLayout.Count || i < 0)
            {
                i = end;
                break;
            }
            //Trigger effect of piece
            piece = courseLayout[i];
            switch (piece.GetComponent<CoursePiece>().pieceName)
            {
                case "Rough":
                    if (luck - luckUsed > 0)
                        luckUsed++;
                    else
                        swing.roughHits += 1; 
                    rollAmount++;
                    break;
                case "Fairway":
                case "Green":
                    rollAmount++;
                    break;
                case "Sand":
                case "Water":
                    if (selectedBall != null && selectedBall.GetComponent<Draggable>().cardName == "Rubber Duck Ball" &&
                        piece.GetComponent<CoursePiece>().pieceName == "Water")
                    {
                        //Rubber Duck Ball ignores water
                        rollAmount++;
                    }
                    else
                    {
                        if (luck - luckUsed > 0)
                        {
                            luckUsed++;
                            rollAmount++;
                        }
                        else
                        {
                            i = end;
                        }
                    }
                    break;
                case "Hole":
                    // Stop immediately
                    i = end;
                    break;
            }
        }
        swing.endIndex = swing.landIndex + rollAmount * direction;
        swing.luckUsed = luckUsed;
        return swing;
    }

    //hit the ball
    public void Swing()
    {
        if (selectedClub == null) return; //need a club to swing
        int carry = selectedClub.GetComponent<Draggable>().carry;
        int roll = selectedClub.GetComponent<Draggable>().roll;
        HitBall(carry / 10, roll / 10);
    }

    public void UpdateStatusEffectDisplay()
    {
        GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text = "";
        int tempPower = 0;
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Caddie 3") > 0)
            tempPower += 10 * GameObject.Find("GameManager").GetComponent<Hand>().caddies.Count;
        if (power + tempPower != 0)
            GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text += "Power: " + (power + tempPower) + "\n";
        if (pinpoint != 0)
            GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text += "Pinpoint: " + pinpoint;
        if (luck != 0)
            GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text += "Luck: " + luck;
    }

    public void DrawArc(Vector3 start, Vector3 end, float targetX, GameObject endObj)
    {
        float arcHeight = 2f;
        int arcSegments = 50;
        int tailSegments = 20;         // Number of points in the straight line
        int totalPoints = arcSegments + tailSegments + 1;
        LineRenderer line = GetComponent<LineRenderer>();
        line.positionCount = totalPoints;

        //Arc points
        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            Vector3 pos = Vector3.Lerp(start, end, t);
            float height = arcHeight * 4 * (t - t * t); // Parabola
            pos.y += height;
            line.SetPosition(i, pos);
        }
        //Straight tail points along the X-axis
        Vector3 lastArcPoint = line.GetPosition(arcSegments);
        float dx = (targetX - lastArcPoint.x) / tailSegments;
        for (int i = 1; i <= tailSegments; i++)
        {
            Vector3 tailPos = lastArcPoint;
            tailPos.x += dx * i; // Move right along X
            line.SetPosition(arcSegments + i, tailPos);
        }
        //Draw a dot at the end
        Vector3 endPoint = line.GetPosition(totalPoints - 1);
        endPoint.z = -1;
        if (dotPrefab != null)
        {
            if (currentDot != null) Destroy(currentDot); // Avoid duplicates
            currentDot = Instantiate(endObj, endPoint, Quaternion.identity);
        }
    }

    public void GoToNextHole()
    {
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Caddie 1") > 0)
            if (GameObject.Find("GameManager").GetComponent<Hand>().hand.Count >= 6)
                strokeCount--;
        DisplayCourse();
        //Display Score
        string[] score = { "ACE", "HOLE IN ONE", "EAGLE", "BIRDIE", "PAR", "BOGEY", "DOUBLE BOGEY", "TRIPLE BOGEY" };
        GameObject txtObj = Instantiate(finishText, GameObject.Find("MainCanvas").transform);
        if(strokeCount >= score.Length)
            txtObj.GetComponent<TextMeshProUGUI>().text = "+" + (strokeCount - pars[holeNum - 1]);
        else
            txtObj.GetComponent<TextMeshProUGUI>().text = score[strokeCount + 4 - pars[holeNum - 1]];
        float t = Mathf.Min(Mathf.Max(strokeCount - 3,0), 7.0f) / 7.0f;
        txtObj.GetComponent<TextMeshProUGUI>().color = Color.Lerp(Color.green, Color.red, t);
        txtObj.GetComponentInChildren<Button>().onClick.AddListener(ContinueButtonClick);
    }

    public void ContinueButtonClick()
    {
        //clear current hole
        foreach (GameObject go in courseLayout)
        {
            Destroy(go);
        }
        courseLayout.Clear();
        ballObj.SetActive(false);
        //reset highlight
        GetComponent<LineRenderer>().positionCount = 0;
        if (currentDot != null)
            Destroy(currentDot);
        //Go to upgrade screen
        scores.Add(strokeCount);
        //If just finished a course, check for loss
        if(holeNum >= 9)
        {
            int totalScore = 0;
            foreach (int score in scores)
                totalScore += score;
            if(totalScore - 36 >= rivalScores[currentRival])
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene("Lose");
                return;
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("New Card");
    }

    //This is called once the New Card scene is finished loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "New Card")
        {
            GameObject.Find("GameManager").GetComponent<Hand>().RemoveDeck();
            GameObject.Find("GameManager").GetComponent<Hand>().CreateNewCardOptions();
            int teeReward = Mathf.Max(pars[holeNum - 1] - strokeCount + 3,1);
            GameObject.Find("TeesText").GetComponent<TextMeshProUGUI>().text = "(     +" + teeReward + ")";
            GameObject.Find("SkipButton").GetComponent<Button>().onClick.AddListener
                (GameObject.Find("GameManager").GetComponent<Hand>().SkipUpgrade);
            // Unsubscribe to avoid duplicate calls in the future
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        if (scene.name == "Lose")
        {
            GameObject.Find("RivalImage").GetComponent<rivalDisplay>().LoseScreenUpdate();
            GameObject.Find("GameManager").GetComponent<Hand>().RemoveDeck();
            GameObject.Find("Scorecard").GetComponent<scorecard>().ShowScores();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    //Take 1 stroke to draw a card
    public void Mulligan()
    {
        strokeCount++;
        GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(1);
        DisplayCourse();
    }
}
