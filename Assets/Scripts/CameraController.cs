using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float dragSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    private Vector3 dragOrigin;

    void Update()
    {
        HandleMouseDrag();
        HandleZoom();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 difference = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(-difference.x * dragSpeed, -difference.y * dragSpeed, 0);
            Camera.main.transform.Translate(move, Space.World);
            dragOrigin = Input.mousePosition;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera mainCam = Camera.main;

        if (mainCam.orthographic)
        {
            mainCam.orthographicSize -= scroll * zoomSpeed;
            mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize, minZoom, maxZoom);
        }
        else
        {
            mainCam.transform.position += mainCam.transform.forward * scroll * zoomSpeed;
        }
    }
}