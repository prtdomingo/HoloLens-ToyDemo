using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PokeballPicker : MonoBehaviour, IFocusable, IInputClickHandler
{
    void Start ()
    {
        Debug.Log("Start");
	}

    public void OnInputClicked(InputEventData eventData)
    {
        Debug.Log("WHY");
    }

    public void OnFocusEnter()
    {
        Debug.Log("Enter");
    }

    public void OnFocusExit()
    {
        Debug.Log("Exit");
    }
}
