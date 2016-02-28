using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RTS;
 
public class Turret : Building {
     
    private Quaternion aimRotation;
     
    protected override void Start () {
        base.Start ();
        detectionRange = weaponRange;
    }
     
    protected override void Update () {
        base.Update();
        
        // Rotate towards the target
        if (aiming) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
            CalculateBounds();
            //sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
            Quaternion inverseAimRotation = new Quaternion(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
            if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation) {
                aiming = false;
            }
        }
    }

    public override List<string> GetBuildingNameRequirements() {
        return new List<string>() { "WarFactory" };
    }

    public override List<Type> GetBuildingRequirements() {
        return new List<Type>() { typeof(WarFactory) };
    }
     
    public override bool CanAttack() {
        if ( UnderConstruction() || hitPoints == 0 ) {
        	return false;
        } else { 
        	return true;
        }
    }
     
    protected override void UseWeapon () {
        base.UseWeapon();
        Vector3 spawnPoint = transform.position;
        spawnPoint.x += (2.6f * transform.forward.x);
        spawnPoint.y += 5.0f;
        spawnPoint.z += (2.6f * transform.forward.z);

        // Instantiate the projectile
        GameObject gameObject = PhotonNetwork.Instantiate("TurretProjectile", spawnPoint, transform.rotation, 0);
        Projectile projectile = gameObject.GetComponentInChildren< Projectile >();
        projectile.SetRange(0.9f * weaponRange);
        projectile.SetTarget(target);
    }
     
    protected override void AimAtTarget () {
        base.AimAtTarget();
        aimRotation = Quaternion.LookRotation (target.transform.position - transform.position);
    }
}