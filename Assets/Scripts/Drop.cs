using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]

public class Drop : MonoBehaviour
{
    [SerializeField] Weapon weaponToDrop;
    [SerializeField] Vector3 Angle = Vector3.zero;
    [SerializeField] private AudioClip clipToPlay;
    private AudioSource audioSource;
    private BoxCollider dropBox;
    private GameObject weapon;
    private bool canPickupWeapon = false;
    // Start is called before the first frame update
    private void Awake()
    {
        dropBox = GetComponent<BoxCollider>();
        dropBox.isTrigger = true;
        dropBox.size *= 3;
    }
    void Start()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true; // Etkinliði etkinleþtirin
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay;
        if (weaponToDrop != null)
        {
            weapon = Instantiate(weaponToDrop.GetWeaponPrefab, transform.position, transform.rotation, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Angle, Space.World);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !canPickupWeapon)
        {
            canPickupWeapon = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (canPickupWeapon && Input.GetKeyDown(KeyCode.E))
            {
                if (weaponToDrop != null)
                {
                    
                    other.GetComponent<AttackController>().EquipWeapon(weaponToDrop);
                }
                Destroy(weapon);
                Destroy(gameObject,3f);
                Invoke("Source", 0.3f);
            }
        }
    }

    public void Source()
    {
        audioSource.Play();
    }
}
