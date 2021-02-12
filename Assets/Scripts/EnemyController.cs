using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    public bool isShotByPlayer = false;
    [SerializeField] float maxVelocity = 1f;
    [SerializeField] float mass = 10f;
    [SerializeField] float followDistance = 0.2f;
    [SerializeField] float slowRadius = 4;
    [SerializeField] float rotationSpeed = 8;

    private enum SteeringBehaviour { Pursue, Flee, Follow, Patrol, Evade}
    private SteeringBehaviour behaviour;
    private Vector3 velocity = Vector3.zero;
    private float targetSpeed, currentSpeed, speedSmoothVelocity;
    private float speedSmoothTime = 0.2f;
    private Transform followTarget;

    // For changing body color material individually
    
    private Renderer renderer;
    private MaterialPropertyBlock propBlock;

    [SerializeField] float randomWanderRadius;
    [SerializeField] GameObject[] patrolNodes;
    Transform currentPatrolTarget;

    float minFleeSeconds = 5f;
    float fleeTimer;

    private void Awake()
    {
        // For changing body color
        propBlock = new MaterialPropertyBlock();
        renderer = GetComponentInChildren<Renderer>();
        
        // Start with color red
        renderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", new Color(1, 0, 0, 1));
        renderer.SetPropertyBlock(propBlock, 0);

        fleeTimer = minFleeSeconds;
    }

    private void Start()
    {
        behaviour = SteeringBehaviour.Patrol;
        SearchForPatrolNodes();
    }

    private void SearchForPatrolNodes()
    {
        patrolNodes = GameObject.FindGameObjectsWithTag("PatrolNode");
    }

    private void Update()
    {
        BehaviourLogic();

        switch (behaviour)
        {
            case SteeringBehaviour.Pursue:
                velocity = Seek(transform.position, followTarget.position, velocity);
                break;
            case SteeringBehaviour.Flee:
                velocity = Flee(transform.position, followTarget.position, velocity);
                break;
            case SteeringBehaviour.Follow:
                velocity = Follow(transform.position, followTarget.position, velocity);
                break;
            case SteeringBehaviour.Patrol:
                velocity = PatrolSeek(transform.position, patrolNodes, velocity);
                break;
            case SteeringBehaviour.Evade:
                break;
        }

        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), rotationSpeed * Time.deltaTime);
    }

    private void BehaviourLogic()
    {
        float distanceToPlayer = 0;

        distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
        // If shot by player, become a player follower drone.
        if (isShotByPlayer)
        {
            EnableFollowPlayer();
        }

        // Take back drone
        if (behaviour == SteeringBehaviour.Pursue && distanceToPlayer < followDistance / 3 && fleeTimer <= 0)
        {
            // Convert last drone in the player follower list back to an enemy
            if (PlayerController.Instance.minionTrail.Count != 0)
                PlayerController.Instance.minionTrail[PlayerController.Instance.minionTrail.Count - 1].GetComponent<EnemyController>().ConvertToEnemy();
            else
            {
                PlayerController.Instance.isDead = true;
            }
                

            behaviour = SteeringBehaviour.Flee;

            fleeTimer = minFleeSeconds;
        }

        if (fleeTimer > 0)
        {
            fleeTimer -= Time.deltaTime;
        }

        // If in aggro range
        if (distanceToPlayer < PlayerController.Instance.aggroRange)
        {
            if (!isShotByPlayer)
            {
                Vector3 targetDir = PlayerController.Instance.transform.position - transform.position;
                Vector3 playerForward = -PlayerController.Instance.transform.forward;

                // In front of the player
                float frontAngle = Vector3.Angle(targetDir, playerForward);
                if (frontAngle < PlayerController.Instance.frontAggroConeDegree / 2 && !PlayerController.Instance.isDead)
                {
                    behaviour = SteeringBehaviour.Flee;
                    followTarget = PlayerController.Instance.transform;
                }

                // Back of the player
                float backAngle = Vector3.Angle(targetDir, -playerForward);
                if (backAngle < PlayerController.Instance.backAggroConeDegree / 2 && fleeTimer <= 0 && !PlayerController.Instance.isDead)
                {
                    behaviour = SteeringBehaviour.Pursue;
                    followTarget = PlayerController.Instance.transform;
                }
            }
        }
        else
        {
            if (!isShotByPlayer)
            {
                behaviour = SteeringBehaviour.Patrol;
                followTarget = null;
            }
        }
    }

    private Vector3 Follow(Vector3 _currentPosition, Vector3 _targetPosition, Vector3 _velocity)
    {
        Vector3 targetDirection = _targetPosition - _currentPosition;
        targetDirection = targetDirection.normalized * maxVelocity;

        float distance = Vector3.Distance(_currentPosition, _targetPosition);
        if (distance < slowRadius)
        {
            targetDirection = targetDirection * (distance / slowRadius);
        }

        Vector3 steering = targetDirection - velocity;
        steering /=  mass;

        _targetPosition.y = transform.position.y;
        transform.LookAt(_targetPosition);

        if (Vector3.Distance(transform.position, _targetPosition) < followDistance)
        {
            //if (PlayerController.Instance.currentSpeed <= 0.2f)
            targetSpeed = 0;
        }
        else
        {
            targetSpeed = 1;
        }

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        return (velocity + steering) * currentSpeed;
    }

    public void EnableFollowPlayer()
    {
        // Change body color to blue
        renderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", new Color(0, 0, 1, 1));
        renderer.SetPropertyBlock(propBlock, 0);

        // Set behaviour to follow player trail and set followtarget
        behaviour = SteeringBehaviour.Follow;
        fleeTimer = minFleeSeconds;

        if(!PlayerController.Instance.minionTrail.Contains(gameObject))
            PlayerController.Instance.minionTrail.Add(gameObject);

        if (PlayerController.Instance.minionTrail[0] == this.gameObject)
        {
            followTarget = PlayerController.Instance.transform;
        }
        else
        {
            int index = PlayerController.Instance.minionTrail.IndexOf(gameObject);
            followTarget = PlayerController.Instance.minionTrail[index - 1].transform;
        }
    }

    public void ConvertToEnemy()
    {
        // Change body color to red
        renderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", new Color(1, 0, 0, 1));
        renderer.SetPropertyBlock(propBlock, 0);
        SoundManager.instance.PlaySound(1);

        PlayerController.Instance.minionTrail.Remove(this.gameObject);

        behaviour = SteeringBehaviour.Patrol;
        isShotByPlayer = false;
    }

    public Vector3 Flee(Vector3 _currentPosition, Vector3 _targetPosition, Vector3 _velocity)
    {
        Vector3 targetDir = _currentPosition - _targetPosition;
        if (targetDir.magnitude < 0.1f)
        {
            targetDir = new Vector3(1, 0, 0);
        }
        targetDir = targetDir.normalized * maxVelocity;
        Vector3 steering = targetDir - _velocity;
        steering = steering / mass;

        return _velocity + steering;
    }

    public Vector3 PatrolSeek(Vector3 currentPosition, GameObject[] nodes, Vector3 velocity)
    {
        if (currentPatrolTarget == null) currentPatrolTarget = nodes[Random.Range(0, nodes.Length)].transform;

        var newVelocity = Seek(currentPosition, currentPatrolTarget.position, velocity);
        if (Vector3.Distance(currentPosition, currentPatrolTarget.position) < 10f)
        {
            var currentIndex = System.Array.IndexOf(nodes, currentPatrolTarget.gameObject);
            currentPatrolTarget = nodes[(currentIndex + 1) % nodes.Length].transform;
        }
        return newVelocity * 0.7f;
    }

    

    public Vector3 Seek(Vector3 currentPosition, Vector3 targetPosition, Vector3 velocity)
    {
        Vector3 targetDir = targetPosition - currentPosition;
        targetDir = targetDir.normalized * maxVelocity;
        Vector3 steering = targetDir - velocity;
        steering = steering / mass;
        return velocity + steering;
    }

    
}
