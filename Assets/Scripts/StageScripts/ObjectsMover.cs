// ObjectsMover.cs
//
// ユニットを指定の位置まで移動させる
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        var unit = unitMoveSettings.unit;
        unit.transform.position = unitMoveSettings.targetPos[0];
        Collider enemyCollider = unit.transform.GetChild(0).GetComponent<Collider>();

        // 次の目的地まで移動・待機を繰り返す
        foreach (var nextPos in unitMoveSettings.targetPos)
        {
            Vector3 targetPosition = nextPos;
            // unitの状態を移動中に変更し、targetPositionに到達するまで移動
            // 移動し終わったら回転中に状態を変更し、次のtargetPositionまで回転
            while (Vector3.Distance(unit.transform.position, targetPosition) > 0.1f && enemyCollider.enabled != false)
            {
                // 状態を移動中に変更
                unitMoveSettings.currentState = moveState.Moving;

                // 次の目的地まで移動
                unit.transform.position = Vector3.MoveTowards(
                    unit.transform.position,
                    targetPosition,
                    unitMoveSettings.moveSpeed * Time.deltaTime
                    );

                // 目的地の方向を向くように回転
                Vector3 direction = unit.transform.position - targetPosition;
                if(direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    unit.transform.rotation = Quaternion.Slerp(
                        unit.transform.rotation,
                        targetRotation,
                        unitMoveSettings.rotatingSpeed * Time.deltaTime
                        );
                }
                yield return null;
            }

            // 目的地に到着

            // 状態を待機中に変更して待機
            unitMoveSettings.currentState = moveState.Waiting;
            yield return new WaitForSeconds(unitMoveSettings.standbyTime);

        }
        yield return null;
    }
}
