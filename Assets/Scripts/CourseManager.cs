using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Course : MonoBehaviour
{
    //holds the info on a course playthrough
    public class CourseData
    {
        public int courseNum, rival, courseType;
        public List<int> scores, pars;
        public bool lostRun;
    }
    public List<CourseData> currentPlaythrough = new();

    //prevent duplicates of this obj
    public static GameObject courseManagerObj;
    public void Awake()
    {
        //set up debug
        GameObject.Find("SkipHoleButton").GetComponent<Button>().onClick.AddListener(GameObject.Find("CourseManager").GetComponent<Course>().SkipHole);
        //
        if (courseManagerObj == null)
        {
            //set up initial game state
            courseManagerObj = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
            for(int i = 0; i < rivalImages.Count; i++)
            {
                availRivals.Add(i);
            }
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
    public enum CourseType
    {
        plains,
        forest,
        hills,
        beach,
        desert,
    }
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
    public CourseType courseType;
    public int nextCourse = 0; //if -1, choose random course. Otherwise use this

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
    public int currentRival = -1;
    public List<int> availRivals = new(); //tracks which rivals have already been played so no duplicates

    //lose screen
    public GameObject recapObj; //instantiate to recap each course played

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
                    GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().StorePositions();
                    float childCount = courseDisplay.transform.childCount;
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

            // Drag the course
            float targetX = startChildX + deltaX;
            float clampedX = Mathf.Clamp(targetX, minX, maxX);
            Vector3 newLocalPos = courseDisplay.transform.localPosition;
            newLocalPos.x = clampedX;
            courseDisplay.transform.localPosition = newLocalPos;

            // Calculate how far the course has actually moved
            float courseDelta = clampedX - startChildX;
            GameObject bgm = GameObject.Find("BackgroundManager");
            if(bgm != null)
                bgm.GetComponent<backgroundManager>().UpdatePositions(courseDelta);

            // Reset and redraw shot arc
            GetComponent<LineRenderer>().positionCount = 0;
            Destroy(currentDot);
            if (selectedClub != null)
            {
                int carry = selectedClub.GetComponent<Draggable>().Carry / 10;
                int roll = selectedClub.GetComponent<Draggable>().Roll / 10;
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
        holeNum = 0; //note: this will be incremented once before each hole
        courseNum++;
        if (nextCourse == -1)
        {
            courseType = (CourseType)Random.Range(0, 5);
        }
        else
        {
            courseType = (CourseType)nextCourse;
            nextCourse = -1;
        }
        //set up rival
        if(currentRival == -1) //if it is not -1, just use what it is currently set to
        {
            currentRival = availRivals[Random.Range(0, availRivals.Count)];
            availRivals.Remove(currentRival);
        }
        //generate par data
        int numParThrees = 1;
        int numParFives = 1;
        switch (courseType)
        {
            case CourseType.plains:
                numParFives += Random.Range(1, 3);
                break;
            case CourseType.desert:
                numParFives += 1;
                numParThrees += 1;
                break;
            case CourseType.beach:
                numParThrees += Random.Range(1, 3);
                break;
            case CourseType.hills:
                numParFives += Random.Range(0, 2);
                break;
            case CourseType.forest:
                break;
        }
        scores.Clear();
        pars = new();
        for (int i = 0; i < 9; i++)
        {
            pars.Add(4);
        }
        List<int> availableHoles = Enumerable.Range(0, 9).ToList();
        // Assign Par 3s
        for (int i = 0; i < numParThrees && availableHoles.Count > 0; i++)
        {
            int index = Random.Range(0, availableHoles.Count);
            pars[availableHoles[index]] = 3;
            availableHoles.RemoveAt(index);
        }
        // Assign Par 5s
        for (int i = 0; i < numParFives && availableHoles.Count > 0; i++)
        {
            int index = Random.Range(0, availableHoles.Count);
            pars[availableHoles[index]] = 5;
            availableHoles.RemoveAt(index);
        }
        //start first hole
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
        GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().SetSprites((int)courseType);
        GetComponent<BoxCollider2D>().enabled = true;
        //Generate Fairway
        CoursePieces fairwayType = CoursePieces.FAIRWAY;
        int lengthMod = 0;
        switch(courseType)
        {
            case CourseType.plains:
                lengthMod = 5;
                break;
            case CourseType.desert:
                fairwayType = CoursePieces.SAND;
                lengthMod = -2;
                break;
            case CourseType.beach:
                lengthMod = -3;
                break;
            case CourseType.hills:
                lengthMod = 1;
                break;
            case CourseType.forest:
                lengthMod = -1;
                fairwayType = CoursePieces.ROUGH;
                break;
        }
        int holeLength = 30 + courseNum * 6 + (pars[holeNum-1] - 4) * 10 + lengthMod + Random.Range(-2,3); //actual length
        //int holeLength = 20 + courseNum * 6 + (pars[holeNum-1] - 4) * 10 + lengthMod + Random.Range(-2,3); //shorter test length
        for (int i = 0; i < holeLength; i++)
        {
            GameObject fairway = Instantiate(coursePieces[(int)fairwayType], courseDisplay.transform);
            fairway.GetComponent<CoursePiece>().myIndex = i;
            courseLayout.Add(fairway);
        }
        //Create Hazards
        int currentPos = Random.Range(1, 10);
        int greenStartOffsetMin = 12;
        int greenStartOffsetMax = 18;
        int greenLengthMin = 6;
        int greenLengthMax = 10;
        switch (courseType)
        {
            case CourseType.plains:
                //small patches of various hazards
                while (currentPos < courseLayout.Count)
                {
                    int patchSize = Random.Range(2, 5);
                    int patchType = Random.Range(1, 3);
                    AddHazardPatch(patchType, currentPos, patchSize);
                    currentPos += patchSize + Random.Range(0, 5);
                }
                //large green
                greenStartOffsetMin = 14;
                greenStartOffsetMax = 20;
                greenLengthMin = 8;
                greenLengthMax = 12;
                break;
            case CourseType.desert:
                //patches of rough and fairway
                while (currentPos < courseLayout.Count)
                {
                    int patchSize = Random.Range(4, 8);
                    int patchType = Random.Range(0, 2);
                    AddHazardPatch(patchType, currentPos, patchSize);
                    currentPos += patchSize + Random.Range(0, 5);
                }
                //large green
                greenStartOffsetMin = 14;
                greenStartOffsetMax = 20;
                greenLengthMin = 8;
                greenLengthMax = 12;
                break;
            case CourseType.beach:
                //patches of sand and water
                while (currentPos < courseLayout.Count)
                {
                    int patchSize = Random.Range(1, 4);
                    int patchType = Random.Range(0,5) == 0 ? 1 : Random.Range(2, 4);
                    AddHazardPatch(patchType, currentPos, patchSize);
                    currentPos += patchSize + Random.Range(0, 5);
                }
                //small green
                greenStartOffsetMin = 10;
                greenStartOffsetMax = 16;
                greenLengthMax = 8;
                break;
            case CourseType.hills:
                //patches large patches of rough and water, small sand pits
                while (currentPos < courseLayout.Count)
                {
                    int patchSize = Random.Range(4, 8);
                    int patchType = Random.Range(0, 3) == 0 ? 3 : 1;
                    if (patchType == 3) patchSize -= 2; 
                    AddHazardPatch(patchType, currentPos, patchSize);
                    currentPos += patchSize + Random.Range(1, 4);
                    if(Random.Range(0,5) == 0)
                    {
                        //add small sand pit occasionally
                        AddHazardPatch((int)CoursePieces.SAND, currentPos + Random.Range(0,5), Random.Range(1,4));
                    }
                }
                //larger green
                greenStartOffsetMax = 20;
                greenLengthMax = 12;
                break;
            case CourseType.forest:
                //patches of fairway, sand, and water
                while (currentPos < courseLayout.Count)
                {
                    int patchSize = Random.Range(2, 5);
                    int patchType = Random.Range(1, 4);
                    if (patchType == 1) patchType = 0; //turn rough into fairway
                    AddHazardPatch(patchType, currentPos, patchSize);
                    currentPos += patchSize + Random.Range(0, 5);
                }
                //small green
                greenStartOffsetMin = 10;
                greenStartOffsetMax = 16;
                greenLengthMax = 8;
                break;
        }
        //Tee box is always fairway
        ReplacePieceAt(0, (int)CoursePieces.FAIRWAY);
        //Add in green
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
            int carry = selectedClub.GetComponent<Draggable>().Carry / 10;
            int roll = selectedClub.GetComponent<Draggable>().Roll / 10;
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
    public void HitBall()
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
        //do sand after discarding used cards
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
        //clean up
        handRef.DrawCard(1);
        if (GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Caddie 4") > 0)
            luck++;
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
            Vector3 localOutOfBoundsPos = new Vector3(swing.landIndex * childWidth - 7.5f, 1.25f, 0);
            localOutOfBoundsPos.y = courseLayout[swing.startIndex].transform.position.y;
            Vector3 worldOutOfBoundsPos = courseDisplay.transform.TransformPoint(localOutOfBoundsPos);
            Vector3 localTailEndPos = new Vector3(swing.endIndex * childWidth - 7.5f, 1.25f, 0);
            Vector3 worldTailEndPos = courseDisplay.transform.TransformPoint(localTailEndPos);
            DrawArc(startPos, worldOutOfBoundsPos, worldTailEndPos.x, boundsPrefab);
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
        int carry = selectedClub.GetComponent<Draggable>().Carry/10;
        int roll = selectedClub.GetComponent<Draggable>().Roll/10;
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
        swing.landIndex = start + Mathf.Max(carry + power/10, 1) * direction;
        int end = swing.landIndex;
        if(swing.landIndex >= courseLayout.Count || swing.landIndex < 0)
        {
            //Is landing out of bounds
            hasRoll = false;
        }
        if (hasRoll)
            end += Mathf.Max(roll - pinpoint / 10, 0) * direction;
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
        int carry = selectedClub.GetComponent<Draggable>().Carry;
        int roll = selectedClub.GetComponent<Draggable>().Roll;
        HitBall();
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

    //debug tool to quickly go through holes
    public void SkipHole()
    {
        strokeCount = 4;
        GoToNextHole();
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
        scores.Add(strokeCount);
        //If just finished a course, save course data and check for loss
        if(holeNum >= 9)
        {
            currentPlaythrough.Add(GetCurrentCourseData());
            int totalScore = 0;
            foreach (int score in scores)
                totalScore += score;
            //if(courseNum >= 2) //debug test to end after 2nd hole
            if(totalScore - 36 >= rivalScores[currentRival])
            {
                //you lost
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene("Lose");
                currentPlaythrough[courseNum - 1].lostRun = true;
                return;
            }
            if(courseNum >= 5)
            {
                //you won
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene("Lose");
                return;
            }
        }
        //Go to upgrade screen
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("New Card");
    }

    //This is called once the New Card scene is finished loading
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "New Card")
        {
            GetComponent<BoxCollider2D>().enabled = false;
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
            if (currentPlaythrough[currentPlaythrough.Count - 1].lostRun)
            {
                GameObject.Find("EndMessage").GetComponent<TextMeshProUGUI>().text = "You Lost";
            }
            else
            {
                GameObject.Find("EndMessage").GetComponent<TextMeshProUGUI>().text = "You Won";
            }
            int index = 0;
            foreach (scorecard sc in GameObject.Find("RecapParent").GetComponentsInChildren<scorecard>())
            {
                //update recap for each course played through
                if(index < currentPlaythrough.Count)
                {
                    sc.gameObject.SetActive(true);
                    sc.ShowRecap(currentPlaythrough[index]);
                }
                else
                {
                    sc.gameObject.SetActive(false);
                }
                index++;
            }
            courseDisplay.SetActive(false);
            GameObject.Find("GameManager").GetComponent<Hand>().RemoveDeck();
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

    //return a courseData obj of the current course
    public CourseData GetCurrentCourseData()
    {
        CourseData courseData = new()
        {
            pars = pars,
            scores = scores,
            rival = currentRival,
            courseNum = courseNum,
            courseType = (int)courseType
        };
        return courseData;
    }
}
