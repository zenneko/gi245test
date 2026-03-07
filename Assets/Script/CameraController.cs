using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [Header("Move")]
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float xInput;
    [SerializeField] private float zInput;
    [SerializeField] private Transform corner1;
    [SerializeField] private Transform corner2;
    [Header("Zoom")]
    [SerializeField] private float zoomModifier;
    
    public static CameraController instance;

    void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    void Update()
    {
        MoveByKB();
        Zoom();
    }

    private void MoveByKB()
    {
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");
        Vector3 dir = (transform.forward * zInput) + (transform.right * xInput);
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.position = Clamp(corner1.position, corner2.position);
    }

    private Vector3 Clamp(Vector3 lowerLeft, Vector3 topRight)
    {
        return new Vector3(Mathf.Clamp(transform.position.x, lowerLeft.x, topRight.x),
                           transform.position.y,
                           Mathf.Clamp(transform.position.z, lowerLeft.z, topRight.z));
    }

    private void Zoom()
    {
        zoomModifier = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.Z)) zoomModifier = -0.1f;
        if (Input.GetKey(KeyCode.X)) zoomModifier = 0.1f;
        cam.orthographicSize += zoomModifier;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 4, 10);
    }
}