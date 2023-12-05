using UnityEngine;

public class FreeCameraMovement : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 2f;

    void Update()
    {
        // Camera Movement
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalMovement, 0f, verticalMovement).normalized;
        transform.Translate(moveDirection * movementSpeed * Time.deltaTime);

        // Camera Rotation
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 rotation = new Vector3(-mouseY, mouseX, 0f) * rotationSpeed;
        transform.Rotate(rotation);

        // Clamp vertical rotation to avoid flipping
        float currentXRotation = transform.eulerAngles.x;
        if (currentXRotation > 180f)
        {
            currentXRotation -= 360f;
        }

        float clampedXRotation = Mathf.Clamp(currentXRotation, -80f, 80f);
        transform.rotation = Quaternion.Euler(clampedXRotation, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
