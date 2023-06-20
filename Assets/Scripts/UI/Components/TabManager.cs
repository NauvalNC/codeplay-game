using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TabCategory
{
    public Button tabButton;
    public GameObject tabWindow;
}

public class TabManager : MonoBehaviour
{
    private List<TabCategory> tabs;
    [SerializeField] private Transform tabBarButtons;
    [SerializeField] private Transform tabWindows;

    private void Awake()
    {
        int tIndex = 0;
        tabs = new List<TabCategory>();

        // Assign tabs
        foreach (Button button in tabBarButtons.GetComponentsInChildren<Button>())
        {
            tabs.Add(new TabCategory()
            {
                tabButton = button, 
                tabWindow = tabWindows.GetChild(tIndex).gameObject
            });

            int tTabId = tIndex++;
            tabs[tTabId].tabButton.onClick.AddListener(() => { SwitchTab(tTabId); });
        }

        SwitchTab(0);
    }

    private void SwitchTab(int targetIndex)
    {
        foreach(TabCategory tab in tabs)
        {
            tab.tabButton.GetComponent<Image>().enabled = false;
            tab.tabWindow.SetActive(false);
        }

        tabs[targetIndex].tabButton.GetComponent<Image>().enabled = true;
        tabs[targetIndex].tabWindow.SetActive(true);
    }
}
