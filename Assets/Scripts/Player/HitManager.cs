// HitManager.cs
// 
// ユニットのペアの当たり判定を管理します
// 

using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.UI;
using JetBrains.Annotations;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class HitManager : MonoBehaviour
{
    [Header("タグ設定")]
    [SerializeField] [Tag]
    private string enemyTag; // 敵のタグ
    [SerializeField][Tag]
    private string princessTag; // 救護対象のタグ[
    [SerializeField][Tag]
    private string parentTag; // 親のタグ

    [Header("カメラ関連")]
    // シネマシーンのカメラオブジェクト
    [SerializeField] private CinemachineCamera ChineCameraOut; // 外視点のシネマシーン
    [SerializeField] private CinemachineCamera ChineCameraIn; // 一人称のシネマシーン
    [SerializeField] private CinemachineInputAxisController _chineCameraInController; // 一人称カメラの入力コントローラー
    [SerializeField] private GameObject gunObj; // ガンオブジェクト
    [SerializeField] private GameObject GameCanvas; // ゲームキャンバスオブジェクト

    [Header("オブジェクトのリスト")]
    [SerializeField] private List<GameObject> enemyObj = new List<GameObject>(); // 敵のオブジェクトリスト
    [SerializeField] private List<GameObject> princessObj = new List<GameObject>(); // 救護対象のオブジェクトリスト
    [SerializeField] private List<GameObject> parentObj = new List<GameObject>(); // 親のオブジェクトリスト

    [Header("各数値")]
    [SerializeField] private int score; // スコア 
    [SerializeField] private int outPoint; // 減点数
    [SerializeField] private int enemyCount; // 敵の数
    [SerializeField] private int attackPoint; // 攻撃ポイント
    [Range(0, 100)]
    [SerializeField] private float phaseTime = 30f; // フェーズの時間
    [SerializeField] private float breakTime = 2f; // 休憩時間
    [SerializeField] private float setUpTime = 6f; // フェーズ開始準備時間
    [SerializeField] private float ScreenMoveTime = 3f; // 画面移動時間 

    [Header("UI関連")]
    [SerializeField] private float countDownStartWaitTime = 3f; // カウントダウン開始までの待機時間
    [SerializeField] private TextMeshProUGUI timeText; // タイム表示用UI
    [SerializeField] private TextMeshProUGUI countDownText; // カウントダウン表示用UI
    [SerializeField] private TextMeshProUGUI enemyCountText; // 敵の数表示用UI
    [SerializeField] private string countDownEndText;
    private bool isStart = false; // フェーズ開始フラグ
    private bool endGame = false; // ゲーム終了フラグ 
    [SerializeField] private TextMeshProUGUI resultAttackPoint;
    [SerializeField] private TextMeshProUGUI resultOutPoint;
    [SerializeField] private TextMeshProUGUI resultTortalPoint;

    [Header("SE関連")]
    private AudioSource audio;
    [SerializeField] private AudioClip countDownSE; // カウントダウンSE
    [SerializeField] private AudioClip phaseStartSE; // フェーズ開始SE
    [SerializeField] private AudioClip gameEndSE; // ゲーム終了SE

    public bool IsStart { get => isStart; private set => isStart = value; }

    /// <summary>
    /// 初期化処理を行います。
    /// </summary>
    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        _chineCameraInController.enabled = false;
        Invoke("StartSetUp", ScreenMoveTime);
        gunObj.SetActive(false);
    }

    /// <summary>
    /// 最初のフェーズの開始準備を行います
    /// </summary>
    private void StartSetUp()
    {
        _chineCameraInController.enabled = true;
        score = 0;

        ChineCameraIn.Priority = 1;
        ChineCameraOut.Priority = 0;

        // EnemyタグとPrincessタグのオブジェクトと、その二つの親ををリストに格納します
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject[] saveObjects = GameObject.FindGameObjectsWithTag(princessTag);
        GameObject[] parent = GameObject.FindGameObjectsWithTag(parentTag);
        foreach (GameObject obj in targetObjects)
        {
            enemyObj.Add(obj);
            obj.GetComponent<Animator>().SetTrigger("isDown");
            obj.GetComponent<Collider>().enabled = false;
        }
        foreach (GameObject obj in saveObjects)
        {
            princessObj.Add(obj);
            obj.GetComponent<Animator>().SetTrigger("isDown");
            obj.GetComponent<Collider>().enabled = false;
        }
        foreach (GameObject obj in parent)
        {
            parentObj.Add(obj);
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
        while(count > 0)
        {
            audio.PlayOneShot(countDownSE);
            countDownText.text = count.ToString();
            countDownText.gameObject.GetComponent<Animator>().SetTrigger("isCount");
            count--;
            yield return new WaitForSeconds(1f);
        }
        audio.PlayOneShot(phaseStartSE);
        countDownText.text = countDownEndText;
        countDownText.gameObject.GetComponent<Animator>().SetTrigger("isCount");
        yield return new WaitForSeconds(1f);
    }

    private void Update()
    {
        // フェーズ時間のカウントダウン
        // フェーズ時間が０になったら、isDown()を呼び出します
        if (IsStart)
        {
            phaseTime -= Time.deltaTime;
            timeText.text = phaseTime.ToString("00.0");
            if(phaseTime <= 0)
            {
                audio.PlayOneShot(gameEndSE);
                IsStart = false;
                endGame = true;
                _chineCameraInController.enabled = false;
                resultTortalPoint.text = score.ToString();
                resultAttackPoint.text = attackPoint.ToString();
                resultOutPoint.text = outPoint.ToString();
                isDown();
                GameCanvas.GetComponent<Animator>().SetTrigger("GameEnd");
            }
        }
    }

    /// <summary>
    /// フェーズ開始の準備をします
    /// １．エネミーと一般市民のオブジェクトをランダムに有効化します。
    /// ２．エネミーの数をカウントします。
    ///  3．エネミーの数が０だった場合、再度ランダムに有効化します。
    /// </summary>
    private void SetUp()
    {
        if(!endGame)
        {
            enemyCount = 0;
            IsStart = true;
            gunObj.SetActive(true);
            // enemyObjからランダムなオブジェクト有効化します。
            foreach (GameObject proj in parentObj)
            {
                // if文でbool値をランダムに生成し、true(1)だった場合に有効化し、enemyCountを加算します。
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    proj.transform.GetChild(0).GetComponent<Animator>().SetTrigger("isStart");
                }
                else
                {
                    proj.transform.GetChild(1).GetComponent<Animator>().SetTrigger("isStart");
                    enemyCount++;
                    enemyCountText.text = enemyCount.ToString();
                }
            }
            if (enemyCount == 0 && IsStart)
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
        foreach (GameObject proj in parentObj)
        {
            proj.transform.GetChild(0).GetComponent<Animator>().SetTrigger("isDown");
            proj.transform.GetChild(1).GetComponent<Animator>().SetTrigger("isDown");
        }
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
            if (enemyCount == 0 && IsStart)
            {
                Invoke("isDown", breakTime);
                Invoke("SetUp", setUpTime);
            }
        }
        // princessだった場合、死亡アニメーションを再生し、リストから削除、減点します
        else if (TargetTag == princessTag)
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
            outPoint++;
            score--;
        }
        // PracticeTargetだった場合、死亡アニメーションを再生します
        else if (TargetTag == "PracticeTarget")
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
        }
    }
}

/// <summary>
/// タグを専用UIに表示させるための属性
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class TagAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TagAttribute))]
public class TagAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 対象のプロパティが文字列かどうか
        if(property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // タグのリストを取得
        var tag = EditorGUI.TagField(position, label, property.stringValue);

        // タグ名を反映
        property.stringValue = tag;
    }
}
#endif