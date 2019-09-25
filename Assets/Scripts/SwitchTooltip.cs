using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTooltip : MonoBehaviour
{
    [SerializeField] private GameObject mysystem;
    private LJCluster cluster;
    private GameObject tooltip1;
    private GameObject tooltip2;

    // Start is called before the first frame update
    void Start()
    {
        cluster = mysystem.GetComponent<LJCluster>();
        tooltip1 = this.gameObject.transform.GetChild(0).gameObject;
        tooltip2 = this.gameObject.transform.GetChild(1).gameObject;
        showtooltip1();
    }

    // Update is called once per frame
    void Update()
    {
        if (cluster.isRotable)
        {
            showtooltip1();
        }
        else
        {
            showtooltip2();
        }
    }

    private void showtooltip1()
    {
        tooltip1.SetActive(true);
        tooltip2.SetActive(false);
    }

    private void showtooltip2()
    {
        tooltip1.SetActive(false);
        tooltip2.SetActive(true);
    }
}
