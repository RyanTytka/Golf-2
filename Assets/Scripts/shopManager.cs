using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class shopManager : MonoBehaviour
{
    public GameObject previewCanvas;
    public bool isPreviewing = false;
    public GameObject yesButton;
    public GameObject noButton;
    public deckViewer deckViewer;
    public GameObject teeRemoveCostObj;
    public GameObject removeButton;
    public TextMeshProUGUI tees;
    public TextMeshProUGUI previewText;
    public List<GameObject> upgrades = new();

    public GameObject upgradePreview, replaceUpgradePreview;
    public GameObject upgradePrefab;
    public List<GameObject> shopOptions;
    //public GameObject previewBg; //black overlay in preview canvas

    private GameObject currentUpgrade;
    private int removeCost = 2; //doubles each time you remove a card (resets each course)
    private GameObject previewCard;
    private Vector3 previewCardPos, previewCardScale;

    public GameObject courseImagesObj;
    public Sprite[] courseImages;
    public GameObject courseSelectButton;
    public GameObject nextCourseImage;

    public GameObject nextRivalImage;
    public GameObject nextRivalButton;
    public GameObject nextRivalName;
    public GameObject nextRivalDesc;

    private void Start()
    {
        //generate shop items
        List<GameObject> availableCards = GameObject.Find("GameManager").GetComponent<Hand>().upgradeCards.OrderBy(x => Random.value).Take(3).ToList();
        GameObject costObj = GameObject.Find("TeesCost");
        int index = 0;
        //new cards
        foreach (GameObject card in availableCards)
        {
            //create card objs
            GameObject newCard = Instantiate(card);
            shopOptions.Add(newCard);
            newCard.transform.position = new Vector3(index * 3 - 5, -1.5f, 0);
            newCard.GetComponent<Draggable>().isShopOption = true;
            newCard.GetComponent<Draggable>().UpdateCard();
            newCard.transform.parent = gameObject.transform;
            //create cost objs
            GameObject newCost = Instantiate(costObj, GameObject.Find("MainCanvas").transform);
            newCost.GetComponent<RectTransform>().position = new Vector3(index * 3 - 5.25f, 0.75f, 0);
            newCost.GetComponentInChildren<TextMeshProUGUI>().text = newCard.GetComponent<Draggable>().myCost.ToString();
            newCard.GetComponent<Draggable>().costObj = newCost;
            index++;
        }
        //upgrades
        for (int i = 0; i < 3; i++)
        {
            //create upgrade options
            GameObject newUpgrade = Instantiate(upgradePrefab, GameObject.Find("MainCanvas").transform);
            newUpgrade.GetComponent<RectTransform>().position = new Vector3(i * 3 - 5.0f, 2.5f, 0);
            newUpgrade.GetComponent<upgradeBuy>().type = (upgradeBuy.upgradeType)Random.Range(0, 3);
            newUpgrade.GetComponent<upgradeBuy>().ID = Random.Range(0, 4);
            newUpgrade.GetComponent<upgradeBuy>().UpdateView();
            newUpgrade.GetComponent<Button>().onClick.AddListener(() => ClickUpgrade(newUpgrade));
            upgrades.Add(newUpgrade);
        }
        Destroy(costObj);
        GameObject.Find("TeesCount").GetComponent<TextMeshProUGUI>().text =
            GameObject.Find("CourseManager").GetComponent<Course>().tees.ToString();
        GameObject.Find("ContinueButton").GetComponent<Button>().onClick.AddListener(ToNewHole);
        UpdateUI();
        //reset rival
        GameObject.Find("CourseManager").GetComponent<Course>().currentRival = -1;
    }

    public void ClickUpgrade(GameObject upgrade)
    {
        //open deck view to select a card to upgrade
        deckViewer.UpgradeDeckView(upgrade.GetComponent<upgradeBuy>());
        currentUpgrade = upgrade;
    }

    public void AddUpgrade(int cardId)
    {
        //add upgrade
        isPreviewing = false;
        upgradePreview.SetActive(false);
        previewCanvas.SetActive(false);
        Destroy(previewCard);
        previewCard = null;
        GameObject card = GameObject.Find("GameManager").GetComponent<Hand>().GetCardById(cardId);
        upgradeBuy u = card.GetComponent<Draggable>().CanUpgrade(currentUpgrade.GetComponent<upgradeBuy>());
        if (u != currentUpgrade.GetComponent<upgradeBuy>())
        {
            //remove old upgrade if they are the same type
            Destroy(u.gameObject);
        }
        card.GetComponent<upgrades>().AddUpgrade(currentUpgrade);
        deckViewer.CloseDeckView();
        GameObject.Find("CourseManager").GetComponent<Course>().tees -= 2;
        currentUpgrade = null;
        UpdateUI();
    }

    public void ToNewHole()
    {
        //Destroy Shop Items
        foreach (GameObject go in shopOptions)
        {
            Destroy(go);
        }
        shopOptions.Clear();
        //Load course scene
        GameObject.Find("CourseManager").GetComponent<Course>().comingFromShop = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Course");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only run logic if it's the "Course" scene
        if (scene.name == "Course")
        {
            if (GameObject.Find("CourseManager").GetComponent<Course>().comingFromShop)
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
    }

    public void UpdateUI()
    {
        int teeAmount = GameObject.Find("CourseManager").GetComponent<Course>().tees;
        //card removal
        teeRemoveCostObj.GetComponentInChildren<TextMeshProUGUI>().text = removeCost.ToString();
        if (removeCost > teeAmount)
            removeButton.GetComponent<Button>().interactable = false;
        //player tees
        tees.text = teeAmount.ToString();
        //course selection
        if (teeAmount < 2)
            courseSelectButton.GetComponent<Button>().interactable = false;
        //upgrades
        foreach(GameObject upgrade in upgrades)
        {
            if(teeAmount < 3)
            {
                upgrade.GetComponent<Button>().interactable = false;
            }
        }
        //card options
        foreach(GameObject card in shopOptions)
        {
            if(card.GetComponent<Draggable>().myCost > teeAmount)
            {
                card.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
        //next rival
        if (teeAmount < 2)
            nextRivalButton.GetComponent<Button>().interactable = false;
    }

    //show the preview canvas and create a copy of the previewed card
    public void OpenPreview(GameObject card, bool remove = false, bool upgrade = false)
    {
        //StartCoroutine(FadeToAlpha(0.9f, 0.5f));
        isPreviewing = true;
        previewCanvas.SetActive(true);
        previewCard = card;
        previewCardPos = card.transform.localPosition;
        previewCardScale = card.transform.localScale;
        previewCard.GetComponent<Draggable>().SetSortOrder(25);
        yesButton.GetComponent<Button>().onClick.RemoveAllListeners();
        yesButton.SetActive(true);
        noButton.SetActive(true);
        courseImagesObj.SetActive(false);
        replaceUpgradePreview.SetActive(false);
        if (remove)
        {
            upgradePreview.SetActive(false);
            previewText.text = "Remove this card?";
            yesButton.GetComponent<Button>().onClick.AddListener(() => RemoveCard(previewCard.GetComponent<Draggable>().cardId));
        }
        if(upgrade)
        {
            upgradePreview.SetActive(true);
            previewText.text = "Add upgrade?";
            //if you are replacing an existing upgrade
            upgradeBuy u = previewCard.GetComponent<Draggable>().CanUpgrade(currentUpgrade.GetComponent<upgradeBuy>());
            if (u != currentUpgrade.GetComponent<upgradeBuy>())
            {
                previewText.text = "Replace upgrade?";
                replaceUpgradePreview.GetComponent<upgradeBuy>().UpdateView(u.gameObject);
                replaceUpgradePreview.SetActive(true);
            }

            //update preview update
            upgradePreview.GetComponent<upgradeBuy>().UpdateView(currentUpgrade);
            yesButton.GetComponent<Button>().onClick.AddListener(() => AddUpgrade(previewCard.GetComponent<Draggable>().cardId));
        }
    }

    public void CancelPreview()
    {
        isPreviewing = false;
        previewCanvas.SetActive(false);
        previewCard.GetComponent<Draggable>().SetSortOrder(10);
        previewCard.GetComponent<Draggable>().StartCoroutine(previewCard.GetComponent<Draggable>().
            AnimateToPoint(previewCardPos, previewCardScale, true, false));
        previewCard = null;
    }

    public void RemoveCard(int id)
    {
        Hand hand = GameObject.Find("GameManager").GetComponent<Hand>();
        hand.RemoveCardById(id);
        isPreviewing = false;
        previewCanvas.SetActive(false);
        Destroy(previewCard);
        previewCard = null;
        deckViewer.CloseDeckView();
        GameObject.Find("CourseManager").GetComponent<Course>().tees -= removeCost;
        removeCost *= 2;
        UpdateUI();
        //previewCard.GetComponent<Draggable>().SetSortOrder(10);
        //previewCard.GetComponent<Draggable>().StartCoroutine(previewCard.GetComponent<Draggable>().
        //    AnimateToPoint(previewCardPos, previewCardScale, true, false));
    }

    //private IEnumerator FadeToAlpha(float targetAlpha, float duration, System.Action onComplete = null)
    //{

    //    Color startColor = previewBg.GetComponent<Image>().color;
    //    float startAlpha = startColor.a;
    //    float time = 0f;

    //    while (time < duration)
    //    {
    //        float t = time / duration;
    //        float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
    //        previewBg.GetComponent<Image>().color = new Color(startColor.r, startColor.g, startColor.b, alpha);
    //        time += Time.deltaTime;
    //        yield return null;
    //    }

    //    previewBg.GetComponent<Image>().color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    //    onComplete?.Invoke();
    //}

    public void ChooseNextCourse()
    {
        //show preview canvas with 3 course options to choose from
        GameObject.Find("GameManager").GetComponent<Hand>().paused = true;
        yesButton.SetActive(false);
        noButton.SetActive(false);
        previewCanvas.SetActive(true);
        upgradePreview.SetActive(false);
        courseImagesObj.SetActive(true);
        previewText.text = "Select the next course:";
        List<int> courseOptions = new List<int> { 0, 1, 2, 3, 4 };
        for (int i = 0; i < 3; i++)
        {
            //randomly select 3 different courses
            int course = courseOptions[Random.Range(0, courseOptions.Count)];
            courseOptions.Remove(course);
            courseImagesObj.GetComponentsInChildren<Image>()[i].sprite = courseImages[course];
            courseImagesObj.GetComponentsInChildren<Button>()[i].onClick.RemoveAllListeners();
            courseImagesObj.GetComponentsInChildren<Button>()[i].onClick.AddListener(() =>
            {
                GameObject.Find("CourseManager").GetComponent<Course>().nextCourse = course;
                nextCourseImage.GetComponent<Image>().sprite = courseImages[course];
                previewCanvas.SetActive(false);
                GameObject.Find("CourseManager").GetComponent<Course>().tees -= 2;
                courseSelectButton.GetComponent<Button>().interactable = false;
                GameObject.Find("GameManager").GetComponent<Hand>().paused = false;
                UpdateUI();
            });
        }
    }

    public void SeeNextRival()
    {
        Course c = GameObject.Find("CourseManager").GetComponent<Course>();
        c.currentRival = c.availRivals[Random.Range(0, c.availRivals.Count)];
        c.availRivals.Remove(c.currentRival);
        nextRivalImage.GetComponent<Image>().sprite = c.rivalImages[c.currentRival];
        nextRivalName.GetComponent<TextMeshProUGUI>().text = c.rivalNames[c.currentRival];
        nextRivalDesc.GetComponent<TextMeshProUGUI>().text = c.rivalDescriptions[c.currentRival];
        nextRivalName.SetActive(true);
        nextRivalDesc.SetActive(true);
        nextRivalButton.GetComponent<Button>().interactable = false;
        c.tees -= 2;
        UpdateUI();
    }
}
