using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveResource : MonoBehaviour
{
    [Header("Resources to Give")]
    [SerializeField] private float health;
    [SerializeField] private float energy;
    [SerializeField] private float ammo;
    [SerializeField] private float lifetime;

    private Health _targetHealth;
    private Energy _targetEnergy;

    private GameObject _target;
    

    // Start is called before the first frame update
    void Start()
    {
        _target = GameObject.Find("Player");
        _targetHealth = _target.GetComponent<Health>();
        _targetEnergy = _target.GetComponent<Energy>();
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
            Destroy(gameObject);
    }

    // Function sets new values for target's resources, making it possible to give multiple resources in one item.
    // Function checks if there's anything to give to the target, reducing unneccessary script calls.
    private void GiveResources(float healthAmount, float energyAmount, float ammoAmount)
    {
        if(healthAmount >= 0)
        {
            _targetHealth.SetHealth(healthAmount);
        }
        if(energyAmount >= 0)
        {
            _targetEnergy.SetEnergy(energyAmount);
        }
        if(ammoAmount >= 0)
        {
            //Give some ammo here
        }
    }

    //// When colliding with Resource item, give the target given resource amounts.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GiveResources(health, energy, ammo);
            Destroy(gameObject);
        }
    }
}
