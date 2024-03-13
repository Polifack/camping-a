using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TalkAction : MonoBehaviour
{
    public void execute()
    {
        GameManager.instance.toggleTalkUI();
        GameManager.instance.printDialog("Tony", "carmela sexo");
    }
}
