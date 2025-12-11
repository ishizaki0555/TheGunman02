using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class EnemyAutoGenetirrung : EditorWindow
{
    private GeneratorType generatorType;        // 生成タイプ
    private GameObject[] baseObject;            // 生成する敵のベースオブジェクト
    private CharaType _chataType;               // 敵のタイプ
    private int crearCount;                     // 生成数

    // ランダム生成用のパラメータ
    private Vector3 GeneretorCecter;            // 生成の中心位置
    private float GeneretorRadiusu;             // 生成の半径

    // 移動ルート生成用のパラメータ
    private Transform[] movePoint;              // 移動ルートのポイント
    private ObjectsMover objectsMover;          // オブジェクト移動スクリプト
    private int movePointIndex;                 // 移動ポイントのインデックス

    [MenuItem("Tools/敵の自動生成")]
    public static void ShowWindow()
    {
        GetWindow<EnemyAutoGenetirrung>("敵の自動生成");
    }

    /// <summary>
    /// GUIの描画を行います。
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("敵の自動生成ツール", EditorStyles.boldLabel);
        generatorType = (GeneratorType)EditorGUILayout.EnumPopup("生成タイプ", generatorType);
        _chataType = (CharaType)EditorGUILayout.EnumPopup("生成物のタイプ", _chataType);
        crearCount = EditorGUILayout.IntField("生成数", crearCount);

        // 生成タイプに応じて処理を分岐
        // ランダム生成タイプの場合
        if (generatorType == GeneratorType.GENERATOR_TYPE_RANDOM)
        {
            GUILayout.Space(20);
            GeneretorCecter = EditorGUILayout.Vector3Field("生成の中心位置", GeneretorCecter);
            GeneretorRadiusu = EditorGUILayout.FloatField("生成の半径", GeneretorRadiusu);
            int size = EditorGUILayout.IntField("生成する敵の種類数", baseObject != null ? baseObject.Length : 0);
            if (baseObject == null || baseObject.Length != size) baseObject = new GameObject[size];
            for (int i = 0; i < size; i++)
            {
                baseObject[i] = (GameObject)EditorGUILayout.ObjectField("敵のベースオブジェクト " + (i + 1), baseObject[i], typeof(GameObject), false);
            }
            if (GUILayout.Button("ランダム座標からエンティティを自動生成"))
            {
                GenerateEnemies();
            }
        }
        // 移動ポイント生成タイプの場合
        else if (generatorType == GeneratorType.GENERATOR_TYPE_MOVE)
        {
            GUILayout.Space(20);
            int size = EditorGUILayout.IntField("生成する敵の種類数", baseObject != null ? baseObject.Length : 0);
            if (baseObject == null || baseObject.Length != size) baseObject = new GameObject[size];
            for (int i = 0; i < size; i++)
            {
                baseObject[i] = (GameObject)EditorGUILayout.ObjectField("敵のベースオブジェクト " + (i + 1), baseObject[i], typeof(GameObject), false);
            }
            if(GUILayout.Button("移動ポイントからエンティティを生成"))
            {
                MovePointGenerat();
            }
            if (GUILayout.Button("エンティティを削除"))
            {
                DeleteObject();
            }
        }
    }

    /// <summary>
    /// 敵とNPCをCreatCountの数分ランダムに生成する。
    /// </summary>
    private void GenerateEnemies()
    {
        // 生成数が０か、ベースオブジェクトが設定されていない場合は警告を表示して終了
        if (baseObject == null || crearCount == 0)
        {
            Debug.LogWarning("生成する敵のベースオブジェクトが設定されていないか、生成数が0です。");
            return;
        }

        // エンティティの生成処理
        for(int i = 0; i < crearCount; i++)
        {
            // 敵かNPCのどちらかをランダムに選択
            _chataType = (CharaType)Random.Range(0, 2);
            GameObject entity = baseObject[(int)_chataType];

            // ====================
            // 中心座標を基準に生成位置を決定
            // ====================
            // 入力してある半径内のランダムな位置を生成
            float radius = GeneretorRadiusu;
            Vector3 randomOffset = new Vector3(
                Random.Range(-radius, radius),
                0f,
                Random.Range(-radius, radius)
                );

            Vector3 spawnPosition = GeneretorCecter + randomOffset;

            // ====================
            // 敵またはNPCの生成
            // ====================
            GameObject enemy = Instantiate(entity, spawnPosition, Quaternion.identity);
            enemy.name = entity.name + "_" + i.ToString("D3");
        }
    }

    private void MovePointGenerat()
    {
        if (baseObject == null || crearCount == 0)
        {
            Debug.LogWarning("生成する敵のベースオブジェクトが設定されていないか、生成数が0です。");
            return;
        }
        movePoint = GameObject.Find("MovePoint").GetComponentsInChildren<Transform>();
        if (movePoint.Length <= 1)
        {
            Debug.LogWarning("移動ポイントが設定されていません。");
            return;
        }
        objectsMover = FindAnyObjectByType<ObjectsMover>();
        if (objectsMover == null)
        {
            Debug.LogError("objectsMoverが見つかりませんでした。");
        }
        for (int i = 0; i < crearCount; i++)
        {
            // 敵かNPCのどちらかをランダムに選択
            _chataType = (CharaType)Random.Range(0, 2);
            GameObject entity = baseObject[(int)_chataType];

            // ====================
            // 移動ポイントから生成位置を決定
            // ====================
            int pointIndex = Random.Range(1, movePoint.Length);
            Vector3 spawnPosition = movePoint[pointIndex].position;

            // ====================
            // 敵またはNPCの生成
            // ====================
            GameObject enemy = Instantiate(entity, spawnPosition, Quaternion.identity);
            enemy.name = "Entity_" + i.ToString("D3");

            // ====================
            // 敵またはNPCの移動ルート設定
            // ==================
            UnitMoveSettings moveSettings = new UnitMoveSettings();
            moveSettings.unit = enemy;
            for (int j = 1; j < movePoint.Length; j++)
            {
                moveSettings.targetPos.Add(movePoint[j]);
            }
            moveSettings.moveSpeed = 2.0f;
            moveSettings.standbyTime = 1.0f;
            moveSettings.rotatingSpeed = 5.0f;
            objectsMover.unitMoveSettings.Add(moveSettings);
        }
    }

    private void DeleteObject()
    {
        objectsMover = FindAnyObjectByType<ObjectsMover>();
        var entitys = GameObject.FindGameObjectsWithTag("Enemy");
        var npcs = GameObject.FindGameObjectsWithTag("Queen");
        foreach (var entity in entitys)
        {
            DestroyImmediate(entity);
        }
        foreach (var npc in npcs)
        {
            DestroyImmediate(npc);
        }
        objectsMover.unitMoveSettings.Clear();
    }
}
#endif

/// <summary>
/// 生成タイプ
/// </summary>
public enum GeneratorType
{
    GENERATOR_TYPE_RANDOM = 0,
    GENERATOR_TYPE_MOVE,
}

/// <summary>
/// キャラクターのタイプ
/// </summary>
public enum CharaType
{
    CHARA_TYPE_ENEMY = 0,
    CHARA_TYPE_QUEEN,
}
