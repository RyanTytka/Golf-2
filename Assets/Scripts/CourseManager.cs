using DG.Tweening;
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
        if (GameObject.Find("SkipHoleButton") != null)
            GameObject.Find("SkipHoleButton").GetComponent<Button>().onClick.AddListener(GameObject.Find("CourseManager").GetComponent<Course>().SkipHole);
        if (GameObject.Find("AddTees") != null)
            GameObject.Find("AddTees").GetComponent<Button>().onClick.AddListener(GameObject.Find("CourseManager").GetComponent<Course>().AddTees);
        if (GameObject.Find("NextHole") != null)
            GameObject.Find("NextHole").GetComponent<Button>().onClick.AddListener(GameObject.Find("CourseManager").GetComponent<Course>().AddHole);
        //set up UI
        Button startButton = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().startButton.GetComponent<Button>();
        startButton.onClick.AddListener(GameObject.Find("GameManager").GetComponent<mainMenuUI>().ScrollDown);
        startButton.onClick.AddListener(() => { StartGame(false); });
        Button quitButton = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().quitButton.GetComponent<Button>();
        quitButton.onClick.AddListener(CloseGame);
        Button tutorialButton = GameObject.Find("ReferenceManager").GetComponent<referenceManager>().tutorialButton.GetComponent<Button>();
        tutorialButton.onClick.AddListener(() =>
        {
            GameObject.Find("ReferenceManager").GetComponent<referenceManager>().tutorialCanvas.GetComponent<tutorialManager>().OpenTutorial();
        });
        if (courseManagerObj == null)
        {
            //set up initial game state
            courseManagerObj = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
            for (int i = 0; i < rivalImages.Count; i++)
            {
                availRivals.Add(i);
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public List<GameObject> courseLayout = new();
    public List<GameObject> coursePieces;
    public GameObject courseDisplay; //the child object that holds all of the course pieces
    public Hand handRef;
    public int courseNum = 0;
    public int holeNum = 0; //what number hole you are on
    public List<int> scores = new List<int>();
    public List<int> pars = new List<int>();
    public GameState gameState = GameState.MAIN_MENU;
    public enum CourseType
    {
        plains,
        forest,
        hills,
        beach,
        desert,
    }
    public enum GameState
    {
        MAIN_MENU,
        PLAYING,
        SHOWING_SCORE,
        NEW_CARD_SELECT,
        SHOP,
        END_SCREEN
    }
    public enum CoursePieces
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
    public GameObject currentDot;
    public GameObject ballObj;
    public int luck; //ignore a hazard then reduce luck by 1. Persists between shots but not holes
    public GameObject finishText; //the text obj that displays whether you got par/etc
    public int tees = 0;
    public bool comingFromShop; //set to true after shop so you know to create new course
    public CourseType courseType;
    public int nextCourse = 0; //if -1, choose random course. Otherwise use this
    public bool canPlayBall = true; //set false by certain abilities

    //dragging the course
    private bool isDragging = false;
    private float startMouseX;
    private float startChildX;
    private float minX;
    private float maxX;
    public float childWidth;
    public float keyboardScrollSpeed = 5f;
    public float minKeyboardScrollSpeed = 5f;
    public float maxKeyboardScrollSpeed = 25f;
    private bool isBallMoving = false;

    //status effects
    public int power = 0;
    public int pinpoint = 0;
    public List<Sprite> statusEffectIcons;

    //rivals
    public List<string> rivalNames = new List<string>();
    public List<string> rivalDescriptions = new List<string>();
    public List<int> rivalScores = new List<int>();
    public List<Sprite> rivalImages = new List<Sprite>();
    public int currentRival = -1;
    public List<int> availRivals = new(); //tracks which rivals have already been played so no duplicates
    public bool canPlayAbilities = true;

    //pause state
    public bool paused = false;

    //putting
    int[] puttDistances; //distance <= [0] = 1 putt, d <= [1] = 2 putt, else 3 putt
    //default = [1, 4]. Resets each hole but can be modified
    public List<GameObject> puttMeterTextObjs; //text objs that display your current putt ranges

    //exit the application
    public void CloseGame()
    {
        Application.Quit();
    }

    void Update()
    {
        //escape key to pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (holeNum > 0) //can't pause while at main menu
                TogglePause();
        }
        if (paused) return;
        //show putting distances
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (gameState == GameState.PLAYING) //only show on course
            {
                int holePos = 0, greenStart = 0, greenEnd = 0;
                bool onGreenFlag = false;
                for (int i = 0; i < courseLayout.Count; i++)
                {
                    if (courseLayout[i].GetComponent<CoursePiece>().myType == (int)CoursePieces.GREEN)
                    {
                        if (onGreenFlag == false)
                        {
                            greenStart = i;
                        }
                        onGreenFlag = true;
                        greenEnd = i;
                    }
                    if (courseLayout[i].GetComponent<CoursePiece>().myType == (int)CoursePieces.HOLE)
                        holePos = i;
                }
                //3 Putt 
                for (int i = greenStart; i <= greenEnd; i++)
                {
                    if (i != holePos)
                    {
                        courseLayout[i].GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(true);
                        courseLayout[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                        courseLayout[i].GetComponentInChildren<TextMeshProUGUI>().text = "+3";
                    }
                }
                //2 Putt 
                for (int i = Mathf.Max(greenStart, holePos - puttDistances[1]); i <= Mathf.Min(greenEnd, holePos + puttDistances[1]); i++)
                {
                    if (i != holePos)
                    {
                        courseLayout[i].GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.8f, 0.5f, 0f);
                        courseLayout[i].GetComponentInChildren<TextMeshProUGUI>().text = "+2";
                    }
                }
                //1 Putt
                for (int i = Mathf.Max(greenStart, holePos - puttDistances[0]); i <= Mathf.Min(greenEnd, holePos + puttDistances[0]); i++)
                {
                    if (i != holePos)
                    {
                        courseLayout[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
                        courseLayout[i].GetComponentInChildren<TextMeshProUGUI>().text = "+1";
                    }
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            //remove putt distance indicators
            if (gameState == GameState.PLAYING) //only if on course
            {
                int greenStart = 0, greenEnd = 0;
                bool onGreenFlag = false;
                for (int i = 0; i < courseLayout.Count; i++)
                {
                    if (courseLayout[i].GetComponent<CoursePiece>().myType == (int)CoursePieces.GREEN)
                    {
                        if (onGreenFlag == false)
                        {
                            greenStart = i;
                        }
                        onGreenFlag = true;
                        greenEnd = i;
                    }
                }
                //3 Putt 
                for (int i = greenStart; i <= greenEnd; i++)
                {
                    courseLayout[i].GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(false);
                }
            }
        }
        // Animate shot arc          
        float scrollSpeed = 1.5f;
        float offset = Time.time * scrollSpeed;
        GetComponent<LineRenderer>().material.SetTextureOffset("_MainTex", new Vector2(-offset, 0));
        //do not let user move course while swinging
        if (isBallMoving) return;
        // Handle mouse input
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
                    GameObject bgm = GameObject.Find("BackgroundManager");
                    if (bgm != null)
                        bgm.GetComponent<backgroundManager>().StorePositions();
                    break;
                }
            }
        }
        float movementDelta = 0f;
        if (isDragging && Input.GetMouseButton(0))
        {
            float currentMouseX = mouseWorldPos.x;
            float deltaX = currentMouseX - startMouseX;
            Vector3 newLocalPos = courseDisplay.transform.localPosition;
            newLocalPos.x = Mathf.Clamp(startChildX + deltaX, minX, maxX);
            courseDisplay.transform.localPosition = newLocalPos;

            float courseDelta = newLocalPos.x - startChildX;
            GameObject bgm = GameObject.Find("BackgroundManager");
            if (bgm != null)
                bgm.GetComponent<backgroundManager>().UpdatePositions(courseDelta);

            // Update startChildX to prevent snapping back
            startChildX = newLocalPos.x;
            startMouseX = currentMouseX;
        }
        // Handle keyboard input
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Sync the startChildX with the current position to prevent snapping
            startChildX = courseDisplay.transform.localPosition.x;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movementDelta -= Time.deltaTime * keyboardScrollSpeed;
            keyboardScrollSpeed = Mathf.Min(keyboardScrollSpeed * (1f + Time.deltaTime), maxKeyboardScrollSpeed);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            movementDelta += Time.deltaTime * keyboardScrollSpeed;
            keyboardScrollSpeed = Mathf.Min(keyboardScrollSpeed * (1f + Time.deltaTime), maxKeyboardScrollSpeed);
        }
        else
        {
            keyboardScrollSpeed = minKeyboardScrollSpeed;
        }
        if (isDragging || movementDelta != 0f)
        {
            float targetX = startChildX + movementDelta;
            float clampedX = Mathf.Clamp(targetX, minX, maxX);
            Vector3 newLocalPos = courseDisplay.transform.localPosition;
            newLocalPos.x = clampedX;
            courseDisplay.transform.localPosition = newLocalPos;
            float courseDelta = clampedX - startChildX;
            GameObject bgm = GameObject.Find("BackgroundManager");
            if (bgm != null)
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
            // Only update startChildX if dragging with mouse
            if (!Input.GetMouseButton(0)) startChildX = courseDisplay.transform.localPosition.x;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    //public method to toggle pause and set pauseCanvas active
    public void TogglePause()
    {
        paused = !paused;
        GameObject.Find("ReferenceManager").GetComponent<referenceManager>().pauseCanvas.SetActive(paused);
        if (SceneManager.GetActiveScene().name == "Course")
        {
            //show scorecard
            GameObject.Find("ReferenceManager").GetComponent<referenceManager>().pauseScorecard.GetComponent<scorecard>().ShowCurrentCard();
        }
        //add listeners to button
        referenceManager rm = GameObject.Find("ReferenceManager").GetComponent<referenceManager>();
        rm.pauseResumeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        rm.pauseResumeButton.GetComponent<Button>().onClick.AddListener(TogglePause);
        rm.mainMenuButton.GetComponent<Button>().onClick.RemoveAllListeners();
        rm.mainMenuButton.GetComponent<Button>().onClick.AddListener(ToMainMenu);
        if (SceneManager.GetActiveScene().name == "Shop")
        {
            rm.settingsButton.GetComponent<Button>().onClick.AddListener(rm.tutorialCanvas.GetComponent<tutorialManager>().OpenSettings);
        }
    }

    public void ToMainMenu()
    {
        //clean up game state and reload main menu
        paused = false;
        Destroy(GameObject.Find("GameManager"));
        Destroy(GameObject.Find("CourseManager"));
        Destroy(GameObject.Find("Music Manager"));
        SceneManager.LoadScene("Course");
    }

    public void StartGame(bool isTutorial = false)
    {
        gameState = GameState.PLAYING;
        NewCourse(isTutorial);
        GameObject.Find("GameManager").GetComponent<Hand>().StartGame();
    }

    public void NewCourse(bool isTutorial = false)
    {
        //set initial course data
        ballObj.SetActive(true);
        holeNum = 0; //note: this will be incremented once before each hole
        courseNum++;
        if (isTutorial) nextCourse = 0; //tutorial is always plains
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
        if (isTutorial) currentRival = 1; //tutorial is always rival ???
        if (currentRival == -1) //if it is not -1, just use what it is currently set to
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
        scores = new();
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
        if (isTutorial) pars[0] = 4;
        //start first hole
        NewHole(isTutorial);
    }

    public int test_par = -1; //use this if you want to set the par to specific num
    //Generate new hole
    public void NewHole(bool isTutorial = false)
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
        puttDistances = new int[] { 1, 4 };

        GetComponent<BoxCollider2D>().enabled = true;
        //Generate Fairway
        CoursePieces fairwayType = CoursePieces.FAIRWAY;
        int lengthMod = 0;
        switch (courseType)
        {
            case CourseType.plains:
                lengthMod = 3;
                break;
            case CourseType.desert:
                // fairwayType = CoursePieces.SAND;
                lengthMod = 0;
                break;
            case CourseType.beach:
                lengthMod = -2;
                break;
            case CourseType.hills:
                lengthMod = 1;
                break;
            case CourseType.forest:
                lengthMod = -1;
                // fairwayType = CoursePieces.ROUGH;
                break;
        }
        int holeLength = Random.Range(32, 38) + courseNum * 4 + lengthMod; //full length of course
        if (test_par == 3 || pars[holeNum - 1] == 3)
            holeLength -= (7 + 3 * courseNum);
        if (test_par == 5 || pars[holeNum - 1] == 5)
            holeLength += (7 + 3 * courseNum);
        int[] tutorialCourse = { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0,
                                 0, 0, 1, 1, 1, 0, 0, 0, 2, 2,
                                 0, 0, 0, 0, 0, 3, 3, 3, 0, 0,
                                 4, 4, 5, 4, 4, 4, 0, 0, 0, 0};
        if (isTutorial) holeLength = tutorialCourse.Length;
        for (int i = 0; i < holeLength; i++)
        {
            int pieceType = isTutorial ? tutorialCourse[i] : (int)fairwayType;
            GameObject fairway = Instantiate(coursePieces[pieceType], courseDisplay.transform);
            fairway.GetComponent<CoursePiece>().myIndex = i;
            courseLayout.Add(fairway);
        }
        if (!isTutorial)
        {
            //Create Hazards
            int currentPos = Random.Range(1, 8);
            int greenStartOffsetMin = 6; //space behind green before out of bounds
            int greenStartOffsetMax = 10;
            int greenLengthMin = 6 - courseNum / 3;
            int greenLengthMax = 10 - courseNum / 3;
            switch (courseType)
            {
                case CourseType.plains:
                    //small patches of various hazards
                    while (currentPos < courseLayout.Count)
                    {
                        int patchSize = Random.Range(1, 4 + courseNum / 2);
                        int patchType = WeightedRandomInt(
                            new List<int> { 1, 2, 3 }, //rough, sand, water
                            new List<int> { 20, 10 + courseNum, 10 + courseNum });
                        if (patchType == 1)
                            patchSize += Random.Range(1, 3);
                        AddHazardPatch(patchType, currentPos, patchSize);
                        currentPos += patchSize + Random.Range(6 - courseNum / 2, 12 - courseNum / 2);
                    }
                    //large green
                    greenStartOffsetMin += 2;
                    greenStartOffsetMax += 2;
                    greenLengthMin += 2;
                    greenLengthMax += 2;
                    break;
                case CourseType.desert:
                    //large patches of rough and sand
                    while (currentPos < courseLayout.Count)
                    {
                        int patchSize = Random.Range(4, 8);
                        int patchType = WeightedRandomInt(
                            new List<int> { 1, 2 }, //rough, sand
                            new List<int> { 7, 7 + courseNum });
                        if (patchType == 2)
                            patchSize -= 3 - courseNum / 3;
                        AddHazardPatch(patchType, currentPos, patchSize);
                        currentPos += patchSize + Random.Range(6 - courseNum / 3, 10);
                    }
                    //large green
                    greenStartOffsetMin = 8;
                    greenLengthMin += 2;
                    greenLengthMax += 2;
                    break;
                case CourseType.beach:
                    //patches of sand and water
                    while (currentPos < courseLayout.Count)
                    {
                        int patchSize = Random.Range(1, 4 + courseNum / 2);
                        int patchType = WeightedRandomInt(
                            new List<int> { 1, 2, 3 }, //rough, sand, water
                            new List<int> { 10, 15 + courseNum, 20 + courseNum });
                        AddHazardPatch(patchType, currentPos, patchSize);
                        currentPos += patchSize + Random.Range(5 - courseNum / 3, 12 - courseNum / 2);
                    }
                    //small green
                    greenStartOffsetMax -= 2;
                    greenLengthMax -= 2;
                    break;
                case CourseType.hills:
                    //patches of water and large rough
                    while (currentPos < courseLayout.Count)
                    {
                        int patchSize = Random.Range(3, 8);
                        int patchType = WeightedRandomInt(
                            new List<int> { 1, 3 }, //rough, water
                            new List<int> { 25, 10 + courseNum });
                        if (patchType == 3) patchSize -= 3 - courseNum / 3;
                        AddHazardPatch(patchType, currentPos, patchSize);
                        currentPos += patchSize + Random.Range(4 - courseNum / 3, 8 - courseNum / 3);
                    }
                    //larger green
                    greenStartOffsetMax += 2;
                    greenLengthMax += 2;
                    break;
                case CourseType.forest:
                    //patches of rough, sand, and water
                    while (currentPos < courseLayout.Count)
                    {
                        int patchSize = Random.Range(1 + courseNum / 3, 4);
                        int patchType = WeightedRandomInt(
                            new List<int> { 1, 2, 3 }, //rough, sand, water
                            new List<int> { 30, 10 + courseNum, 10 + courseNum });
                        if (patchType == 1)
                            patchSize += 2;
                        AddHazardPatch(patchType, currentPos, patchSize);
                        currentPos += patchSize + Random.Range(3, 7 - courseNum / 2);
                    }
                    //small behind green
                    greenStartOffsetMin -= 2;
                    greenStartOffsetMax -= 2;
                    break;
            }
            //Tee box is always fairway
            ReplacePieceAt(0, (int)CoursePieces.FAIRWAY);
            //Add in green
            int greenLength = Random.Range(greenLengthMin, greenLengthMax);
            int startOfGreen = holeLength - Random.Range(greenStartOffsetMin, greenStartOffsetMax) - greenLength;
            for (int i = startOfGreen; i < Mathf.Min(startOfGreen + greenLength, courseLayout.Count); i++)
            {
                ReplacePieceAt(i, (int)CoursePieces.GREEN);
            }
            ReplacePieceAt(startOfGreen - 1, Random.Range(0, 2)); //Dont let there be water before the green
                                                                  //Place Hole on the Green
            int holePlacement = Mathf.Clamp(startOfGreen + Random.Range(1, greenLength - 1), startOfGreen, courseLayout.Count - 1);
            ReplacePieceAt(holePlacement, (int)CoursePieces.HOLE);
            //make last piece of the course grass (so it doesnt end in water)
            ReplacePieceAt(courseLayout.Count - 1, (int)CoursePieces.FAIRWAY);
        }
        DisplayCourse();
        // Calculate drag limits for coursedisplay, based on screen size and hole size
        Vector3 newLocalPos = courseDisplay.transform.localPosition;
        maxX = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x + childWidth / 2;
        float childCount = courseLayout.Count;
        minX = maxX - childCount * childWidth - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x * 2;
        newLocalPos.x = maxX;
        courseDisplay.transform.localPosition = newLocalPos;
        //generate bg elements
        if (GameObject.Find("BackgroundManager") != null)
            GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().SetSprites((int)courseType);
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
            if (startIndex + i < courseLayout.Count)
                ReplacePieceAt(startIndex + i, hazardType);
        }
    }

    //helper method that gives you a random key based off weights
    private int WeightedRandomInt(List<int> keys, List<int> weights)
    {
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }
        float f = Random.Range(0, (float)totalWeight);
        int cumulative = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += weights[i];
            if (f < cumulative)
                return keys[i];
        }
        return -1;
    }

    public void DisplayCourse()
    {
        if (GameObject.Find("RivalDisplay") != null)
        {
            GameObject.Find("RivalDisplay").GetComponent<rivalDisplay>().idNum = currentRival;
            GameObject.Find("RivalDisplay").GetComponent<rivalDisplay>().UpdateView();
            GameObject.Find("StrokeCount").GetComponent<TextMeshProUGUI>().text = strokeCount.ToString();
            GameObject.Find("HoleCount").GetComponent<TextMeshProUGUI>().text = holeNum.ToString();
            GameObject.Find("TeeCount").GetComponent<TextMeshProUGUI>().text = tees.ToString();
        }
        //Put pieces in their place
        int prevPieceType = 0;
        for (int i = 0; i < courseLayout.Count; i++)
        {
            //set position
            GameObject go = courseLayout[i];
            go.transform.localPosition = new Vector2(i * childWidth, 1.0f);
            //set art
            //Type: 0 = Fairway, 1 = Rough, 2 = Sand, 3 = Water, 4 = Green, 5 = Hole, 6 = Out of Bounds
            //Offset: 0 = single, 1 = small left, 2 = small right, 3 = large left, 4 = large middle, 5 = large right
            int myType = go.GetComponent<CoursePiece>().myType;
            int offset = 4;
            if (prevPieceType != myType)
            {
                offset = 3; // this is the first piece of this type
                if (i < courseLayout.Count - 1 && myType != courseLayout[i + 1].GetComponent<CoursePiece>().myType)
                {
                    //this is a single piece
                    offset = 0;
                }
            }
            else if (i < courseLayout.Count - 1 && myType != courseLayout[i + 1].GetComponent<CoursePiece>().myType)
                offset = 5; //this is the last piece
            if (myType == 5) { offset = 4; myType = 4; } //hole is always middle piece
            if (myType == 4 && courseLayout[i + 1].GetComponent<CoursePiece>().myType == 5) //green piece before hole
                if (courseLayout[i - 1].GetComponent<CoursePiece>().myType == 4)
                    offset = 4;
                else
                    offset = 3;
            if (myType == 4 && courseLayout[i - 1].GetComponent<CoursePiece>().myType == 5) //green piece after hole
                if (courseLayout[i + 1].GetComponent<CoursePiece>().myType == 4)
                    offset = 4;
                else
                    offset = 5;
            go.GetComponent<SpriteRenderer>().sprite = courseArt[myType * 6 + offset];
            prevPieceType = myType;
        }
        //Put ball in its place
        ballObj.transform.localPosition = new Vector3(ballPos * childWidth, 1f, -1);
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
        UpdateStatusEffectDisplay();
    }

    // Returns the distance from given point to the hole
    public int DistanceToHole(int startIndex)
    {
        //Find where the hole is
        foreach (GameObject go in courseLayout)
        {
            if (go.GetComponent<CoursePiece>().pieceName == "Hole")
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

    Vector3 lastPosition;
    private void RollBall(bool isRolling)
    {
        //calculate roll
        float ballRadius = 0.5f;
        Vector3 currentPos = ballObj.transform.position;
        Vector3 delta = currentPos - lastPosition;
        int direction = delta.x > 0 ? 1 : -1;
        //ball rolls much slower in the air
        float distance = isRolling ? delta.magnitude : delta.magnitude * 0.1f;
        //TODO: need to calculate if going forwards or backwards
        if (distance > 0.0001f)
        {
            float rotationDegrees = (distance / (2f * Mathf.PI * ballRadius)) * -360f * direction;

            ballObj.transform.Rotate(Vector3.forward, rotationDegrees, Space.World);
        }

        lastPosition = currentPos;
    }

    //Move the ball its carry distance then the roll distance
    public void HitBall()
    {
        //animate the ball
        LineRenderer line = GetComponent<LineRenderer>();
        Vector3[] path = new Vector3[line.positionCount];
        line.GetPositions(path);
        if (!line.useWorldSpace)
        {
            for (int i = 0; i < path.Length; i++)
                path[i] = line.transform.TransformPoint(path[i]);
        }
        SwingResult swing = CalculateSwing();
        int lastTriggeredIndex = swing.landIndex - 1;
        Vector3 rollStart = path[path.Length - 2];
        Vector3 rollEnd = path[path.Length - 1];
        float segmentLength = Vector3.Distance(rollStart, rollEnd);
        Tween DOPath = null;
        bool isRolling = false;
        //calculate shot timing
        float ratio;
        if (swing.landIndex - swing.endIndex == 0)
            ratio = 0;
        else
            ratio = Mathf.Abs((swing.landIndex - swing.startIndex) / (swing.landIndex - swing.endIndex));
        AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f, 0f, -2f), new Keyframe(1f + ratio, 0f, -2f, 0f));
        // need less linear curve. short shots too fast and long shots too slow
        float shotLength = Mathf.Abs((swing.endIndex - swing.startIndex)) / 10f;
        isBallMoving = true;
        GameObject.Find("SwingButton").GetComponent<Button>().interactable = false;
        GameObject.Find("MulliganButton").GetComponent<Button>().interactable = false;
        int luckUsed = 0;
        //send the ball on its path
        DOPath = ballObj.transform.DOPath(path, shotLength, PathType.Linear).SetEase(Ease.OutCubic).OnUpdate(() =>
        {
            RollBall(isRolling);
            //check if the ball is still in the air
            float travelled = ballObj.transform.position.x - rollStart.x;
            if (travelled < 0) return;
            //trigger tiles as your roll across them
            isRolling = true;
            float rollT = Mathf.Clamp01(travelled / segmentLength);
            int totalTiles = Mathf.Abs(swing.endIndex - swing.landIndex);
            int rollDirection = swing.endIndex > swing.landIndex ? 1 : -1;
            int currentIndex = swing.landIndex + rollDirection * Mathf.FloorToInt(rollT * (float)totalTiles);
            if (currentIndex != lastTriggeredIndex)
            {
                bool usingLuck = false;
                lastTriggeredIndex = currentIndex;
                if (courseLayout[currentIndex].GetComponent<CoursePiece>().myType == (int)CoursePieces.WATER ||
                    courseLayout[currentIndex].GetComponent<CoursePiece>().myType == (int)CoursePieces.SAND ||
                    courseLayout[currentIndex].GetComponent<CoursePiece>().myType == (int)CoursePieces.ROUGH)
                {
                    //check if hazard is being applied or luck is being used
                    if (luck + swing.luckGained > luckUsed)
                    {
                        luckUsed++;
                        usingLuck = true;
                    }
                }
                courseLayout[currentIndex].GetComponent<CoursePiece>().RolledOver(usingLuck);
            }
        }).OnComplete(AfterHitBall);
        //make the camera move with the ball
        //first, move the camera to where the ball is
        //Vector3 courseMoveTo = courseDisplay.transform.position;
        //courseMoveTo.x -= swing.endIndex - swing.startIndex;
        //float cameraMoveDuration = 2f;
        //courseDisplay.transform.DOMove(courseMoveTo, cameraMoveDuration);
        //if the shot goes far enough, the camera travels with the ball
        //finally, settle the camera where the ball lies
    }

    //called when the ball hit animation is complete
    public void AfterHitBall()
    {
        isBallMoving = false;
        GameObject.Find("SwingButton").GetComponent<Button>().interactable = true;
        GameObject.Find("MulliganButton").GetComponent<Button>().interactable = true;
        SwingResult swing = CalculateSwing();
        //card effects
        if (selectedClub.GetComponent<Draggable>().cardName == "Shovel Wedge")
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Sand")
                GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(2 + selectedClub.GetComponent<Draggable>().rarity);
        //can play another caddie
        GameObject.Find("GameManager").GetComponent<Hand>().playedCaddie = false;
        GameObject.Find("GameManager").GetComponent<Hand>().playedAbility = false;
        //calculate swing
        string ballName = selectedBall != null ? selectedBall.GetComponent<Draggable>().cardName : "";
        luck += swing.luckGained;
        luck -= swing.luckUsed;
        if (!(selectedClub.GetComponent<Draggable>().cardName == "Foot Wedge" &&
            (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Sand" ||
            courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Rough")))
            ChangeStrokeCount(1);
        //card effects
        //clubs
        if (selectedClub.GetComponent<Draggable>().cardName == "Swiss Army Wedge")
        { //Swing - Gain {1/2/2} Luck, {1/2/2} Putt Range, Draw {1/1/2} card, Gain {1/1/2} Tees.
            int rarity = selectedClub.GetComponent<Draggable>().rarity;
            luck += rarity == 0 ? 1 : 2;
            ModifyPuttDistances(rarity == 0 ? 1 : 2);
            tees += rarity == 2 ? 2 : 1;
            GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(rarity == 2 ? 2 : 1);
        }
        //if out of bounds
        if (swing.endIndex >= courseLayout.Count)
        {
            //ball stays where it is and you lose a stroke
            ChangeStrokeCount(1);
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
                    if (selectedBall != null && selectedBall.GetComponent<Draggable>().cardName == "Jungle Ball")
                        power += 10;
                    else
                        power -= 10;
                }
            }
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Water")
            {
                //move up to next non water space and take stroke penalty (if not lucky)
                while (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Water")
                    ballPos++;
                //Item effects
                foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Bubba Bunker"))
                {
                    tees += 2 + rarity;
                }
                if (luck > 0)
                {
                    luck--;
                }
                else
                {
                    //rubber duck ball does not take stroke penalty
                    if (selectedBall == null || selectedBall.GetComponent<Draggable>().cardName != "Rubber Duck Ball")
                    {
                        ChangeStrokeCount(1);
                        if (currentRival == 1)
                            ChangeStrokeCount(1);
                    }
                }
            }
            pinpoint = 0;
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Fairway")
            {
                //item effects
                if (ballName == "Ace Ball")
                {
                    //Draw 2/3/4
                    int drawAmount = selectedBall.GetComponent<Draggable>().rarity + 2;
                    GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(drawAmount);
                }
                if (selectedClub.GetComponent<Draggable>().cardName == "Club of Riches")
                    tees += 2 + selectedClub.GetComponent<Draggable>().rarity;
            }
        }
        //card effects
        if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Hole")
        {
            //ball effects
            if (selectedBall != null)
            {
                if (selectedBall.GetComponent<Draggable>().cardName == "")
                {
                    tees += 3 + selectedBall.GetComponent<Draggable>().rarity;
                }
            }
        }
        //club effects
        if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Green")
        {
            if (selectedClub.GetComponent<Draggable>().cardName == "Pitching Wedge" ||
                selectedClub.GetComponent<Draggable>().cardName == "Sand Wedge")
            {
                puttDistances[0]++;
                puttDistances[1]++;
                UpdateStatusEffectDisplay();
            }
        }
        if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Fairway")
        {
            if (selectedClub.GetComponent<Draggable>().cardName == "7 Iron" ||
                selectedClub.GetComponent<Draggable>().cardName == "9 Iron")
            {
                puttDistances[0]++;
                puttDistances[1]++;
            }
        }
        //deselect current club and ball and update hand
        if (selectedClub != null)
        {
            //paper club tosses itself
            if (selectedClub.GetComponent<Draggable>().cardName == "Paper Club")
            {
                selectedClub.GetComponent<Draggable>().AnimateDiscard(true);
            }
            //lead ball destroys club used
            else if (selectedBall != null && selectedBall.GetComponent<Draggable>().cardName == "Lead Ball")
            {
                selectedClub.GetComponent<Draggable>().AnimateDiscard(true);
            }
            else if (selectedClub.GetComponent<Draggable>().isDriver)
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
                go.GetComponent<Draggable>().AnimateDiscard(true);
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
                if (handObj.hand.Count > 0)
                    handObj.DiscardCard(handObj.hand[Random.Range(0, handObj.hand.Count)]);
            }
            //Item Effects
            if (ballName == "Beach Ball")
            {
                //Draw 3/4/5
                GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(3 + selectedBall.GetComponent<Draggable>().rarity);
            }
            foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Bubba Bunker"))
            {
                tees += 2 + rarity;
            }
        }
        //If on green, perform a putt
        if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Green")
        {
            DisplayCourse();
            int distanceToHole = Mathf.Abs(DistanceToHole(ballPos));
            int puttCount;
            if (distanceToHole <= puttDistances[0]) puttCount = 1;
            else if (distanceToHole <= puttDistances[1]) puttCount = 2;
            else puttCount = 3;
            //animate the ball being hit for each putt
            Vector3 currentPos = ballObj.transform.position;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(1.0f); //wait for cards to finish discarding
            int holeIndex = courseLayout.Find(obj =>
                obj.GetComponent<CoursePiece>().myType == (int)CoursePieces.HOLE
            ).GetComponent<CoursePiece>().myIndex;
            Vector3 target = courseLayout[holeIndex].transform.position;
            float durationPerPutt = 0.5f;
            for (int i = 0; i < puttCount; i++)
            {
                // Target is 75% towards the hole
                Vector3 nextPos = currentPos + (target - currentPos) * 0.75f;
                if (i == puttCount - 1)
                {
                    //make last putt actually reach hole
                    nextPos = target;
                }
                // Append movement
                sequence.Append(
                    ballObj.transform.DOMove(nextPos, durationPerPutt)
                             .SetEase(Ease.OutQuad).OnUpdate(() =>
                             {
                                 RollBall(true);
                             }).OnComplete(() =>
                             {
                                 ChangeStrokeCount(1);
                                 UpdateStatusEffectDisplay();
                             })
                );
                // small delay between putts
                sequence.AppendInterval(0.25f);
                currentPos = nextPos;
            }
            sequence.OnComplete(() =>
            {
                ballPos = holeIndex;
                GoToNextHole();
            });
            sequence.Play();
            return;
        }
        //if in the hole, go to next hole
        if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Hole")
        {
            GoToNextHole();
            return;
        }
        //clean up
        GameObject.Find("GameManager").GetComponent<Hand>().cardsTossed = 0;
        canPlayBall = true;
        canPlayAbilities = true;
        handRef.DrawCard(1);
        foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Ray Scratch"))
        {
            luck += 1 + rarity;
        }
        DisplayCourse();
        UpdateStatusEffectDisplay();
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
        public int luckGained;
    }
    //called by both hitball and highlight hit to calculate the swing
    private SwingResult CalculateSwing()
    {
        SwingResult swing = new();
        swing.roughHits = 0;
        //get base distances
        if (selectedClub == null)
            return swing; //cant swing without a club!
        int carry = selectedClub.GetComponent<Draggable>().Carry / 10;
        int roll = selectedClub.GetComponent<Draggable>().Roll / 10;
        bool hasRoll = true; //false if an effect makes this shot have 0 roll
        int tempLuck = 0; //luck that would be gained during this shot
        //card effects
        //clubs
        if (selectedClub.GetComponent<Draggable>().cardName == "Big Iron")
            if (courseLayout[ballPos].GetComponent<CoursePiece>().pieceName == "Rough")
                carry += 5 + (selectedClub.GetComponent<Draggable>().rarity * 3);
        if (selectedClub.GetComponent<Draggable>().cardName == "Hot Rod")
            carry += GameObject.Find("GameManager").GetComponent<Hand>().hand.Count;
        if (selectedClub.GetComponent<Draggable>().cardName == "The Showoff")
        {
            bool onlyClub = true;
            foreach (GameObject go in GameObject.Find("GameManager").GetComponent<Hand>().hand)
            {
                if (go.GetComponent<Draggable>().cardType == Draggable.CardTypes.Club && go != selectedClub)
                    onlyClub = false;
            }
            if (onlyClub)
            {
                carry += 5 + (selectedClub.GetComponent<Draggable>().rarity * 3);
                roll -= 2 + selectedClub.GetComponent<Draggable>().rarity;
            }
        }
        if (selectedClub.GetComponent<Draggable>().cardName == "The Loner")
            carry = Mathf.Max(1, carry - GameObject.Find("GameManager").GetComponent<Hand>().hand.Count + 1);
        if (selectedClub.GetComponent<Draggable>().cardName == "Bone Club")
            carry += GameObject.Find("GameManager").GetComponent<Hand>().cardsTossed;
        //caddies
        if (selectedClub.GetComponent<Draggable>().clubType == Draggable.ClubTypes.Iron)
        {
            foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Chip Johnson"))
            {
                carry += rarity == 2 ? 10 : 5 + (rarity * 2);
            }
        }
        foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Paula Parsons"))
        {
            carry += GameObject.Find("GameManager").GetComponent<Hand>().caddies.Count * (rarity + 1);
        }
        foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Elsa Woods"))
        {
            if (selectedClub.GetComponent<Draggable>().clubType == Draggable.ClubTypes.Wood)
            {
                carry += 3 + selectedClub.GetComponent<Draggable>().rarity;
                roll -= 3 + selectedClub.GetComponent<Draggable>().rarity;
            }
        }
        //rivals
        if (currentRival == 0)
        {
            if (selectedClub.GetComponent<Draggable>().isDriver)
                carry -= 3;
        }
        //balls
        if (selectedBall != null)
        {
            string ball = selectedBall.GetComponent<Draggable>().cardName;
            int ballRarity = selectedBall.GetComponent<Draggable>().rarity;
            switch (ball)
            {
                case "Distance Ball":
                    carry += 2 + ballRarity;
                    break;
                case "Breakfast Ball":
                    if (selectedClub.GetComponent<Draggable>().clubType == Draggable.ClubTypes.Wood)
                    {
                        hasRoll = false;
                        carry += ballRarity == 2 ? 10 : 5 + (ballRarity * 2);
                    }
                    break;
                case "Ice Ball":
                    roll += 4 + (ballRarity * 2);
                    break;
                case "Square Ball":
                    carry -= 2;
                    hasRoll = false;
                    break;
                case "Lead Ball":
                    carry += 5 + (ballRarity * 2);
                    break;
                case "Heat Seeking Ball":
                    tempLuck += ballRarity + 1;
                    break;
                case "Clover Ball":
                    carry += luck + ballRarity;
                    roll += luck;
                    break;
                case "Disco Ball":
                    carry += handRef.currentDeck.Count;
                    tempLuck += ballRarity;
                    break;
            }
        }
        int direction = 1;
        if (DistanceToHole(ballPos) < 0)
        {
            direction = -1;
        }
        int luckUsed = 0; //temporary count for using luck
        int rollAmount = 0;
        int start = ballPos;
        swing.startIndex = start;
        swing.landIndex = start + Mathf.Max(carry + power / 10, 1) * direction;
        int end = swing.landIndex;
        if (swing.landIndex >= courseLayout.Count || swing.landIndex < 0)
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
                break;
            }
            //Trigger effect of piece
            piece = courseLayout[i];
            switch (piece.GetComponent<CoursePiece>().pieceName)
            {
                case "Rough":
                    if (luck + tempLuck - luckUsed > 0)
                    {
                        luckUsed++;
                        rollAmount++;
                    }
                    else
                    {
                        if (currentRival == 6) //this rival stops rolling on rough
                            i = end;
                        else
                        {
                            rollAmount++;
                            swing.roughHits += 1;
                        }
                    }
                    break;
                case "Fairway":
                case "Green":
                    rollAmount++;
                    break;
                case "Sand":
                case "Water":
                    if (luck + tempLuck - luckUsed > 0)
                    {
                        luckUsed++;
                        rollAmount++;
                    }
                    else
                    {
                        i = end;
                    }
                    break;
                case "Hole":
                    // Stop immediately
                    i = end;
                    break;
            }
        }
        swing.endIndex = swing.landIndex + rollAmount * direction;
        swing.luckGained = tempLuck;
        swing.luckUsed = luckUsed;
        return swing;
    }

    //hit the ball
    public void Swing()
    {
        if (selectedClub == null) return; //need a club to swing
        HitBall();
    }

    public void UpdateStatusEffectDisplay()
    {
        if (GameObject.Find("StatusEffects") == null) return;
        GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[0].enabled = false;
        GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[1].enabled = false;
        GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[2].enabled = false;
        GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text = "";
        int tempPower = 0;
        foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Paula Parsons"))
        {
            tempPower += 10 * GameObject.Find("GameManager").GetComponent<Hand>().caddies.Count * (1 + rarity);
        }
        int statusIndex = 0;
        if (power + tempPower != 0)
        {
            GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text += (power + tempPower) + "\n";
            GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[statusIndex].enabled = true;
            GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[statusIndex].sprite = statusEffectIcons[0];
            statusIndex++;
        }
        if (pinpoint != 0)
        {
            GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text += pinpoint + "\n";
            GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[statusIndex].enabled = true;
            GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[statusIndex].sprite = statusEffectIcons[1];
            statusIndex++;
        }
        if (luck != 0)
        {
            GameObject.Find("StatusEffects").GetComponent<TextMeshProUGUI>().text += luck + "\n";
            GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[statusIndex].enabled = true;
            GameObject.Find("StatusEffects").GetComponentsInChildren<Image>()[statusIndex].sprite = statusEffectIcons[2];
        }
        //Update Putt Meter
        GameObject.Find("ReferenceManager").GetComponent<referenceManager>().puttMeterTextObjs[0].GetComponent<TextMeshProUGUI>().text = puttDistances[0].ToString();
        GameObject.Find("ReferenceManager").GetComponent<referenceManager>().puttMeterTextObjs[1].GetComponent<TextMeshProUGUI>().text = puttDistances[1].ToString();
        //Update stroke count
        GameObject.Find("StrokeCount").GetComponent<TextMeshProUGUI>().text = strokeCount.ToString();
    }

    public void DrawArc(Vector3 start, Vector3 end, float targetX, GameObject endObj)
    {
        float arcHeight = 2f;
        int arcSegments = 50;
        int totalPoints = arcSegments + 2;
        LineRenderer line = GetComponent<LineRenderer>();
        line.positionCount = totalPoints;
        start.z = -1;
        end.z = -1;
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
        Vector3 tailEnd = lastArcPoint;
        tailEnd.x = targetX;
        line.SetPosition(arcSegments + 1, tailEnd);
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
        strokeCount += 3;
        GoToNextHole();
    }

    public void ChangeStrokeCount(int amount)
    {
        strokeCount = Mathf.Max(0, strokeCount + amount);
    }

    public GameObject continueObj;
    public void GoToNextHole()
    {
        canPlayAbilities = true;
        gameState = GameState.SHOWING_SCORE;
        foreach (int rarity in GameObject.Find("GameManager").GetComponent<Hand>().HasCaddie("Ace Rollins"))
        {
            if (GameObject.Find("GameManager").GetComponent<Hand>().hand.Count >= 6 - rarity)
                ChangeStrokeCount(-1);
        }
        DisplayCourse();
        //Display Score
        string[] score = { "ACE", "HOLE IN ONE", "EAGLE", "BIRDIE", "PAR", "BOGEY", "DOUBLE BOGEY", "TRIPLE BOGEY" };
        continueObj = Instantiate(finishText, GameObject.Find("MainCanvas").transform);
        continueObj.transform.localPosition = new Vector3(0, 200, 56);
        if (strokeCount >= score.Length)
            continueObj.GetComponent<TextMeshProUGUI>().text = "+" + (strokeCount - pars[holeNum - 1]);
        else
            continueObj.GetComponent<TextMeshProUGUI>().text = Mathf.Max(0, strokeCount - pars[holeNum - 1] + 4).ToString();
        float t = Mathf.Min(Mathf.Max(strokeCount - 3, 0), 7.0f) / 7.0f;
        continueObj.GetComponent<TextMeshProUGUI>().color = Color.Lerp(Color.green, Color.red, t);
        continueObj.GetComponentInChildren<Button>().onClick.AddListener(ContinueButtonClick);
    }

    //called after a hole is completed and the player clicks the continue button
    public void ContinueButtonClick()
    {
        scores.Add(strokeCount);
        //if you lost or this is the last hole, go to the end screen
        if (holeNum >= 9)
        {
            //save course data
            currentPlaythrough.Add(GetCurrentCourseData());
            //clear last hole
            // foreach (GameObject go in courseLayout)
            // {
            //     Destroy(go);
            // }
            // courseLayout.Clear();
            //reset shot highlight
            GameObject.Find("CourseManager").GetComponent<LineRenderer>().positionCount = 0;
            if (currentDot != null)
                Destroy(currentDot);
            //clear continue text/button
            // Destroy(continueObj);
            //clear bg elements
            // GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().RemoveSprites();
            //check for loss
            int totalScore = 0;
            foreach (int score in scores)
                totalScore += score;
            //if(c.courseNum >= 2) //debug test to end after 2nd hole
            if (totalScore - 36 >= rivalScores[currentRival])
            {
                //you lost
                SceneManager.LoadScene("Lose");
                currentPlaythrough[courseNum - 1].lostRun = true;
                return;
            }
            //check for last course
            if (courseNum >= 5)
            {
                //you won
                SceneManager.LoadScene("Lose");
                return;
            }
        }
        //otherwise, go to the card reward screen
        //move the camera up
        GameObject.Find("GameManager").GetComponent<mainMenuUI>().HideMainMenu();
        GameObject.Find("GameManager").GetComponent<mainMenuUI>().ScrollUp(() =>
        {
            //set gamestate to card select once done moving up
            gameState = GameState.NEW_CARD_SELECT;
            GameObject.Find("GameManager").GetComponent<Hand>().RemoveDeck();
        });
        //set up new card select
        GetComponent<BoxCollider2D>().enabled = false;
        GameObject newCard = GameObject.Find("GameManager").GetComponent<Hand>().RandomUpgrade();
        int teeReward = Mathf.Max(pars[holeNum - 1] - strokeCount + 4, 1);
        if (currentRival == 4 && holeNum <= scores.Count && pars[holeNum - 1] < scores[holeNum - 1]) //must par or better to get tees against rival 4
            teeReward = 0;
        GameObject.Find("GameManager").GetComponent<NewCardManager>().ShowUI(teeReward, newCard);
        //GameObject.Find("TeesText").GetComponent<TextMeshProUGUI>().text = "(     +" + teeReward + ")";
        //GameObject.Find("SkipButton").GetComponent<Button>().onClick.AddListener
        //    (GameObject.Find("GameManager").GetComponent<Hand>().SkipUpgrade);
        //wait to clear the hole until the camera finished moving up




        ////clear current hole
        //foreach (GameObject go in courseLayout)
        //{
        //    Destroy(go);
        //}
        //courseLayout.Clear();
        //ballObj.SetActive(false);
        ////reset highlight
        //GetComponent<LineRenderer>().positionCount = 0;
        //if (currentDot != null)
        //    Destroy(currentDot);
        //scores.Add(strokeCount);
        ////If just finished a course, save course data and check for loss
        //if(holeNum >= 9)
        //{
        //    currentPlaythrough.Add(GetCurrentCourseData());
        //    int totalScore = 0;
        //    foreach (int score in scores)
        //        totalScore += score;
        //    //if(courseNum >= 2) //debug test to end after 2nd hole
        //    if(totalScore - 36 >= rivalScores[currentRival])
        //    {
        //        //you lost
        //        SceneManager.sceneLoaded += OnSceneLoaded;
        //        SceneManager.LoadScene("Lose");
        //        currentPlaythrough[courseNum - 1].lostRun = true;
        //        return;
        //    }
        //    if(courseNum >= 5)
        //    {
        //        //you won
        //        SceneManager.sceneLoaded += OnSceneLoaded;
        //        SceneManager.LoadScene("Lose");
        //        return;
        //    }
        //}
        ////Go to upgrade screen
        //SceneManager.sceneLoaded += OnSceneLoaded;
        //SceneManager.LoadScene("New Card");
        //GameObject.Find("BackgroundManager").GetComponent<backgroundManager>().RemoveSprites();
    }

    //Take 1 stroke to draw a card
    public void Mulligan()
    {
        ChangeStrokeCount(1);
        GameObject.Find("GameManager").GetComponent<Hand>().DrawCard(1);
        if (currentRival == 3)
            power -= 30;
        UpdateStatusEffectDisplay();
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


    //Debug Helper method to give yourself tees
    public void AddTees()
    {
        tees += 25;
        DisplayCourse();
    }

    //Debug Helper method to quickly skip holes
    public void AddHole()
    {
        holeNum++;
        DisplayCourse();
    }

    public void ModifyPuttDistances(int amount)
    {
        puttDistances[0] = Mathf.Max(0, puttDistances[0] + amount);
        puttDistances[1] = Mathf.Max(0, puttDistances[1] + amount);
    }
}
