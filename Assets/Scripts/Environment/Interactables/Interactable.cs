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


    
    public Action action; // Action to be performed when interacted with
    public UnityEvent interactCoroutine; // Coroutine to handle the interaction

    public virtual void Interact()
    {
        Debug.Log(interactablePrompt);
        interactCoroutine.Invoke();
    }

    public void EnableInteract()
    {
        GameManager.instance.enableInteractUI();
        GameManager.instance.printInteractPrompt(interactableName, interactablePrompt);
    }
    public void DisableInteract()
    {
        GameManager.instance.disableInteractUI();
    }

}
