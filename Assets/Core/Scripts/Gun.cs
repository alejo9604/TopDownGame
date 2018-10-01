using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    public enum FireMod { Auto, Burst, Single};
    public FireMod fireMod;

    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;

    public Transform shell;
    public Transform shellEjection;
    Muzzleflash muzzleflash;

    float nextShotTime;

    bool triggerReleasedSinceLastShoot;
    int shotsRemainingInBurst;

    private void Start()
    {
        muzzleflash = GetComponent<Muzzleflash>();
        shotsRemainingInBurst = burstCount;
    }

    void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            if(fireMod == FireMod.Burst)
            {
                if (shotsRemainingInBurst == 0)
                    return;
                shotsRemainingInBurst--;
            }
            else if(fireMod == FireMod.Single)
            {
                if (!triggerReleasedSinceLastShoot)
                    return;
            }

            nextShotTime = Time.time + msBetweenShots / 1000;


            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation);
                newProjectile.SetSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShoot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShoot = true;
        shotsRemainingInBurst = burstCount;
    }

}
