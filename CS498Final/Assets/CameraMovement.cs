using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public float mouseSensitivity = 1f;

    float xRotation = 0f;

    public Camera FPSCam;

    // Start is called before the first frame update
    void Start()
    {
        // hide/lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        FPSCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, FPSCam.transform.localEulerAngles.z);

        transform.Rotate(Vector3.up * mouseX);
    }
}
