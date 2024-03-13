using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public TextMeshProUGUI charName;
    public TextMeshProUGUI dialogText;

    public void show(string name, string text)
    {
        charName.text = name;
        dialogText.text = text;
    }


}
