using UnityEngine;
using System.Collections;
 
public class Projectile : MonoBehaviour {
 
    public float velocity = 1;
    public int damage = 1;
 
    private float range = 1;
    private WorldObject target;
 
 	// Process the movement of the projectile, towards the target. If the movement is finished, but the target is not there anymore,
 	// (because it has moved or was destroyed by another projectile), destroy the projectile, without doing any damage.
    void Update () {
        if ( HitSomething() ) {
            InflictDamage();
            Destroy(gameObject);
            // TODO animation (explosion,...)
        }

        // Make the projectile move if not yet to the target
        if ( range > 0 ) {
            float positionChange = Time.deltaTime * velocity;
            range -= positionChange;
            transform.position += (positionChange * transform.forward);
        } else {
            Destroy(gameObject);
        }
    }
 
    public void SetRange(float range) {
        this.range = range;
    }
 
    public void SetTarget(WorldObject target) {
        this.target = target;
    }
 
 	// Note : Only detect if the target was hit ! This means that if another object is in the way, it will NOT hit it (which is intended, even if not realistic)
    private bool HitSomething() {
        if (target && target.GetSelectionBounds().Contains(transform.position)) {
        	return true;
        } else {
        	return false;
        }
    }
 
    private void InflictDamage() {
        if (target) {
        	target.TakeDamage(damage);
        }
    }
}