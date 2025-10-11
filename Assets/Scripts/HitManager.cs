// HitManager.cs
// 
// オブジェクトの当たり判定を管理します
// 

using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class HitManager : MonoBehaviour
{
    [SerializeField] [Tag]
    private string enemyTag; // 敵のタグ
    [SerializeField][Tag]
    private string princessTag; // 救護対象のタグ

    [SerializeField] private List<GameObject> pairObj = new List<GameObject>();

    private void Awake()
    {
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (GameObject obj in targetObjects)
        {
            pairObj.Add(obj);
        }
    }

    public void TagCheck(string TargetTag, GameObject TargetObj)
    {
        if (TargetTag == enemyTag)
        {
            TargetObj.GetComponent<Animator>().SetBool("isDead", true);
            TargetObj.GetComponent<Collider>().enabled = false;
            pairObj.Remove(TargetObj);
        }
        else if (TargetTag == princessTag)
        {
            Debug.Log("プリンセスに命中");
            Destroy(TargetObj); // 敵オブジェクトを破壊
        }
        else
        {
            Debug.Log("その他に命中");
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