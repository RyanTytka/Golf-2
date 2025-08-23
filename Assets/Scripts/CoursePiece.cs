using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoursePiece : MonoBehaviour
{
    public string pieceName;
    public string pieceDesc;
    public int myIndex;
    public int myType;
    public GameObject distanceDataObj;
    private GameObject currentDataObj = null;

    void Update()
    {
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
                if (currentDataObj == null)
                {
                    currentDataObj = Instantiate(distanceDataObj, GameObject.Find("MainCanvas").transform);
                }
                // Convert world position to canvas-local position
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    GameObject.Find("MainCanvas").GetComponent<RectTransform>(),
                    screenPoint,
                    GameObject.Find("MainCanvas").GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                    out Vector2 anchoredPos
                );
                currentDataObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;

                //Set distance text
                int distToPin = Mathf.Abs(GameObject.Find("CourseManager").GetComponent<Course>().DistanceToHole(myIndex)) * 10;
                int distFromBall = Mathf.Abs(GameObject.Find("CourseManager").GetComponent<Course>().DistanceToBall(myIndex)) * 10;
                currentDataObj.GetComponentsInChildren<TextMeshProUGUI>()[0].text = distFromBall.ToString() + " yds.";
                currentDataObj.GetComponentsInChildren<TextMeshProUGUI>()[1].text = distToPin.ToString() + " yds.";
                break;
            }
        }
        // If not hovering this frame, delete it
        if (!hovering && currentDataObj != null)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            Destroy(currentDataObj);
        }
    }
}
