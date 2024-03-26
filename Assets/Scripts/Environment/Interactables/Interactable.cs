using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class Interactable : MonoBehaviour
{
    public string interactableName;
    public string interactablePrompt;

    private float interactableFadeTime = .5f;
    private float currentFadeTime = 0f;
    
    public Action action; // Action to be performed when interacted with


    public void EnableInteract()
    {
        GameManager.instance.enableInteractUI();
        GameManager.instance.printInteractPrompt(interactableName, interactablePrompt);
        currentFadeTime = 0f;
    }
    public void DisableInteract()
    {
        GameManager.instance.disableInteractUI();
    }

    public void Update(){
        if (GameManager.instance.interactUI.activeSelf){
            currentFadeTime += Time.deltaTime;
            if (currentFadeTime >= interactableFadeTime){
                DisableInteract();
                currentFadeTime = 0f;
            }
        }
    }

}
