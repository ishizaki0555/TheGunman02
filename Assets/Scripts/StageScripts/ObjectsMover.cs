// ObjectsMover.cs
//
// ユニットを指定の位置まで移動させる
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum moveState
{
    Moving,     // 移動中
    Waiting,    // 待機中
    Dead        // 死亡
}

/// <summary>
/// ユニットの移動設定データ
/// </summary>
[System.Serializable]
public class UnitMoveSettings
{
    public GameObject unit;                                     // 移動するオブジェクト
    public List<Vector3> targetPos = new List<Vector3>();       // 移動先リスト 
    public float moveSpeed;                                     // 移動速度
    public float standbyTime;                                   // 移動先での待機時間
    public float rotatingSpeed;                                 // 回転速度
    public moveState currentState;                              // 現在の移動状態
}


public class ObjectsMover : MonoBehaviour
{
    public List<UnitMoveSettings> unitMoveSettings;    // 各ユニットの移動設定リスト

    /// <summary>
    /// ユニットの移動を開始させます
    /// </summary>
    public void ObjectsMove()
    {
        // 各オブジェクトの移動を開始
        foreach(var moveData in unitMoveSettings)
        {
            StartCoroutine(MoveObjectCoroutine(moveData));
        }
    }

    /// <summary>
    /// ユニットを指定の位置まで移動させます
    /// </summary>
    /// <param name="unitMoveSettings">ユニットの移動設定リスト</param>
    IEnumerator MoveObjectCoroutine(UnitMoveSettings unitMoveSettings)
    {
        if(!unitMoveSettings.unit) yield break;

        NavMeshAgent agent = unitMoveSettings.unit.GetComponent<NavMeshAgent>();
        if(!agent) yield break;

        agent.speed = unitMoveSettings.moveSpeed;

        foreach(var nextPos in unitMoveSettings.targetPos)
        {
            unitMoveSettings.currentState = moveState.Moving;
            agent.SetDestination(nextPos);

            // 到達するまで待つ
            while(true)
            {
                if(!agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance &&
                    !agent.hasPath)
                {
                    break;
                }
                yield return null;
            }

            // 待機
            unitMoveSettings.currentState = moveState.Waiting;
            yield return new WaitForSeconds(unitMoveSettings.standbyTime);
        }
    }
}
