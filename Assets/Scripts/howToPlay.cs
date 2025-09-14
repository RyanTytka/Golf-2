using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        howToPlayPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        howToPlayPanel.GetComponentInChildren<Button>().onClick.AddListener(HidePanel);
    }
}
