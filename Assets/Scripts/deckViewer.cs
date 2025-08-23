using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class deckViewer : MonoBehaviour
{
    private List<GameObject> displayedObjs = new();

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

    //create background and copies of cards in deck for player to look at
    private void OpenDeckView(List<GameObject> list)
    {
        //paus game
        //GameObject.Find("CourseManager").paused = true;
        //show black bg

        //create card objs
        // World-space screen bounds
        float screenHeight = 2f * Camera.main.orthographicSize;
        float screenWidth = screenHeight * Camera.main.aspect;
        float minCardWidth = 1.2f;
        float minSpacing = 0.2f; 
        float cardAspect = 1.4f; 
        // Determine how many columns can fit
        int columns = Mathf.FloorToInt((screenWidth + minSpacing) / (minCardWidth + minSpacing));
        columns = Mathf.Max(1, columns); // At least 1 column
        // Calculate final card size to fill screen
        float totalSpacing = minSpacing * (columns - 1);
        float availableWidth = screenWidth - totalSpacing;
        float cardWidth = Mathf.Min(minCardWidth, availableWidth / columns);
        float spacingX = (screenWidth - (cardWidth * columns)) / (columns - 1);
        float cardHeight = cardWidth * cardAspect;
        float spacingY = cardHeight + minSpacing;
        // Starting position (top-center of screen)
        Vector3 startPos = new Vector3(
            -screenWidth / 2 + cardWidth / 2,
            screenHeight / 2 - cardHeight / 2,
            0f
        );
        for (int i = 0; i < list.Count; i++)
        {
            GameObject prefab = list[i];
            GameObject newCard = Instantiate(prefab);
            newCard.SetActive(true);
            displayedObjs.Add(newCard);
            int row = i / columns;
            int col = i % columns;
            float x = startPos.x + col * (cardWidth + spacingX);
            float y = startPos.y - row * spacingY;
            newCard.transform.position = new Vector3(x, y, 0);
            newCard.transform.localScale = new Vector3(cardWidth, cardHeight, 1f); // adjust scale if needed
        }
    }

    public void CloseDeckView()
    {
        foreach(GameObject go in displayedObjs)
        {
            Destroy(go);
        }
        displayedObjs.Clear();
    }
}
