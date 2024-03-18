using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TalkAction : Action
{

    public void execute()
    {
        GameManager.instance.toggleTalkUI();
        GameManager.instance.printDialog("Tony", "carmela sexo");
    }


    public override void BeforeCharacterUpdate()
    {
    }

    public override void UpdateRotation()
    {
    }

    public override void UpdateVelocity()
    {
    }

    public override void AfterCharacterUpdate()
    {
    }
}
