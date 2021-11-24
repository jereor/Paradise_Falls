using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    public static SFXController Instance { get; private set; }

    [Header("Player Sound Effects")]
    [SerializeField] private AudioSource[] playerSteps;
    [SerializeField] private AudioSource playerJump;
    [SerializeField] private AudioSource playerAirJump;
    [SerializeField] private AudioSource playerAirDive;
    [SerializeField] private AudioSource playerLanding_Soft;
    [SerializeField] private AudioSource playerLanding_Hard;
    [SerializeField] private AudioSource playerDash;
    [SerializeField] private AudioSource playerTakeDamage;

    [Header("Shield Sound Effects")]
    [SerializeField] private AudioSource shield_On;
    [SerializeField] private AudioSource shield_Off;
    [SerializeField] private AudioSource shield_Stay;
    [SerializeField] private AudioSource shieldBlock;
    [SerializeField] private AudioSource shieldParry_Melee;
    [SerializeField] private AudioSource shieldParry_Projectile;

    [Header("Multitool Sound Effects")]
    [SerializeField] private AudioSource multitoolThrow;
    [SerializeField] private AudioSource multitoolHit_Ground;
    [SerializeField] private AudioSource multitoolHit_Enemy;
    [SerializeField] private AudioSource multitoolHit_GrapplePoint;
    [SerializeField] private AudioSource multitoolRecall;
    [SerializeField] private AudioSource multitoolGrapple_Start;
    [SerializeField] private AudioSource multitoolGrapple_End;
    [SerializeField] private AudioSource multitoolGrapple_Stay;

    [Header("Shockwave Sound Effects")]
    [SerializeField] private AudioSource shockwaveAttack_Start;
    [SerializeField] private AudioSource shockwaveAttack_End;
    [SerializeField] private AudioSource shockwaveAttack_Stay;

    [Header("Box Sound Effects")]
    [SerializeField] private AudioSource boxMove;
    [SerializeField] private AudioSource boxChainCut;
    [SerializeField] private AudioSource boxGroundHit;
    [SerializeField] private AudioSource boxDestroy;

    [Header("Conveyor Belt Sound Effects")]
    [SerializeField] private AudioSource conveyorBelt_On;
    [SerializeField] private AudioSource conveyorBelt_Off;
    [SerializeField] private AudioSource conveyorBelt_Stay;

    [Header("Laser Door Sound Effects")]
    [SerializeField] private AudioSource laserDoor_On;
    [SerializeField] private AudioSource laserDoor_Off;
    [SerializeField] private AudioSource laserDoor_Stay;

    [Header("Lever & Button Sound Effects")]
    [SerializeField] private AudioSource leverUse;
    [SerializeField] private AudioSource buttonUse;

    [Header("Turret Sound Effects")]
    [SerializeField] private AudioSource turretShoot;
    [SerializeField] private AudioSource turretTakeDamage;
    [SerializeField] private AudioSource turretDestroy;

    [Header("Aero Drone Sound Effects")]
    [SerializeField] private AudioSource aeroDroneMove;
    [SerializeField] private AudioSource aeroDroneShoot;
    [SerializeField] private AudioSource aeroDroneTakeDamage;
    [SerializeField] private AudioSource aeroDroneDestroy;

    [Header("Worker Drone Sound Effects")]
    [SerializeField] private AudioSource workerDroneMove;
    [SerializeField] private AudioSource workerDroneMelee;
    [SerializeField] private AudioSource workerDroneTakeDamage;
    [SerializeField] private AudioSource workerDroneDestroy;

    private void Awake()
    {
        Instance = this;
    }

    public void Test()
    {
        Debug.Log("This is Test");
    }

    public void PlayRandomPlayerStepSound()
    {
        int random = Random.Range(0, 5);
        playerSteps[random].Play();
    }

    public void PlayPlayerJumpSound()
    {
        playerJump.Play();
    }

    public void PlayPlayerAirJumpSound()
    {
        playerAirJump.Play();
    }

    public void PlayPlayerAirDiveSound()
    {
        playerAirDive.Play();
    }

    public void PlayPlayerLanding_Soft()
    {
        playerLanding_Soft.Play();
    }

    public void PlayPlayerLanding_Hard()
    {
        playerLanding_Hard.Play();
    }
}
