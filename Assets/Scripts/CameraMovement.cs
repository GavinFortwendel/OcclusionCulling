using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public float Xsens;
    public float Ysens;

    public Transform orientation;

    float xRot;
    float yRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float xMouse = Input.GetAxisRaw("Mouse X") * Time.deltaTime * Xsens;
        float yMouse = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * Ysens;

        xRot -= yMouse;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        yRot += xMouse;

        transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        orientation.rotation = Quaternion.Euler(0, yRot, 0);

    }


}
