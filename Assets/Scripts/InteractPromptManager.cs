using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractPromptManager : MonoBehaviour
{
    public TextMeshProUGUI interactableName;
    public TextMeshProUGUI interactableAction;

    public void show(string name, string text)
    {
        // this should be shown by raycasting the interactable object towards the canvas
        interactableName.text = name;
        interactableAction.text = text;
    }


}
