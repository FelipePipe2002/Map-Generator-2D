using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 50.0f; // Adjust the camera movement speed
    public float zoomSpeed = 50.0f; // Adjust the camera zoom speed
    private Camera cameraComp;
    private void Start() {
        cameraComp = this.GetComponent<Camera>();
    }
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
        cameraComp.orthographicSize = Mathf.Clamp(Mathf.Round(cameraComp.orthographicSize - scrollInput * zoomSpeed), 1f, 1000f);
    }
}

