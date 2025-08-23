using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class howToPlay : MonoBehaviour
{
    public GameObject howToPlayPanel;

    public void HidePanel()
    {
        howToPlayPanel.SetActive(false);
    }

    public void ShowPanel()
    {
        howToPlayPanel.SetActive(true);
    }
}
