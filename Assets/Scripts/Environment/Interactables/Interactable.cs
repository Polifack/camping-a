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
    private bool isInteracting = false;
    
    public Action action; // Action to be performed when interacted with


    public void EnableInteract()
    {
        if (isInteracting){
            currentFadeTime = 0f;
            return;
        } 

        GameManager.instance.enableInteractUI();
        GameManager.instance.printInteractPrompt(interactableName, interactablePrompt);
        currentFadeTime = 0f;
        isInteracting = true;
    }
    public void DisableInteract()
    {
        isInteracting = false;
        GameManager.instance.disableInteractUI();
    }

    public void FixedUpdate(){
        if (isInteracting){
            currentFadeTime += Time.deltaTime;
            if (currentFadeTime >= interactableFadeTime){
                DisableInteract();
                currentFadeTime = 0f;
            }
        }
    }

}
