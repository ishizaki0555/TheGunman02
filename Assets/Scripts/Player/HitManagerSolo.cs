// HitManagerSolo.cs
//
// 個々のユニットのヒット管理を行う
//

using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitManagerSolo : MonoBehaviour
{
    [Header("タグ設定")]
    [Tag]
    [SerializeField] private string enemyTag;                               // 敵のタグ
    [Tag]
    [SerializeField] private string princessTag;                            // 救護対象のタグ

    [SerializeField] private GameObject gunObj;                             // ガンオブジェクト
    [SerializeField] private GameObject GameCanvas;                         // ゲームキャンバスオブジェクト
    [SerializeField] private Transform cameraPos;                           // カメラポジションオブジェクト
    [SerializeField] private Camera mainCamera;                             // メインカメラオブジェクト

    [Header("レイヤー設定")]
    public string invisibleLayerName = "InvisibleToCamera";
    [SerializeField] private GameObject playerObj;                          // プレイヤーオブジェクト

    [Header("オブジェクトのリスト")]
    [SerializeField] private List<GameObject> enemyObjs = new List<GameObject>();    // 敵のオブジェクトリスト
    [SerializeField] private List<GameObject> princessObjs = new List<GameObject>(); // 救護対象のオブジェクトリスト

    [Header("各数値")]
    [SerializeField] private int score;                             // スコア 
    [SerializeField] private int outPoint;                          // 減点数
    [SerializeField] private int enemyCount;                        // 敵の数
    [SerializeField] private int attackPoint;                       // 攻撃ポイント
    [Range(0, 100)]
    [SerializeField] private float phaseTime = 30f;                 // フェーズの時間
    [SerializeField] private float breakTime = 2f;                  // 休憩時間
    [SerializeField] private float setUpTime = 6f;                  // フェーズ開始準備時間
    [SerializeField] private float ScreenMoveTime = 3f;             // 画面移動時間 
    [SerializeField] private float cameraMoveTime = 2f;             // カメラ移動時間

    [Header("UI関連")]
    [SerializeField] private float countDownStartWaitTime = 3f;     // カウントダウン開始までの待機時間
    [SerializeField] private TextMeshProUGUI timeText;              // タイム表示用UI
    [SerializeField] private TextMeshProUGUI countDownText;         // カウントダウン表示用UI
    [SerializeField] private TextMeshProUGUI enemyCountText;        // 敵の数表示用UI
    [SerializeField] private string countDownEndText;
    private bool isStart = false;                                   // フェーズ開始フラグ
    private bool endGame = false;                                   // ゲーム終了フラグ 
    [SerializeField] private TextMeshProUGUI resultAttackPoint;     // 結果画面攻撃ポイント表示用UI
    [SerializeField] private TextMeshProUGUI resultOutPoint;        // 結果画面減点ポイント表示用UI
    [SerializeField] private TextMeshProUGUI resultLimitTime;       // 結果画面合計ポイント表示用UI

    [Header("SE関連")]
    private AudioSource audio;
    [SerializeField] private AudioClip countDownSE;                 // カウントダウンSE
    [SerializeField] private AudioClip phaseStartSE;                // フェーズ開始SE
    [SerializeField] private AudioClip gameEndSE;                   // ゲーム終了SE
    [SerializeField] private AudioClip _mainBGM;                    // メインBGM

    public bool IsStart { get => isStart; private set => isStart = value; }
    public bool EndGame { get => endGame; set => endGame = value; }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        Invoke("StartSetUp", ScreenMoveTime);
        gunObj.SetActive(false);
    }

    private void StartSetUp()
    {
        score = 0;

        InvisiblePlayerCamera();

        // カメラの位置を移動させます
        StartCoroutine(CameraSet());

        // EnemyタグとPrincessタグのオブジェクトをリストに格納します
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject[] saveObjects = GameObject.FindGameObjectsWithTag(princessTag);
        foreach (GameObject obj in targetObjects)
        {
            enemyObjs.Add(obj);
            obj.GetComponent<Animator>().SetTrigger("isDown");
            obj.GetComponent<Collider>().enabled = false;
        }
        foreach (GameObject obj in saveObjects)
        {
            princessObjs.Add(obj);
            obj.GetComponent<Animator>().SetTrigger("isDown");
            obj.GetComponent<Collider>().enabled = false;
        }
        // フェーズ開始の準備をします
        StartCoroutine(countDownAnim());
        Invoke("SetUp", setUpTime);
    }
    /// <summary>
    /// カウントダウンアニメーションを再生します
    /// </summary>
    private IEnumerator countDownAnim()
    {
        // countDownStartWaitTime秒間待機した後、三秒間のカウントダウンを表示します
        yield return new WaitForSeconds(countDownStartWaitTime);
        int count = 3;
        while (count > 0)
        {
            audio.PlayOneShot(countDownSE);
            countDownText.text = count.ToString();
            countDownText.gameObject.GetComponent<Animator>().SetTrigger("isCount");
            count--;
            yield return new WaitForSeconds(1f);
        }
        audio.PlayOneShot(phaseStartSE);
        IsStart = true;
        countDownText.text = countDownEndText;
        countDownText.gameObject.GetComponent<Animator>().SetTrigger("isCount");
        audio.clip = _mainBGM;
        audio.Play();
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// カメラをプレイヤー位置に移動させます
    /// </summary>
    private IEnumerator CameraSet()
    {
        float elapsedTime = 0.0f;         // 経過時間
        Vector3 startingPos = mainCamera.transform.position; // カメラの開始位置
        Quaternion startingRot = mainCamera.transform.rotation; // カメラの開始回転
        Quaternion targetRotation = cameraPos.rotation; // カメラの目標回転

        while (elapsedTime < cameraMoveTime)
        {
            mainCamera.gameObject.transform.position = new Vector3(
                Mathf.Lerp(startingPos.x, cameraPos.position.x, elapsedTime / cameraMoveTime),
                Mathf.Lerp(startingPos.y, cameraPos.position.y, elapsedTime / cameraMoveTime),
                Mathf.Lerp(startingPos.z, cameraPos.position.z, elapsedTime / cameraMoveTime)
                );
            float t = elapsedTime / cameraMoveTime;
            mainCamera.gameObject.transform.rotation = Quaternion.Slerp(startingRot, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        mainCamera.gameObject.transform.position = cameraPos.position;
        mainCamera.gameObject.transform.rotation = targetRotation;
        yield return null;
    }

    private void Update()
    {
        // フェーズ時間のカウントダウン
        // フェーズ時間が０になったら、isDown()を呼び出します
        if (IsStart)
        {
            phaseTime -= Time.deltaTime;
            timeText.text = phaseTime.ToString("00.0");
            // 残り時間がなくなるか、敵が全滅したらフェーズ終了
            if (phaseTime <= 0 || enemyCount == 0)
            {
                audio.Stop();
                audio.PlayOneShot(gameEndSE);
                IsStart = false;
                EndGame = true;
                resultLimitTime.text = phaseTime.ToString("00.0秒");
                resultAttackPoint.text = attackPoint.ToString();
                resultOutPoint.text = outPoint.ToString();
                isDown();
                GameCanvas.GetComponent<Animator>().SetTrigger("GameEnd");
            }
        }
    }

    /// <summary>
    /// Soloと無印との違い
    /// 親がいないため個別に設定する必要がある
    /// 個別に位置があるため各々で有効化する必要がある
    /// </summary>
    private void SetUp()
    {
        if (!EndGame)
        {
            enemyCount = 0;
            isStart = true;
            gunObj.SetActive(true);
            // エネミーの有効化
            foreach (GameObject enemy in enemyObjs)
            {
                enemy.GetComponent<Animator>().SetTrigger("isStart");
                enemyCount++;
                enemyCountText.text = enemyCount.ToString();
            }
            // 一般市民の有効化
            foreach (GameObject princess in princessObjs)
            {
                princess.GetComponent<Animator>().SetTrigger("isStart");
            }
            if (enemyCount == 0 && isStart)
            {
                Invoke("isDown", breakTime);
                Invoke("SetUp", setUpTime);
            }
        }
    }

    /// <summary>
    /// 各オブジェクトの降下アニメーションを再生します
    /// </summary>
    private void isDown()
    {
        // エネミーの無効化
        foreach (GameObject enemy in enemyObjs)
        {
            enemy.GetComponent<Animator>().SetTrigger("isDown");
        }
        // 一般市民の無効化
        foreach (GameObject princess in princessObjs)
        {
            princess.GetComponent<Animator>().SetTrigger("isDown");
        }
    }

    /// <summary>
    /// プレイヤーをカメラから見えなくします
    /// </summary>
    private void InvisiblePlayerCamera()
    {
        int invisibleLayer = LayerMask.NameToLayer(invisibleLayerName);
        playerObj.layer = invisibleLayer;
    }

    /// <summary>
    /// 弾が当たったオブジェクトのタグから処理を分岐させます
    /// </summary>
    /// <param name="TargetTag">弾が当たったオブジェクトのタグ</param>
    /// <param name="TargetObj">弾が当たったオブジェクト</param>
    public void TagCheck(string TargetTag, GameObject TargetObj)
    {
        // enemyだった場合、死亡アニメーションを再生し、リストから削除、加点します
        if (TargetTag == enemyTag)
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
            score++;
            attackPoint++;
            enemyCount--;
            enemyCountText.text = enemyCount.ToString();
        }
        // princessだった場合、死亡アニメーションを再生し、リストから削除、減点します
        else if (TargetTag == princessTag)
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
            outPoint++;
            score--;
        }
        else
        {
            // 何も当たらなかった場合銃が打てなくなるので何もしない動作を追加する
            return;
        }
    }
}