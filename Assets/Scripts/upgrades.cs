using UnityEngine;
using UnityEngine.UI;

public class upgrades : MonoBehaviour
{
    public GameObject gripIcon, shaftIcon, clubheadIcon;
    private int carry, roll;

    //return [carry, roll] based off of current upgrades
    public int[] Stats()
    {
        int[] stats = { carry, roll };
        return stats;
    }

    //update the icons on the card and get stats
    public void UpdateView()
    {
        carry = 0;
        roll = 0;
        foreach (upgradeBuy upgrade in GetComponentsInChildren<upgradeBuy>())
        {
            int id = (int)upgrade.type * 4 + upgrade.ID;
            carry += upgrade.carryStats[id];
            roll += upgrade.rollStats[id];
            if (upgrade.type == upgradeBuy.upgradeType.GRIP)
            {
                gripIcon.GetComponent<Image>().sprite = upgrade.icons[id];
                gripIcon.SetActive(true);
            }
            if (upgrade.type == upgradeBuy.upgradeType.SHAFT)
            {
                shaftIcon.GetComponent<Image>().sprite = upgrade.icons[(int)upgrade.type * 4 + upgrade.ID];
                shaftIcon.SetActive(true);
            }
            if (upgrade.type == upgradeBuy.upgradeType.CLUBHEAD)
            {
                clubheadIcon.GetComponent<Image>().sprite = upgrade.icons[(int)upgrade.type * 4 + upgrade.ID];
                clubheadIcon.SetActive(true);
            }
        }
    }

    public void AddUpgrade(GameObject upgrade)
    {
        upgrade.transform.parent = transform;
    }
}
