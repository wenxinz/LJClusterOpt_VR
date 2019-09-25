using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlScreenControl : MonoBehaviour
{
    public bool isHide;
    [SerializeField] private GameObject ljcluster;

    private CanvasGroup screen;
    private CanvasGroup infoscreen;
    [SerializeField] private GameObject displayscreen;
    [SerializeField] private Toggle isDisplay;
    [SerializeField] private Slider ljclustersize;
    [SerializeField] private GameObject slidercurrent;
    [SerializeField] private Button restartbutton;
    [SerializeField] private Button closebutton;

    // Start is called before the first frame update
    void Start()
    {
        screen = GetComponent<CanvasGroup>();
        Hide();
        infoscreen = displayscreen.GetComponent<CanvasGroup>();
        isDisplay.onValueChanged.AddListener((value) => { hide_show_infoscreen(value); });
        ljclustersize.wholeNumbers = true;
        ljclustersize.onValueChanged.AddListener((value) => { change_clustersize((int)value); });
        restartbutton.onClick.AddListener(Restart);
        closebutton.onClick.AddListener(Hide);
    }

    public void Hide()
    {
        screen.alpha = 0f;
        screen.blocksRaycasts = false;
        screen.interactable = false;
        isHide = true;
        ljcluster.SetActive(true);
    }

    public void Show()
    {
        screen.alpha = 1f;
        screen.blocksRaycasts = true;
        screen.interactable = true;
        isHide = false;
        ljcluster.SetActive(false);
    }

    private void hide_show_infoscreen(bool value)
    {
        if (value)
        {
            infoscreen.alpha = 1f;
        }
        else
        {
            infoscreen.alpha = 0f;
        }
    }

    private void change_clustersize(int size)
    {
        ljcluster.GetComponent<LJCluster>().changeClusterSize(size);
        ljcluster.GetComponent<OptimizationControl>().stopOptimizeCluster();
        slidercurrent.GetComponent<TextMeshProUGUI>().text = size.ToString();
    }

    private void Restart()
    {
        ljcluster.GetComponent<LJCluster>().restartCluster();
        ljcluster.GetComponent<OptimizationControl>().stopOptimizeCluster();
    }
}
