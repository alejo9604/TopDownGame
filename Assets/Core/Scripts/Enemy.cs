using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LeavingEntity
{

    public enum State { Idle, Chasing, Attacking };
    State currentState;

    public ParticleSystem deathEffect;

    NavMeshAgent pathFinder;
    LeavingEntity targetEntity;
    Transform target;

    Material skinMaterial;
    Color originalColor;

    float attacktDistanceThreshold = 0.5f;
    float timeBetweenAttacks = 1;
    float damage = 1f;

    float nextAttackTime = 0;
    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;
	
	protected override void Start () {
        base.Start();
        pathFinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LeavingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());
        }

        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if(damage >= health)
        {
            Destroy( Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }


    void Update () {
        
        if (hasTarget && Time.time > nextAttackTime && currentState == State.Chasing)
        {
            float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
            float threshold = attacktDistanceThreshold + myCollisionRadius + targetCollisionRadius;
            if (sqrDstToTarget < (threshold * threshold))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());

            }
        }
        

	}


    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathFinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTraget = (target.position - transform.position).normalized;
        Vector3 attackPosition= target.position - dirToTraget * myCollisionRadius;

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if(percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-(percent * percent) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            
            yield return null;
        }

        skinMaterial.color = originalColor;


        currentState = State.Chasing;
        pathFinder.enabled = true;
    }


    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        if (pathFinder.isStopped)
            pathFinder.isStopped = false;

        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTraget = (target.position - transform.position).normalized;
                Vector3 taergetPostion = target.position - dirToTraget * (myCollisionRadius + targetCollisionRadius + attacktDistanceThreshold/2);
                //new Vector3(target.position.x, 0, target.position.z);
                if (!dead)
                    pathFinder.SetDestination(taergetPostion);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
