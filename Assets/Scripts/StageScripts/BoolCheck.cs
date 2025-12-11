// BoolCheck.cs
//
// ステージ開始時に特定の条件をチェックし、オブジェクトの移動を開始する
//

using UnityEngine;

public class BoolCheck : MonoBehaviour
{
    [SerializeField] private HitManager _hitManager;
    [SerializeField] private ObjectsMover _objectsMover;

    private bool isPlayMode;

    private void Start()
    {
        isPlayMode = false;
    }

    /// <summary>
    /// ステージ開始条件をチェックし、条件が満たされたらオブジェクトの移動を開始する
    /// </summary>
    private void Update()
    {
        if(!isPlayMode && _hitManager.IsStart)
        {
            isPlayMode = true;
            _objectsMover.ObjectsMove();
        }
    }
}
