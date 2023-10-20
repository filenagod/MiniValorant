using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;


public class AttackController : MonoBehaviour
{
    [SerializeField] Weapon currentWeapon;
    private Transform mainCamera;
    private Animator anim;
    private bool isAttacking = false;
    [SerializeField] float damage = 10f;
    [SerializeField] float range = 100f;
    public GameObject bulletTrailPrefab;
    public ParticleSystem muzzleFlash;
    public ParticleSystem bloodEnemy;
    public float magazine = 30f;
    public float availableBullet = 120f;
    public float bullet = 30f;
    float additionalBullet;
    float reloadTimer;
    public Image ammoFill;
    public GameObject magazinePrefab;
    private Rigidbody rigidbody;
    private bool dusmeDurum = false;
    [SerializeField] private Transform player;

    [SerializeField] private AudioClip clipToPlay;
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipToPlay1;
    private AudioSource magazineSource;
    private bool GetAttacking
    {
        get { return isAttacking; }
    }
    private void Awake()
    {
        mainCamera = GameObject.FindWithTag("CameraPoint").transform;
        anim = mainCamera.transform.GetChild(0).GetComponent<Animator>();
        if (currentWeapon != null)
        {
            SpawnWeapon();
        }
    }



    private void Start()
    {
        rigidbody = magazinePrefab.GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        magazineSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay;
        magazineSource.clip = clipToPlay1;
        rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        ammoFill.fillAmount = bullet / additionalBullet;
        additionalBullet = magazine - bullet;
        if(additionalBullet > availableBullet)
        {
            additionalBullet = availableBullet;
        }
        if(Input.GetKeyDown(KeyCode.R) && additionalBullet > 0 && availableBullet > 0)
            if(Time.time > reloadTimer)
            {

                StartCoroutine(Reload());
                reloadTimer = Time.time;
                dusmeDurum = true;
                magazineSource.Play();
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                player.SetParent(null);
                GameObject newMagazine = Instantiate(magazinePrefab, transform.position, Quaternion.identity);
                newMagazine.transform.parent = null;
            }
        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
        
    }

    private void Attack()
    {
        if(Mouse.current.leftButton.isPressed && !isAttacking && bullet > 0)
        {

            StartCoroutine(AttackRoutine());
        }
    }

    private void SpawnWeapon()
    {
        if (currentWeapon == null)
        {
            return;
        }
        currentWeapon.SpawnNewWeapon(mainCamera.transform.GetChild(0).GetChild(0),anim);
    }

    public void EquipWeapon(Weapon weaponType)
    {
        if (currentWeapon != null)
        {
            currentWeapon.Drop();
        }
        currentWeapon = weaponType;
        SpawnWeapon();
    }

    private IEnumerator AttackRoutine()
    {
        if (bullet <= 0 || isAttacking) 
        {
            yield break;
        }
        isAttacking = true;
        audioSource.Play();
        muzzleFlash.Play();
        bullet--;
        anim.SetTrigger("Attack");
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            EnemyController target = hit.transform.GetComponent<EnemyController>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
            if (hit.transform.CompareTag("Enemy"))
            {
                CreateBloodSplatter(hit.point);
            }
            if (hit.transform.CompareTag("Other"))
            {
                CreateBulletTrail(hit.point);
            }
 
        }
        yield return new WaitForSeconds(currentWeapon.GetAttackRate);
        isAttacking = false;
    }

    void CreateBloodSplatter(Vector3 hitPoint)
    {
        // Kan efekti oluþtur
        ParticleSystem bloodEffect = Instantiate(bloodEnemy, hitPoint, Quaternion.identity);
        Destroy(bloodEffect.gameObject, 2f);
    }
    void CreateBulletTrail(Vector3 hitPoint)
    {
        GameObject bulletTrail = Instantiate(bulletTrailPrefab, hitPoint, Quaternion.identity);
        Destroy(bulletTrail, 1f);
    }

    public int GetDamage()
    {
        if(currentWeapon != null)
        {
            return currentWeapon.GetDamage;
        }
        return 0;
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(1.2f);
        bullet = bullet + additionalBullet;
        availableBullet = availableBullet - additionalBullet;
    }
}
