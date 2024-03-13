using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeAction : MonoBehaviour
{
    public AudioClip fridgeSound;

    Vector3 position = new Vector3(-0.9f, 0.1f, -44.3f);
    Vector3 rotation = new Vector3(0, 90, 0);
    public void execute()
    {
        GameObject p = GameManager.instance.getPlayer();
        StartCoroutine(p.GetComponent<PlayerStateMachine>().PerformAction("sitting", position, rotation, fridgeSound));
    }
}
