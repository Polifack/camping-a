using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public bool lockCursor;
    public bool useController;
    public float mouseSensitivity = 10;
    public Transform target;
    public float distanceFromTarget = 2;
    public Vector2 pitchMinMax = new Vector2(-40, 85);
    public float rotationSmoothTime = .12f;

    private Vector3 m_smoothVel;
    private Vector3 m_currentRot;
    private float m_xRotation;
    private float m_yRotation;
    private float input_x;
    private float input_y;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[Warning]: no target found.");
        }

        // lock cursor

        if (lockCursor)
        {   
            Debug.Log("Cursor locked");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
    }

    private void LateUpdate()
    {
        // input with both mouse and controller
        if (useController)
        {
            input_x = Input.GetAxis("Axis3");
            input_y = Input.GetAxis("Axis6");
        }
        else
        {
            input_x = Input.GetAxis("Mouse X");
            input_y = Input.GetAxis("Mouse Y");
        }


        m_xRotation += input_x * mouseSensitivity; 
        m_yRotation -= input_y * mouseSensitivity;
        m_yRotation = Mathf.Clamp(m_yRotation, pitchMinMax.x, pitchMinMax.y); 

        m_currentRot = Vector3.SmoothDamp(m_currentRot, new Vector3(m_yRotation, m_xRotation), ref m_smoothVel, rotationSmoothTime);
        transform.eulerAngles = m_currentRot;
        transform.position = target.position - transform.forward * distanceFromTarget;
    }

}
