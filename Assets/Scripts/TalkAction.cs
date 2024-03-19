using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TalkAction : Action
{
    public override void OnStateEnter()
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

    public override void OnStateExit()
    {
    }
}
