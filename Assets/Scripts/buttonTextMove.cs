using UnityEngine;
using UnityEngine.EventSystems;

public class buttonTextMove : MonoBehaviour
{
    public void MoveText(float amount)
    {
        gameObject.transform.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(0, amount, 0);
        // deselect this button after clicked
        if(amount == 0)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
