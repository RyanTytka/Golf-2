using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class caddieDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject nameObj;
    public GameObject descriptionObj;
    public GameObject caddieRef;

    public void OnPointerEnter(PointerEventData eventData)
    {
        //update and show caddie name and description
        nameObj.GetComponent<TextMeshProUGUI>().text = caddieRef.GetComponent<Draggable>().cardName;
        descriptionObj.GetComponent<TextMeshProUGUI>().text = caddieRef.GetComponent<Draggable>().description;
        descriptionObj.SetActive(true);
        nameObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //hide name and description
        descriptionObj.SetActive(false);
        nameObj.SetActive(false);
    }
}
