// BoolCheck.cs
//
// ステージ開始時に特定の条件をチェックし、オブジェクトの移動を開始する
//

using UnityEngine;

public class BoolCheck : MonoBehaviour
{
    [SerializeField] private HitManager _hitManager;
    [SerializeField] private HitManagerSolo _hitManagerSolo;
    [SerializeField] private ObjectsMover _objectsMover;

    private bool isPlayMode;

    private void Start()
    {
        isPlayMode = false;
    }

    private void Update()
    {
        if(!isPlayMode && _hitManagerSolo.IsStart)
        {
            isPlayMode = true;
            _objectsMover.ObjectsMove();
        }
    }
}
