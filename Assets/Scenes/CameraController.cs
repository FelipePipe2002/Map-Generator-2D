using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 50.0f; // Adjust the camera movement speed
    public float zoomSpeed = 50.0f; // Adjust the camera zoom speed

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftShift)){
            moveSpeed = 200.0f;
            zoomSpeed = 200.0f;
        } else {
            moveSpeed = 50.0f;
            zoomSpeed = 50.0f;
        }
        // Camera Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0.0f);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Camera Zoom Out
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        this.GetComponent<Camera>().orthographicSize = Mathf.Clamp(Mathf.Round(this.GetComponent<Camera>().orthographicSize - scrollInput * zoomSpeed), 1f, 1000f);
    }
}

