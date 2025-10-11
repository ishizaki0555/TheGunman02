// Gun.cs
//
// �e�������܂�
//

using System.Collections;
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

    [SerializeField] private HitManager _hitManager;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Animator = GetComponent<Animator>();
        _hitManager = FindAnyObjectByType<HitManager>();
    }

    /// <summary>
    /// �e�̔��˂��s���܂�
    /// </summary>
    public void Shoot()
    {
        // �A�˖h�~
        if (LastShootTime + ShootDelay < Time.time && isShooting)
        {
            audioSource.PlayOneShot(shootSound);
            isShooting = false;
            Animator.SetBool("IsShooting", true);
            ShootingSystem.Play();
            Vector3 direction = GetDirection();

            // Ray���΂��ē��������I�u�W�F�N�g�𒲂ׂ�
            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                string hitTag = hit.collider.tag;
                _hitManager.TagCheck(hitTag, hit.collider.gameObject);


                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));

                LastShootTime = Time.time;
            }
            // ����������Ȃ������ꍇ
            else
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, BulletSpawnPoint.position + GetDirection() * 100, Vector3.zero, false));

                LastShootTime = Time.time;
            }
        }
    }

    /// <summary>
    /// �e�e�̕������擾���܂�
    /// </summary>
    /// <returns>�ŏI�I�ȕ���</returns>
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

    /// <summary>
    /// �e�e�̋O�Ղ�`�悵�܂�
    /// </summary>
    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {

        Vector3 startPosition = Trail.transform.position; // �J�n�ʒu
        float distance = Vector3.Distance(Trail.transform.position, HitPoint); // �J�n�ʒu�ƒ��e�_�̋���
        float remainingDistance = distance; // �c��̋���

        // �ړ�
        while (remainingDistance > 0)
        {
            // �J�n�ʒu���璅�e�_�܂ł̋����ɉ����āA�e�e�̈ʒu����`��ԂōX�V
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            // �c��̋������X�V
            remainingDistance -= BulletSpeed * Time.deltaTime;

            yield return null;
        }
        Animator.SetBool("IsShooting", false);
        isShooting = true;
        Trail.transform.position = HitPoint;
        // ���e�G�t�F�N�g�̐���
        if (MadeImpact)
        {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}
