using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class referenceManager : MonoBehaviour
{
    //main menu
    public GameObject startButton;
    public GameObject quitButton;
    public GameObject tutorialButton;
    public GameObject tutorialCanvas;
    //adding new card to deck
    public GameObject mainCamera;
    public GameObject newCardCanvas;
    public GameObject addTeesText;
    public GameObject rerollCostText;
    public GameObject rerollButton;
    public GameObject yourTees;
    public GameObject takeTeesButton;
    public GameObject takeCardButton;
    //pause canvas
    public GameObject pauseCanvas;
    public GameObject pauseScorecard;
    public GameObject pauseResumeButton; //resume button
    public GameObject pauseButton;
    public GameObject mainMenuButton;
    public GameObject settingsButton;
    public Slider musicSlider;
    public Slider sfxSlider;
    //hand highlight canvas
    public GameObject handHighlightCanvas;
    public GameObject handHighlightText;
    //course objs
    public GameObject[] puttMeterTextObjs;
}
