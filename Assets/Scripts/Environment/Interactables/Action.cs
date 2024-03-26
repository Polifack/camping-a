using UnityEngine;

public abstract class Action : MonoBehaviour
{
    public PlayerStateMachine player;
    //public GameManager gameManager;

    public void Start()
    {
        GameObject playerObj = GameManager.instance.player;
        player = playerObj.GetComponent<PlayerStateMachine>();
    }

    public abstract void OnStateEnter();
    public abstract void SetInputs(PlayerCharacterInputs inputs);
    public abstract void BeforeCharacterUpdate();
    public abstract void UpdateRotation();
    public abstract void UpdateVelocity();
    public abstract void AfterCharacterUpdate();
    public abstract void OnStateExit();
}