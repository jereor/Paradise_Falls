using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParticleCollision : MonoBehaviour
{
    private void OnParticleCollision(GameObject collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent(out GroundEnemyAI enemyScript))
            {
                enemyScript.KnockBackEnemy(this.gameObject, 300);
            }

        }
    }
}
