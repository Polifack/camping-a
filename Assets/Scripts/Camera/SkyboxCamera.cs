using UnityEngine;

public class SkyboxCamera : MonoBehaviour
{
    [SerializeField] public Transform playerCamera;
    [SerializeField] public float skyboxScale;


    private void Update()
    {
        transform.rotation = playerCamera.rotation;
        //transform.localPosition = playerCamera.position / skyboxScale;
    }

}