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
    public int magazineSize;
    public float reloadTime = 0.3f;

    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(1.5f,2.5f);
    public Vector2 recoilAngleMinMax = new Vector2(10, 20);
    public float recoilMoveSettleTime = 0.15f;
    public float recoilRotationSettleTime = 0.1f;

    [Header("Effects")]
    public Transform ejectedShell;
    public Transform shellEjection;
    MuzzleFlash muzzleflash;

    float nextShotTime;
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int ammoRemainingInMag;
    bool isReloading;

    Vector2 recoilSmoothDampVelocity;
    float recoilRotSmoothDampVelocity;
    float recoilAngle;

    private void Start()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        ammoRemainingInMag = magazineSize;
    }

    private void Update()
    {
        //animate recoil
        transform.localPosition = Vector2.SmoothDamp(transform.localPosition, Vector2.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles =  Vector3.forward * recoilAngle * Mathf.Sign(transform.localScale.y);

        if(!isReloading && ammoRemainingInMag == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if (!isReloading && Time.time > nextShotTime && ammoRemainingInMag > 0)
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
                if(ammoRemainingInMag == 0)
                {
                    break;
                }
                ammoRemainingInMag--;

                nextShotTime = Time.time + msBetweenShots / 1000f;

                Quaternion baseRotation = projectileSpawns[i].rotation;
                float angleOffset = Random.Range(-inaccuracyAngle, inaccuracyAngle);    //random angle offset around Z axis
                Quaternion finalRotation = baseRotation * Quaternion.Euler(0f, 0f, angleOffset);    //apply random rotation around Z axis

                Projectile newProjectile = Instantiate(projectile, projectileSpawns[i].position, finalRotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);

            }

            Transform ejectedShellInstance = Instantiate(ejectedShell, shellEjection.position, shellEjection.rotation);
            //.since gun flips when aiming left, do this
            if (transform.localScale.y < 0)
                ejectedShellInstance.localEulerAngles += new Vector3(0, 0, 180);
            muzzleflash.Activate();
            transform.localPosition -= Vector3.right * Random.Range(kickMinMax.x, kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
        }
    }

    public void Reload()
    {
        if(!isReloading && ammoRemainingInMag != magazineSize)
            StartCoroutine(AnimateReload_IEnum());
    }

    private IEnumerator AnimateReload_IEnum()
    {
        isReloading = true;
        yield return new WaitForSeconds(0.2f);

        float percent = 0;
        float reloadSpeed = 1f / reloadTime;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        while(percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.forward * reloadAngle * Mathf.Sign(transform.localScale.y);

            yield return null;
        }

        isReloading = false;
        ammoRemainingInMag = magazineSize;
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
