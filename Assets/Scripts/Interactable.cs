using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactableName;
    public bool isInteractable = true;

    public virtual void Interact()
    {
        Debug.Log("Interacting with " + interactableName);
    }
}
