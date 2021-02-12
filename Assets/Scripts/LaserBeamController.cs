using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamController : MonoBehaviour
{
    [SerializeField] float movementSpeed = 100f;
    [SerializeField] float maxLifeTime = 1.5f;

    Ray ray;
    RaycastHit hit;

    int layerMask;


    private void Awake()
    {
        layerMask = LayerMask.GetMask("Enemy");
    }

    private void Start()
    {
        
    }

    void FixedUpdate()
    {
        transform.Translate(0, 0, movementSpeed * Time.deltaTime);

        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime < 0)
        {
            Destroy(gameObject);
        }

        ray = new Ray(transform.position, -transform.forward);

        if (Physics.Raycast(ray, out hit, 1.5f, layerMask))
        {
            if (!hit.transform.GetComponentInParent<EnemyController>().isShotByPlayer)
            {
                hit.transform.GetComponentInParent<EnemyController>().isShotByPlayer = true;
                SoundManager.instance.PlaySound(2);
                Destroy(gameObject);
            }
        }
    }
}
