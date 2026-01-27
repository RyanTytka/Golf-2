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
    private GameObject currentDataObj = null;

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
                //highlight this piece
                GetComponent<SpriteRenderer>().color = new Color(.8f, .8f, .5f, 1);
                hovering = true;
                if (currentDataObj == null)
                {
                    //if we dont have a data obj already, create one
                    currentDataObj = Instantiate(distanceDataObj, GameObject.Find("FrontCanvas").transform);
                    currentDataObj.GetComponent<RectTransform>().position = new Vector3(gameObject.transform.position.x, 1, 90);
                    //set the animation for it
                    float bobHeight = 20f;
                    float bobDuration = 1f;
                    float rotationDuration = 2f;
                    // Bob up & down
                    currentDataObj.GetComponentsInChildren<RectTransform>()[0].DOAnchorPosY(
                        currentDataObj.GetComponentsInChildren<RectTransform>()[0].anchoredPosition.y + bobHeight,
                        bobDuration
                    )
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                    //spin around y axis
                    currentDataObj.GetComponentsInChildren<RectTransform>()[1].DORotate(
                        new Vector3(0f, 360f, 0f),
                        rotationDuration,
                        RotateMode.FastBeyond360
                        )
                        .SetEase(Ease.Linear)
                        .SetLoops(-1);
                }
                //set the pos of our current data obj
                currentDataObj.GetComponent<RectTransform>().position = new Vector3(gameObject.transform.position.x, 1, 90);
                //Set distance text
                int distFromBall = Mathf.Abs(GameObject.Find("CourseManager").GetComponent<Course>().DistanceToBall(myIndex)) * 10;
                currentDataObj.GetComponentsInChildren<TextMeshProUGUI>()[0].text = distFromBall.ToString() + " yds.";
                break;
            }
        }
        // If not hovering this frame, delete it
        if (!hovering)// && currentDataObj != null)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            Destroy(currentDataObj);
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
