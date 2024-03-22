using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public TextMeshProUGUI charName;
    public TextMeshProUGUI dialogText;

    private bool _isWriting = false;


    IEnumerator writeText(string content, float delay = 0.05f)
    {
        if (_isWriting) yield break; // Exit the coroutine if it is already running

        _isWriting = true; // Set the flag to indicate that the coroutine is running
        for (int i = 0; i <= content.Length; i++)
        {
            dialogText.text = content.Substring(0, i);
            yield return new WaitForSeconds(delay);
        }
    }

    public void show(string name, string text)
    {
        charName.text = name;
        StartCoroutine(writeText(text));
    }


}
