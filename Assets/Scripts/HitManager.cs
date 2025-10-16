// HitManager.cs
// 
// オブジェクトの当たり判定を管理します
// 

using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif


public class HitManager : MonoBehaviour
{
    [SerializeField] [Tag]
    private string enemyTag; // 敵のタグ
    [SerializeField][Tag]
    private string princessTag; // 救護対象のタグ

    [SerializeField] private List<GameObject> enemyObj = new List<GameObject>();
    [SerializeField] private List<GameObject> princessObj = new List<GameObject>();

    [SerializeField] private int score; // スコア 
    [SerializeField] private int enemyCount; // 敵の数

    private void Awake()
    {
        score = 0;

        // EnemyタグとPrincessタグのオブジェクトをリストに格納します
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject[] saveObjects = GameObject.FindGameObjectsWithTag(princessTag);
        foreach (GameObject obj in targetObjects)
        {
            enemyObj.Add(obj);
            obj.GetComponent<Collider>().enabled = false;
        }
        foreach (GameObject obj in saveObjects)
        {
            princessObj.Add(obj);
            obj.GetComponent<Collider>().enabled = false;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            foreach(GameObject proj in enemyObj)
            {
                proj.GetComponent<Animator>().SetTrigger("isStart");
                proj.GetComponent<Collider>().enabled = true;
            }

            foreach (GameObject proj in princessObj)
            {
                proj.GetComponent<Animator>().SetTrigger("isStart");
                proj.GetComponent<Collider>().enabled = true;
            }
        }
    }

    /// <summary>
    /// フェーズ開始の準備をします
    /// </summary>
    private void SetUp()
    {
        enemyCount = 0;
        // enemyObjからランダムなオブジェクト有効化します。
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
            TargetObj.GetComponent<Collider>().enabled = false;
            score++;
        }
        // princessだった場合、死亡アニメーションを再生し、リストから削除、減点します
        else if (TargetTag == princessTag)
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
            TargetObj.GetComponent<Collider>().enabled = false;
            score--;
        }
        // PracticeTargetだった場合、死亡アニメーションを再生します
        else if (TargetTag == "PracticeTarget")
        {
            TargetObj.GetComponent<Animator>().SetTrigger("isDead");
            TargetObj.GetComponent<Collider>().enabled = false;
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