using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single}
    public FireMode fireMode;

    public float inaccuracyAngle = 5f;  //max deviation in degrees
    public Transform[] projectileSpawns;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;

    public Transform ejectedShell;
    public Transform shellEjection;
    MuzzleFlash muzzleflash;

    float nextShotTime;
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;

    private void Start()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
    }

    void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            if(fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                    return;
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                    return;
            }

            for (int i = 0; i < projectileSpawns.Length; i++)
            {

                nextShotTime = Time.time + msBetweenShots / 1000f;

                Quaternion baseRotation = projectileSpawns[i].rotation;
                float angleOffset = Random.Range(-inaccuracyAngle, inaccuracyAngle);    //random angle offset around Z axis
                Quaternion finalRotation = baseRotation * Quaternion.Euler(0f, 0f, angleOffset);    //apply random rotation around Z axis

                Projectile newProjectile = Instantiate(projectile, projectileSpawns[i].position, finalRotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);

            }

            Instantiate(ejectedShell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
