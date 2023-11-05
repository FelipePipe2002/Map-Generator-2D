using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 50.0f; // Adjust the camera movement speed
    public float zoomSpeed = 50.0f; // Adjust the camera zoom speed

    void Update()
    {
        // Camera Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0.0f);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Camera Zoom Out
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        this.GetComponent<Camera>().orthographicSize = Mathf.Clamp(this.GetComponent<Camera>().orthographicSize - scrollInput * zoomSpeed, 1f, 100f);
    }
}

