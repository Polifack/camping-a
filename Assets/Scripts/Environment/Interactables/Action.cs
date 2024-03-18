using UnityEngine;

public abstract class Action : MonoBehaviour
{
    public PlayerStateMachine player;

    public abstract void BeforeCharacterUpdate();
    public abstract void UpdateRotation();
    public abstract void UpdateVelocity();
    public abstract void AfterCharacterUpdate();
}