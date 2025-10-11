// HitManager.cs
// 
// �I�u�W�F�N�g�̓����蔻����Ǘ����܂�
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
    private string enemyTag; // �G�̃^�O
    [SerializeField][Tag]
    private string princessTag; // �~��Ώۂ̃^�O

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
            Debug.Log("�v�����Z�X�ɖ���");
            Destroy(TargetObj); // �G�I�u�W�F�N�g��j��
        }
        else
        {
            Debug.Log("���̑��ɖ���");
        }
    }
}

    /// <summary>
    /// �^�O���pUI�ɕ\�������邽�߂̑���
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
        // �Ώۂ̃v���p�e�B�������񂩂ǂ���
        if(property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // �^�O�̃��X�g���擾
        var tag = EditorGUI.TagField(position, label, property.stringValue);

        // �^�O���𔽉f
        property.stringValue = tag;
    }
}
#endif