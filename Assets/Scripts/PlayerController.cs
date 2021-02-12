using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public float movementSpeed = 10f;
    public float aggroRange = 20f;
    public bool shooting = false;

    public bool isDead = false;

    [SerializeField] GameObject laserBeamObject;
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] Transform[] laserCannons;
    [SerializeField] Material backLineMaterial;

    //Movement
    Vector2 inputDir;
    Vector3 velocity;
    Vector3 targetDirection;
    float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    float speedSmoothTime = 0.3f;
    float speedSmoothVelocity;
    public float currentSpeed;
    bool isAiming = false;

    // Shooting
    [SerializeField] float delayTimer = 0.02f;
    float shootTimer = 0f;

    //References
    Transform cameraT;
    CharacterController controller;

    // Snake trail
    public List<GameObject> minionTrail = new List<GameObject>();

    public List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public float frontAggroConeDegree = 30f;
    public float backAggroConeDegree = 140f;
    GameObject tempLR;

    void Start()
    {
        if (Instance == null)
            Instance = this;

        cameraT = Camera.main.transform;
        controller = GetComponent<CharacterController>();

        for (int i = 0; i < 7; i++)
        {
            tempLR = Instantiate(lineRendererPrefab, transform);
            lineRenderers.Add(tempLR.GetComponent<LineRenderer>());
        }
    }

    void Update()
    {
        

        if (!isDead && !GameManager.InEscapeMenu)
        {
            //Movement
            inputDir = GetMovementInput();
            Move(inputDir);
            Aiming();
            Shooting();
            DrawDebugRays();
        }
    }

    private Vector2 GetMovementInput()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        return input.normalized;
    }

    private void Aiming()
    {
        if (Input.GetMouseButton(1))
        {
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }
    }

    private void Shooting()
    {
        
        // Count down shoot delay timer
        if (delayTimer > 0)
            shootTimer -= Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            shooting = true;
            if (shootTimer <= 0)
            {
                SoundManager.instance.PlaySound(3);
                // Shooting from all listed laser cannons
                foreach (Transform cannon in laserCannons)
                {
                    GameObject laserBeam = Instantiate(laserBeamObject, cannon.position + transform.forward, Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z)));
                    laserBeam.transform.rotation = transform.rotation;
                }

                // Reset shoot delay timer
                shootTimer = delayTimer;
            }
        }
        else
        {
            shooting = false;
        }
    }

    private void Move(Vector2 _inputDir)
    {
        // Smooth movement speed 
        float targetSpeed = movementSpeed * _inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        //Player rotation in movement direction (for third person camera)
        if (inputDir != Vector2.zero && !isAiming)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }

        // Aiming player drone
        if (isAiming)
        {
            if (inputDir != Vector2.zero)
            {

                
                if (CameraController.Instance.topViewMode)
                {
                    // Top view
                    targetDirection = new Vector3(-inputDir.x, 0, -inputDir.y);
                }
                else
                {
                    // Third person view
                    targetDirection = new Vector3(inputDir.x, 0, inputDir.y);
                    targetDirection = Camera.main.transform.TransformDirection(targetDirection);
                    targetDirection.y = 0;
                }
            }

            // Direction the player model rotates towards
            Vector3 cameraDirection = Vector3.zero;
            if (CameraController.Instance.topViewMode)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 targetPoint = new Vector3(ray.origin.x, 0, ray.origin.z) - transform.position;
                cameraDirection = targetPoint;
                cameraDirection.y = 0;
            }
            else
            {
                cameraDirection = cameraT.forward;
                cameraDirection.y = 0;
            }

            // Calculate velocity and move player
            velocity = targetDirection.normalized * currentSpeed;
            controller.Move(velocity * Time.deltaTime);
            // Rotate player towards movement
            if(cameraDirection != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraDirection), 12f * Time.deltaTime);
        }
        else
        {
            // If not aiming
            velocity = transform.forward * currentSpeed;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    public void DrawDebugRays()
    {
        if (PlayerInterface.Instance.debugActive)
        {
            foreach (LineRenderer lineR in lineRenderers)
            {
                lineR.enabled = true;
            }

            // Front Right
            Vector3 rightForwardRay = transform.forward;
            rightForwardRay = Quaternion.Euler(0, frontAggroConeDegree / 2, 0) * rightForwardRay;
            // Front Left
            Vector3 lefttForwardRay = transform.forward;
            lefttForwardRay = Quaternion.Euler(0, -frontAggroConeDegree / 2, 0) * lefttForwardRay;
            // Back Right
            Vector3 rightBackwardRay = transform.forward;
            rightBackwardRay = Quaternion.Euler(0, 180f - backAggroConeDegree / 2, 0) * rightBackwardRay;
            // Back Left
            Vector3 leftBackwardRay = transform.forward;
            leftBackwardRay = Quaternion.Euler(0, 180f + backAggroConeDegree / 2, 0) * leftBackwardRay;

            // Front lines
            Vector3[] newPositions = { transform.position, transform.position + rightForwardRay * aggroRange };
            lineRenderers[0].SetPositions(newPositions);
            Vector3[] newPositions2 = { transform.position, transform.position + lefttForwardRay * aggroRange };
            lineRenderers[1].SetPositions(newPositions2);
            Vector3[] frontPositions = { newPositions[1], newPositions2[1] };
            lineRenderers[2].SetPositions(frontPositions);

            // Back lines
            Vector3[] newPositions3 = { transform.position, transform.position + rightBackwardRay * aggroRange };
            lineRenderers[3].SetPositions(newPositions3);
            lineRenderers[3].material = backLineMaterial;
            Vector3[] newPositions4 = { transform.position, transform.position + leftBackwardRay * aggroRange };
            lineRenderers[4].SetPositions(newPositions4);
            lineRenderers[4].material = backLineMaterial;



            Vector3 back = transform.forward;
            back = Quaternion.Euler(0, 180f, 0) * back;

            Vector3 behindPosition = transform.position + back * aggroRange;
            
            Vector3[] backPositions = { newPositions3[1], behindPosition };
            lineRenderers[5].SetPositions(backPositions);
            lineRenderers[5].material = backLineMaterial;

            Vector3[] backPositions2 = { behindPosition, newPositions4[1] };
            lineRenderers[6].SetPositions(backPositions2);
            lineRenderers[6].material = backLineMaterial;
        }
        else
        {
            foreach (LineRenderer lineR in lineRenderers)
            {
                lineR.enabled = false;
            }
        }
    }
}

