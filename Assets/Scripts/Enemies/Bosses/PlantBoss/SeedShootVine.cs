using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SeedShootVine : MonoBehaviour
{
    private BigPlantController plantController;

    private GameObject plantBoss;

    private GameObject target;

    [SerializeField] private float grappleVineStretchDuration; // Duration the vine changes the local scale.
    [SerializeField] private float grappleVineStretchAmount; // How far the vine reaches.
    [SerializeField] private float vineSpeed = 1; // Local vine speed used in all other situations except when boss is spawning them personally.

    [SerializeField] private bool isSeedVineActivated = false;
    [SerializeField] private bool isSeedVineDeactivated = false;
    // Start is called before the first frame update
    void Start()
    {
        plantBoss = GameObject.Find("PlantBoss");
        plantController = plantBoss.GetComponent<BigPlantController>();
        target = GameObject.Find("Player");
        vineSpeed = plantController.GetSpeedMultiplier();
        transform.parent.localScale = new Vector2(1, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSeedVineActivated)
        {
            StartCoroutine(StretchVine());
        }
        if(!plantController.GetIsSettingUpForSeedShoot() && !isSeedVineDeactivated)
        {
            StartCoroutine(StretchBackVine());
        }
    }

    // THe whole behaviour for the vine except the rotation.
    private IEnumerator StretchVine()
    {
        isSeedVineActivated = true;
        transform.parent.DOScaleY(grappleVineStretchAmount, grappleVineStretchDuration * vineSpeed); // After waiting the given time it stretches in the direction given.
        yield return new WaitForSeconds(grappleVineStretchDuration * vineSpeed);
    }

    private IEnumerator StretchBackVine()
    {
        isSeedVineDeactivated = true;
        transform.parent.DOScaleY(0.1f, grappleVineStretchDuration * vineSpeed); // And back to normal.
        yield return new WaitForSeconds(grappleVineStretchDuration * vineSpeed);

        plantController.grappleVineInstances.Remove(gameObject.transform.parent.gameObject); // Remove the vine from the list. Destroy the game object afterwards.
        Destroy(gameObject.transform.parent.gameObject);
    }

}
