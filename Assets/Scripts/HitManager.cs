// HitManager.cs
// 
// オブジェクトの当たり判定を管理します
// 

using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.ComponentModel.Design;



#if UNITY_EDITOR
using UnityEditor;
#endif


public class HitManager : MonoBehaviour
{
    [SerializeField] [Tag]
    private string enemyTag; // 敵のタグ
    [SerializeField][Tag]
    private string princessTag; // 救護対象のタグ[
    [SerializeField][Tag]
    private string parentTag; // 親のタグ

    [SerializeField] private List<GameObject> enemyObj = new List<GameObject>();
    [SerializeField] private List<GameObject> princessObj = new List<GameObject>();
    [SerializeField] private List<GameObject> parentObj = new List<GameObject>();

    [SerializeField] private int score; // スコア 
    [SerializeField] private int enemyCount; // 敵の数

    [SerializeField] private float phaseTime = 30f; // フェーズの時間
    [SerializeField] private float breakTime = 2f; // 休憩時間
    [SerializeField] private float setUpTime = 6f; // フェーズ開始準備時間

    private void Awake()
    {
        score = 0;

        // EnemyタグとPrincessタグのオブジェクトをリストに格納します
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
        Invoke("SetUp", setUpTime);
    }

    /// <summary>
    /// フェーズ開始の準備をします
    /// １．エネミーと一般市民のオブジェクトをランダムに有効化します。
    /// </summary>
    private void SetUp()
    {
        enemyCount = 0;
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
            }
        }
        if (enemyCount == 0)
        {
            Invoke("isDown", breakTime);
            Invoke("SetUp", setUpTime);
        }
    }

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
            enemyCount--;
            if(enemyCount == 0)
            {
                Invoke("isDown", breakTime);
                Invoke("SetUp", setUpTime);
            }
        }
        // princessだった場合、死亡アニメーションを再生し、リストから削除、減点します
        else if (TargetTag == princessTag)
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
            score--;
        }
        // PracticeTargetだった場合、死亡アニメーションを再生します
        else if (TargetTag == "PracticeTarget")
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
        }
        // それ以外
        else
        {
            // 何もしない
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