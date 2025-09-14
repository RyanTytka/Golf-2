using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using System.Linq;

public class deckViewer : MonoBehaviour
{
    public GameObject backgroundPanel;

    private List<GameObject> displayedObjs = new();

    public GameObject cardContainer;

    //dragging card container
    private float scrollOffset = 0f;
    private float maxScroll = 0f;
    private float scrollSpeed = 5f;
    private bool isScrolling = false;
    private float previousMouseY;
    private float startDragY;
    private float startScrollOffset;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private float dragSensitivity = 2.5f;
    private PointerEventData pointerEventData;

    public void ViewBaseDeck()
    {
        Hand hand = GameObject.Find("GameManager").GetComponent<Hand>();
        OpenDeckView(hand.baseDeck);
    }

    public void ViewCurrentDeck()
    {
        Hand hand = GameObject.Find("GameManager").GetComponent<Hand>();
        OpenDeckView(hand.currentDeck);
    }

    public void RemoveDeckView()
    {
        Hand hand = GameObject.Find("GameManager").GetComponent<Hand>();
        OpenDeckView(hand.baseDeck, true);
    }

    //opens view of deck with only cards that are eligible for specified upgrade
    public void UpgradeDeckView(upgradeBuy upgrade)
    {
        Hand hand = GameObject.Find("GameManager").GetComponent<Hand>();
        List<GameObject> toDisplay = hand.baseDeck
            .Where(obj => obj.GetComponent<Draggable>().CanUpgrade(upgrade) != null)
            .ToList();
        OpenDeckView(toDisplay, false, true);
    }

    //create background and copies of cards in deck for player to look at
    private void OpenDeckView(List<GameObject> list, bool isRemoving = false, bool isUpgrading = false)
    {
        //paus game
        GameObject.Find("GameManager").GetComponent<Hand>().paused = true;
        //show black bg
        backgroundPanel.SetActive(true);
        backgroundPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        backgroundPanel.GetComponentInChildren<Button>().onClick.AddListener(CloseDeckView);
        backgroundPanel.transform.SetSiblingIndex(backgroundPanel.transform.parent.childCount - 1);
        //create card objs
        // World-space screen bounds
        float screenHeight = 2f * Camera.main.orthographicSize - 1.0f;
        float screenWidth = screenHeight * Camera.main.aspect - 1.0f * Camera.main.aspect;
        float cardWidth = 2.5f;
        float cardHeight = 3.5f;
        float minSpacing = 0.2f; 
        // Determine how many columns can fit
        int columns = Mathf.FloorToInt((screenWidth + minSpacing) / (cardWidth + minSpacing));
        columns = Mathf.Max(1, columns); // At least 1 column
        float totalSpacing = minSpacing * (columns - 1);
        float availableWidth = screenWidth - totalSpacing;
        float spacingX = (screenWidth - (cardWidth * columns)) / (columns - 1);
        float spacingY = cardHeight + minSpacing;
        // Starting position (top-center of screen)
        Vector3 startPos = new Vector3(
            -screenWidth / 2 + cardWidth / 2,
            screenHeight / 2 - cardHeight / 2,
            0f
        );
        for (int i = 0; i < list.Count; i++)
        {
            GameObject newCard = Instantiate(list[i], cardContainer.transform);
            newCard.SetActive(true);
            newCard.GetComponent<Draggable>().UpdateCard();
            newCard.GetComponent<Draggable>().isDeckView = true;
            newCard.GetComponent<Draggable>().isRemoveView = isRemoving;
            newCard.GetComponent<Draggable>().isUpgradeView = isUpgrading;
            newCard.GetComponent<Draggable>().SetSortOrder(10);
            newCard.GetComponent<Draggable>().cardId = i;
            list[i].GetComponent<Draggable>().cardId = i;
            displayedObjs.Add(newCard);
            int row = i / columns;
            int col = i % columns;
            float x = startPos.x + col * (cardWidth + spacingX);
            float y = startPos.y - row * spacingY;
            newCard.transform.position = new Vector3(x, y, 0);
        }
        int totalRows = Mathf.CeilToInt((float)list.Count / columns);
        float contentHeight = totalRows * (cardHeight + minSpacing);
        float visibleHeight = screenHeight;

        maxScroll = Mathf.Max(0, contentHeight - visibleHeight);// / canvas.transform.localScale.x;
        scrollOffset = 0f;
        cardContainer.transform.localPosition = Vector3.zero;
    }
    void Update()
    {
        if (!backgroundPanel.activeSelf) return;
        if (GameObject.Find("ShopManager") != null && 
            GameObject.Find("ShopManager").GetComponent<shopManager>().isPreviewing) return;

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == backgroundPanel)
                {
                    isScrolling = true;
                    previousMouseY = Input.mousePosition.y;
                    break;
                }
            }
        }

        if (isScrolling && Input.GetMouseButton(0))
        {
            float currentMouseY = Input.mousePosition.y;
            float deltaY = (currentMouseY - previousMouseY) * dragSensitivity;
            previousMouseY = currentMouseY;

            scrollOffset = Mathf.Clamp(scrollOffset + deltaY, 0f, maxScroll);

            Vector3 pos = cardContainer.transform.localPosition;
            pos.y = scrollOffset;
            cardContainer.transform.localPosition = pos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isScrolling = false;
        }
    }

    public void CloseDeckView()
    {
        GameObject.Find("GameManager").GetComponent<Hand>().paused = false;
        foreach (GameObject go in displayedObjs)
        {
            Destroy(go);
        }
        scrollOffset = 0f;
        cardContainer.transform.localPosition = Vector3.zero;
        displayedObjs.Clear();
        backgroundPanel.SetActive(false);
    }
}
