using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{
    [SerializeField]
    private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private float BulletSpeed = 100;
    [SerializeField]
    private AudioClip shootSound;

    private Animator Animator;
    private AudioSource audioSource;
    private float LastShootTime;
    private bool isShooting = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Animator = GetComponent<Animator>();
    }

    public void Shoot()
    {
        if (LastShootTime + ShootDelay < Time.time && isShooting)
        {
            audioSource.PlayOneShot(shootSound);
            isShooting = false;
            Animator.SetBool("IsShooting", true);
            ShootingSystem.Play();
            Vector3 direction = GetDirection();

            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));

                LastShootTime = Time.time;
            }
            // this has been updated to fix a commonly reported problem that you cannot fire if you would not hit anything
            else
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, BulletSpawnPoint.position + GetDirection() * 100, Vector3.zero, false));

                LastShootTime = Time.time;
            }
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;

        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {
        // This has been updated from the video implementation to fix a commonly raised issue about the bullet trails
        // moving slowly when hitting something close, and not
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= BulletSpeed * Time.deltaTime;

            yield return null;
        }
        Animator.SetBool("IsShooting", false);
        isShooting = true;
        Trail.transform.position = HitPoint;
        if (MadeImpact)
        {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}
