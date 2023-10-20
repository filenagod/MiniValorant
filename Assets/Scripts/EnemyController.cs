using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    Animator anim;
    private NavMeshAgent agent;
    private Transform player;
    private PlayerMovement playerMovement;

    [Header("Move Settings")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float chaseRange = 20f;
    [SerializeField] float turnSpeed = 15f;
    [SerializeField] float patrolRaidus = 10f;
    [SerializeField] float patrolWaitTime = 2f;
    [SerializeField] float chaseSpeed = 4f;
    [SerializeField] float searchSpeed = 10f;
    [SerializeField] float health = 100f;
    [SerializeField] int damage = 2;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private float shootRange = 10f;
    [SerializeField] private LayerMask playerLayer;
    public ParticleSystem muzzleFlashEnemy;
    public ParticleSystem bloodPlayer;
    public GameObject effectPrefab;
    private Camera mainCamera;
    public float lastAttackTime;
    private bool isSearched = false;
    private bool isAttacking = false;
    [SerializeField] private PanelController panelController;
    // The prefab of the effect
    public float damageAmount = 50f; // Amount of damage to deal to the object
    private bool isEffectActive = false;
    private float effectDuration = 10f;
    private bool hasEffectBeenTriggered = false;
    public float playInterval = 1.0f;
    private float lastPlayTime;
    private int lastIndex = -1;
    private bool landSoundPlayed = true;
    private bool dead;
    [SerializeField] List<AudioClip> footStepSounds = new List<AudioClip>();
    [SerializeField] private AudioClip clipToPlay1;
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipToPlay2;
    private AudioSource explosionSourcee;
    [SerializeField] private AudioClip clipToPlay3;
    private AudioSource fireSourcee;
    [SerializeField] private AudioClip clipToPlay4;
    private AudioSource berachSourcee;
    
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
        playerMovement = player.GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        explosionSourcee = gameObject.AddComponent<AudioSource>();
        fireSourcee = gameObject.AddComponent<AudioSource>();
        berachSourcee = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay1;
        fireSourcee.clip = clipToPlay2;
        explosionSourcee.clip = clipToPlay3;
        berachSourcee.clip = clipToPlay4;
        mainCamera = Camera.main;
    }

    
    void Update()
    {
        if(dead) return;
        //agent.SetDestination(player.position);
        StateCheck();
        StateExecute();

        if (Input.GetKeyDown(KeyCode.X) && !hasEffectBeenTriggered)
        {
            berachSourcee.Play();
            TakeDamage(50f);
            Invoke("StartEffect", 2f);
            Invoke("PlaySound", 2f);
        }
    }

    void PlaySound()
    {
        hasEffectBeenTriggered = true;
        explosionSourcee.Play();
        fireSourcee.Play();
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
                    anim.SetBool("Walk", false);
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
        anim.SetBool("Walk", true);
        agent.SetDestination(GetRandomPosition());
        PlayFootstepSound();
    }
    private void Chase()
    {
        if (player == null)
        {
            return;
        }
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        anim.SetBool("Walk", true);
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
        anim.SetBool("Walk", false);
        anim.SetTrigger("Attack");
        LookTheTarget(player.position);
        if (Time.time - lastAttackTime < 0.5f)
        {
            return;
        }
        muzzleFlashEnemy.Play();
        audioSource.Play();
        lastAttackTime = Time.time;
        if (aimTransform != null)
        {
            bool hit = Physics.Raycast(aimTransform.position, transform.forward, shootRange, playerLayer);
            if (hit)
            {
                playerMovement.TakeDamage(damage);
                CreateBloodSplatter(mainCamera.transform.position + mainCamera.transform.forward * 0.1f);
            }
            Debug.DrawRay(aimTransform.position, transform.forward * shootRange, Color.blue);
        }
        //bool hit = Physics.Raycast(aimTransform.position, transform.forward, shootRange, playerLayer);
        
    }

    public void PlayFootstepSound()
    {
        if (footStepSounds.Count > 0 && audioSource != null)
        {
            //bir önceki seçtiðimizi bir daha seçmemek için böyle bir döngü kullanabilir
            int index;
            do
            {
                index = UnityEngine.Random.Range(0, footStepSounds.Count);
                if (lastIndex != index)
                {
                    audioSource.PlayOneShot(footStepSounds[index]);
                    lastIndex = index;
                    break;
                }
            }
            while (index == lastIndex);
        }
    }

    void CreateBloodSplatter(Vector3 hitPoint)
    {
        // Kan efekti oluþtur
        ParticleSystem bloodEffect = Instantiate(bloodPlayer, hitPoint, Quaternion.identity);
        Destroy(bloodEffect.gameObject, 2f);
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
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        dead = true;
        anim.SetTrigger("Dead");
        agent.enabled = false;
        panelController.Remove(this);
        Destroy(this);
        Debug.Log("Eliff");
    }


    void StartEffect()
    {
        isEffectActive = true;
        GameObject effect = Instantiate(effectPrefab, transform.position, transform.rotation);
        Destroy(effect, effectDuration);
        TakeDamage();
        StartCoroutine(WaitAndDisableEffect());
    }

    void TakeDamage()
    {
        Target health = GetComponent<Target>();

        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
    }

    IEnumerator WaitAndDisableEffect()
    {
        yield return new WaitForSeconds(effectDuration);
        isEffectActive = false;
    }
}
