using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string interactablePrompt;
    public Transform interactableTransform;
    
    private bool isInteractable = false;
    private bool isWriting = false;

    public TextMeshPro text;
    public SpriteRenderer frame;

    public Action action; // Action to be performed when interacted with

    public UnityEvent interactCoroutine; // Coroutine to handle the interaction

    public virtual void Interact()
    {
        Debug.Log(interactablePrompt);
        interactCoroutine.Invoke();
    }

    public void RotateText()
    {
        // Rotate the text to face the camera
        text.transform.rotation = Camera.main.transform.rotation;
        // frame.transform.rotation = Camera.main.transform.rotation;
        // text.transform.position = transform.position + new Vector3(0, 0, -0.1f);
    }

    public void EnableInteract()
    {
        isInteractable = true;
        StartCoroutine(DisableInteract());
    }

    public IEnumerator DisableInteract()
    {
        if (!isInteractable) yield break; // exit coroutine if already disabled (to prevent multiple coroutines running at the same time
        
        // if coroutine is already running, stop it
        yield return new WaitForSeconds(3);
        isInteractable = false;
    }

    IEnumerator writeText(string content, float delay = 0.05f)
    {
        if (isWriting) yield break; // Exit the coroutine if it is already running

        isWriting = true; // Set the flag to indicate that the coroutine is running
        for (int i = 0; i <= content.Length; i++)
        {
            text.text = content.Substring(0, i);
            yield return new WaitForSeconds(delay);

            if (!isInteractable) // If the object is no longer interactable, stop writing
            {
                text.text = "";
                isWriting = false;
                yield break;
            }
        }
    }

    public void Update()
    {
        if (isInteractable) // Check if interactable and not currently writing
        {
            text.enabled = true;
            RotateText();
            StartCoroutine(writeText(interactablePrompt, 0.05f));

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
        else if (!isInteractable)
        {
            text.enabled = false;
            isWriting = false;
        }
    }


}
