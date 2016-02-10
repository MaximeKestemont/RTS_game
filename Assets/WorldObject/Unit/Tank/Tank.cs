using UnityEngine;
using System.Collections;
using RTS;

public class Tank : Unit {

	private Quaternion aimRotation;

    protected override void Start () {
        base.Start ();
    }
 
	protected override void Update () {
	    base.Update();
	    
	    // If in aiming mode, perform the rotation
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
    	return true;
	}

	// Face the target.
	protected override void AimAtTarget () {
	    base.AimAtTarget();
	    aimRotation = Quaternion.LookRotation (target.transform.position - transform.position);
	}

	// Create a projectile and "fire" (initialize) it
	protected override void UseWeapon () {
	    base.UseWeapon();
	    Vector3 spawnPoint = transform.position;
	    spawnPoint.x += (2.1f * transform.forward.x);
	    spawnPoint.y += 1.4f;
	    spawnPoint.z += (2.1f * transform.forward.z);
	    GameObject gameObject = PhotonNetwork.Instantiate("TankProjectile", spawnPoint, transform.rotation, 0);
	    Projectile projectile = gameObject.GetComponentInChildren< Projectile >();
	    projectile.SetRange(0.9f * weaponRange);
	    projectile.SetTarget(target);
	}
}
