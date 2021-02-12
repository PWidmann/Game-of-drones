using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    Transform cameraTargetTransform;

    //Camera Zoom 
    [Header("Camera Zoom")]
    [SerializeField] float cameraDistance3D = 6f;
    [SerializeField] float cameraDistance2D = 30f;
    private float targetCameraDistance = 0;
    private float currentCameraDistance = 0;

    //Camera Rotation
    [HideInInspector]
    public bool topViewMode = true;
    private float yaw;
    private float pitch;
    [Header("Camera Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] Vector2 pitchMinMax = new Vector2(-85, 85);
    Vector2 topViewCameraRotation = new Vector2(180f, 90f);
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;
    Vector3 targetRotation;

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        // References
        cameraTargetTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Starting camera rotation
        yaw = topViewCameraRotation.x;
        pitch = topViewCameraRotation.y;
        targetRotation = new Vector3(pitch, yaw);
        targetCameraDistance = cameraDistance2D;
        currentCameraDistance = targetCameraDistance;
        topViewMode = true;

        // Camera smoothing
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) && !GameManager.InEscapeMenu)
        {
            topViewMode = !topViewMode;
            GameManager.TopViewMode = topViewMode;

            if (topViewMode || GameManager.InEscapeMenu)
            {
                yaw = topViewCameraRotation.x;
                pitch = topViewCameraRotation.y;
                targetCameraDistance = cameraDistance2D;
            }
            else
            {
                if (!GameManager.InEscapeMenu && !topViewMode)
                {
                    // Set camera for third person view
                    pitch = 10f;
                    targetCameraDistance = cameraDistance3D;
                }
            }
        }
    }


    void LateUpdate()
    {
        currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetCameraDistance, 6f * Time.deltaTime);

        // If in third person view
        if (!topViewMode && !GameManager.InEscapeMenu)
        {
            // Camera rotation
            yaw += Input.GetAxis("Mouse X") * GameManager.MouseSensitivity;
            pitch += Input.GetAxis("Mouse Y") * GameManager.MouseSensitivity * -1;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        }

        targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;
        transform.position = cameraTargetTransform.position - transform.forward * currentCameraDistance;
    }
}
