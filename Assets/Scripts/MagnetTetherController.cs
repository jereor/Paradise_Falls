using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetTetherController : MonoBehaviour
{
    private GameObject weaponInstance;
    [SerializeField] private GameObject player;
    private ParticleSystem magnetTether;
    // Start is called before the first frame update
    void Start()
    {
        magnetTether = GetComponent<ParticleSystem>();
        gameObject.SetActive(false);
        weaponInstance = GetComponentInParent<PlayerCombat>().getWeaponInstance();
    }

    // Update is called once per frame
    void Update()
    {
        // Find the thrown weapon so the tether can be adjusted accordingly.
        weaponInstance = GetComponentInParent<PlayerCombat>().getWeaponInstance();

        // If the weapon instance exists, calculate the vector from player to weapon and set the tether in the middle between the two.
        // Stretch the particle effect's scale depending on the distance and rotate it.
        if (weaponInstance)
        {
            Vector3 vectorToTarget = weaponInstance.transform.position - player.transform.position;
            transform.position = new Vector2(player.transform.position.x + vectorToTarget.x / 2, player.transform.position.y + vectorToTarget.y / 2);

            float angle = Mathf.Atan2(-vectorToTarget.x, vectorToTarget.y) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = q;

            var shape = magnetTether.shape;
            shape.scale = new Vector2(magnetTether.shape.scale.x, vectorToTarget.magnitude);
        }
    }
}
