using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CoursePiece : MonoBehaviour
{
    public string pieceName;
    public string pieceDesc;
    public int myIndex;
    public int myType;
    public GameObject distanceDataObj;
    public GameObject rollOverPrefab; //obj created when ball rolls over this piece
    public Sprite rollOverSprite; //sprite to set for rollOverPrefab
    //private GameObject currentDataObj = null;

    void Update()
    {
        if (GameObject.Find("CourseManager").GetComponent<Course>().paused) return;
        //If hovering this piece, display distance data
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool hovering = false;

        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.transform == transform)
            {
                GetComponent<SpriteRenderer>().color = new Color(.8f, .8f, .5f, 1);
                hovering = true;
                //if (currentDataObj == null)
                //{
                //    currentDataObj = Instantiate(distanceDataObj, GameObject.Find("MainCanvas").transform);
                //}
                //// Convert world position to canvas-local position
                //Vector2 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
                //RectTransformUtility.ScreenPointToLocalPointInRectangle(
                //    GameObject.Find("MainCanvas").GetComponent<RectTransform>(),
                //    screenPoint,
                //    GameObject.Find("MainCanvas").GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                //    out Vector2 anchoredPos
                //);
                //currentDataObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
                //currentDataObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 90);

                ////Set distance text
                //int distToPin = Mathf.Abs(GameObject.Find("CourseManager").GetComponent<Course>().DistanceToHole(myIndex)) * 10;
                //int distFromBall = Mathf.Abs(GameObject.Find("CourseManager").GetComponent<Course>().DistanceToBall(myIndex)) * 10;
                //currentDataObj.GetComponentsInChildren<TextMeshProUGUI>()[0].text = distFromBall.ToString() + " yds.";
                //currentDataObj.GetComponentsInChildren<TextMeshProUGUI>()[1].text = distToPin.ToString() + " yds.";
                break;
            }
        }
        // If not hovering this frame, delete it
        if (!hovering)// && currentDataObj != null)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            //Destroy(currentDataObj);
        }
    }

    //create pop up effect that triggers when landing on this piece
    public void RolledOver()
    {
        GameObject obj = Instantiate(rollOverPrefab, transform);
        obj.transform.localPosition = new Vector3(0, 1, 0);
        obj.GetComponent<SpriteRenderer>().sprite = rollOverSprite;
        obj.transform.DOLocalMoveY(transform.position.y + 1.25f, 2f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                Destroy(obj);
            });
    }
}
