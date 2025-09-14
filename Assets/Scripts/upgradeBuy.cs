using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class upgradeBuy : MonoBehaviour
{
    public enum upgradeType
    {
        GRIP,
        SHAFT,
        CLUBHEAD,
    }

    public upgradeType type;
    public int ID;

    public GameObject nameObj, imageObj, descObj;
    public string[] names, descriptions;
    public Sprite[] icons;
    public int[] carryStats, rollStats;
    public GameObject costObj;

    //leave blank if using its own upgrade stats, or pass in an upgrade to display its stats
    public void UpdateView(GameObject upgrade = null)
    {
        int id = (int)type * 4 + ID;
        costObj.SetActive(upgrade == null);
        if (upgrade != null)
        {
            id = (int)upgrade.GetComponent<upgradeBuy>().type * 4 + upgrade.GetComponent<upgradeBuy>().ID;
        }
        nameObj.GetComponent<TextMeshProUGUI>().text = names[id];
        imageObj.GetComponent<Image>().sprite = icons[id];
        descObj.GetComponent<TextMeshProUGUI>().text = descriptions[id].Replace("\\n", "\n");
    }
}
