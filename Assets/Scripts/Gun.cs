// Gun.cs
//
// 銃を撃ちます
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
    /// 銃の発射を行います
    /// </summary>
    public void Shoot()
    {
        // 連射防止
        if (LastShootTime + ShootDelay < Time.time && isShooting)
        {
            audioSource.PlayOneShot(shootSound);
            isShooting = false;
            Animator.SetBool("IsShooting", true);
            ShootingSystem.Play();
            Vector3 direction = GetDirection();

            // Rayを飛ばして当たったオブジェクトを調べる
            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                string hitTag = hit.collider.tag;
                _hitManager.TagCheck(hitTag, hit.collider.gameObject);


                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));

                LastShootTime = Time.time;
            }
            // 何も当たらなかった場合
            else
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, BulletSpawnPoint.position + GetDirection() * 100, Vector3.zero, false));

                LastShootTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 銃弾の方向を取得します
    /// </summary>
    /// <returns>最終的な方向</returns>
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
    /// 銃弾の軌跡を描画します
    /// </summary>
    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {

        Vector3 startPosition = Trail.transform.position; // 開始位置
        float distance = Vector3.Distance(Trail.transform.position, HitPoint); // 開始位置と着弾点の距離
        float remainingDistance = distance; // 残りの距離

        // 移動
        while (remainingDistance > 0)
        {
            // 開始位置から着弾点までの距離に応じて、銃弾の位置を線形補間で更新
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            // 残りの距離を更新
            remainingDistance -= BulletSpeed * Time.deltaTime;

            yield return null;
        }
        Animator.SetBool("IsShooting", false);
        isShooting = true;
        Trail.transform.position = HitPoint;
        // 着弾エフェクトの生成
        if (MadeImpact)
        {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}
