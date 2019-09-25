using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DaydreamElements.ObjectManipulation;

public class MenuInteraction : BaseInteractiveObject, IPointerDownHandler
{

    [SerializeField] private ControlScreenControl controlscreen;

    protected override void OnSelect()
    {
        if (controlscreen.isHide)
        {
            controlscreen.Show();
        }
    }
}
