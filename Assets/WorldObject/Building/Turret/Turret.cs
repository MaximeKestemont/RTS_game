using UnityEngine;
using System.Collections;
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
     
    public override bool CanAttack() {
    	Debug.Log("Can attack ? ");
        if ( UnderConstruction() || hitPoints == 0 ) {
        	Debug.Log("Nope...");
        	return false;
        } else { 
        	Debug.Log("YEA...");
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