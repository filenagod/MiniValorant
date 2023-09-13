using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Move Settings")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float chaseRange = 5f;
    [SerializeField] float turnSpeed = 15f;
    [SerializeField] float patrolRaidus = 6f;
    [SerializeField] float patrolWaitTime = 2f;
    [SerializeField] float chaseSpeed = 4f;
    [SerializeField] float searchSpeed = 3f; 

    private bool isSearched = false;
    private bool isAttacking = false;
    enum State
    {
        Idle,
        Search,
        Chase,
        Attack

    }

    [SerializeField] private State currentState = State.Idle;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
    }
    void Start()
    {
        
    }

    
    void Update()
    {
        //agent.SetDestination(player.position);
        StateCheck();
        StateExecute();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        switch (currentState)
        {

            case State.Search:
                Gizmos.color = Color.green;
                Vector3 targetPos = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);
                Gizmos.DrawLine(transform.position, targetPos);
                break;
            case State.Chase:
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, player.position);
                break;
            case State.Attack:
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
                break;

        }
    }


    private void StateCheck()
    {
       float distanceToTarget = Vector3.Distance(player.position, transform.position);

        if(distanceToTarget <= chaseRange && distanceToTarget > attackRange)
        {
            currentState = State.Chase;
        }
        else if(distanceToTarget <= attackRange)
        {
            currentState = State.Attack;
        }
        else
        {
            currentState = State.Search;
        }
    }

    private void StateExecute()
    {
        switch (currentState)
        {
            case State.Idle:
                break;
            case State.Search:
                if (!isSearched && agent.remainingDistance <= 0.1f ||
                    !agent.hasPath && !isSearched)
                {
                    Vector3 agentTarget = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);
                    agent.enabled = false;
                    transform.position = agentTarget;
                    agent.enabled = true;

                    Invoke("Search", patrolWaitTime);
                    isSearched = true;
                }
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attcak();
                break;
        }
    }

    private void Search()
    {
        agent.isStopped = false;
        agent.speed = searchSpeed;
        isSearched = false;
        agent.SetDestination(GetRandomPosition());
    }
    private void Chase()
    {
        if (player == null)
        {
            return;
        }
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void Attcak()
    {
        if (player == null)
        {
            return;
        }
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        LookTheTarget(player.position);
    }

   

    private void LookTheTarget(Vector3 target)
    {
        Vector3 lookPos = new Vector3(target.x, transform.position.y, target.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos - transform.position),
            turnSpeed * Time.deltaTime);
    }
    private Vector3 GetRandomPosition()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRaidus;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, patrolRaidus, 1);
        return hit.position;
    }
}
