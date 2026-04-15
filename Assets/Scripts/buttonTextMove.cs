using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class buttonTextMove : MonoBehaviour
{
    public void MoveText(float amount)
    {
        //make sure button is enabled
        if (GetComponent<Button>().interactable == false) return;
        //move text
        gameObject.transform.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(0, amount, 0);
        // deselect this button after clicked
        if(amount == 0)
            EventSystem.current.SetSelectedGameObject(null);
        //play sound effect
        if(amount == 0)
        {
            //release
            GameObject.Find("Music Manager").GetComponent<musicmanager>().PlaySounEffect(0);
        }
        else
        {
            //down

        }
    }
}
