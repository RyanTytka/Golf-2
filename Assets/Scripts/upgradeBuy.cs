using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
