using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PokemonInteraction : MonoBehaviour, IInputClickHandler
{
    public void OnInputClicked(InputEventData eventData)
    {
        gameObject.GetComponent<AudioSource>().Play();
    }
}
