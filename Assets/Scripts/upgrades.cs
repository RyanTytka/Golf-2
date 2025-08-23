using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class upgrades : MonoBehaviour
{
    public GameObject gripIcon, shaftIcon, clubheadIcon;
    public Sprite[] gripImages, shaftImages, clubheadImages;
    public int gripID, shaftID, clubheadID;

    //return [carry, roll] based off of current upgrades
    public int[] stats()
    {
        int[] stats = { 0, 0 };
        return stats;
    }

    //update the icons on the card
    public void updateView()
    {
        if(gripID != 0)
            gripIcon.GetComponent<Image>().sprite = gripImages[gripID];
        gripIcon.SetActive(gripID != 0);
        if (shaftID != 0)
            shaftIcon.GetComponent<Image>().sprite = shaftImages[shaftID];
        shaftIcon.SetActive(shaftID != 0);
        if (clubheadID != 0)
            clubheadIcon.GetComponent<Image>().sprite = clubheadImages[clubheadID];
        clubheadIcon.SetActive(clubheadID != 0);
    }
}
