// TitleCameraManager.cs
//
// タイトル画面でのカメラ位置の管理を行う
//

using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class TitleCameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamraa;                                                 // メインカメラ
    [SerializeField] private float limitCameraMoveTime = 1.0f;                                  // カメラ移動制限時間
    [SerializeField] List<string> SceneNames = new List<string>();                              // 各カメラに対応するシーン名を管理するリスト
    [SerializeField] private string currentSceneName;                                           // 現在選択されているシーン名
    [SerializeField] private Vector3 startCameraPosition;                                       // カメラの開始位置
    [SerializeField] private Vector3 startCameraRotation;                                       // カメラの開始角度
    [SerializeField] List<Vector3> CameraPosition = new List<Vector3>();                        // 各カメラの位置を管理するリスト
    [SerializeField] List<Vector3> CameraRotation = new List<Vector3>();                        // 各カメラの角度を管理するリスト

    private int currentCameraIndex = 0;             // 現在選択されているカメラのインデックス
    private bool canSlected = false;                // カメラが選択されているかどうか
    private bool isStarted = false;                 // タイトル画面が開始されたかどうか
    private Animator _animator;         

    [SerializeField] private bool isExperiment;

    private AudioSource _audio;
    [SerializeField] private AudioClip _selectSE;
    [SerializeField] private AudioClip _decideSE;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        mainCamraa.gameObject.transform.position = startCameraPosition;
        mainCamraa.gameObject.transform.eulerAngles = startCameraRotation;
        Cursor.lockState = CursorLockMode.Locked; // カーソルをロックする
    }

    /// <summary>
    /// タイトル画面での決定アクションです。
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnSelect(InputAction.CallbackContext context)
    {
        // カメラが選択されていない状態で決定ボタンが押されたらカメラを選択状態にする
        if (context.performed && !canSlected && isStarted)
        {
            isStarted = false;
            _animator.SetTrigger("isStart");
            _audio.PlayOneShot(_decideSE);
            currentSceneName = SceneNames[currentCameraIndex];
            StartCoroutine(SetCameraPosition());
            StartCoroutine(SetCameraRotation());
        }
        // 何もないシーンに移り、記録されているシーン名のシーンからオブジェクトを読み込む
        else if (context.performed && canSlected && !isExperiment)
        {
            // カメラが選択された状態で決定ボタンが押されたら次のシーンへ
            PlayerPrefs.SetString("SelectedScene", currentSceneName);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
        }
        // 記録されているシーン名のシーンからオブジェクトを非同期で読み込む
        else if (context.performed && canSlected && isExperiment)
        {
            StartCoroutine(LoadYourAsyncScene());
        }
    }

    /// <summary>
    /// 次のカメラへの切り替えアクションです。
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnNextCamera(InputAction.CallbackContext context)
    {
        if(context.performed && canSlected)
        {
            currentCameraIndex++;
            currentCameraIndex = (int)Mathf.Repeat(currentCameraIndex, SceneNames.Count);
            _animator.SetTrigger("isRight");
            SelectCamera();
        }
    }

    /// <summary>
    /// 前のカメラへの切り替えアクションです。
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnPrevCamera(InputAction.CallbackContext context)
    {
        if(context.performed && canSlected)
        {
            currentCameraIndex--;
            currentCameraIndex = (int)Mathf.Repeat(currentCameraIndex, SceneNames.Count);
            _animator.SetTrigger("isLeft");
            SelectCamera();
        }
    }

    /// <summary>
    /// カメラの位置を設定します
    /// </summary>
    private IEnumerator SetCameraPosition()
    {
        // 経過時間の初期化と開始地点の取得
        float elapsedTime = 0.0f; // 経過時間
        Vector3 startPosition = mainCamraa.transform.position;

        // 制限時間まで毎フレーム補間を続ける
        while (elapsedTime < limitCameraMoveTime)
        {
            mainCamraa.gameObject.transform.position = new Vector3(
                Mathf.Lerp(startPosition.x, CameraPosition[currentCameraIndex].x, elapsedTime / limitCameraMoveTime),
                Mathf.Lerp(startPosition.y, CameraPosition[currentCameraIndex].y, elapsedTime / limitCameraMoveTime),
                Mathf.Lerp(startPosition.z, CameraPosition[currentCameraIndex].z, elapsedTime / limitCameraMoveTime)
                );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 補間が終わった後、目標の位置にぴったり合わせる
        mainCamraa.gameObject.transform.position = CameraPosition[currentCameraIndex];
        elapsedTime = 0.0f;
    }

    /// <summary>
    /// カメラの角度を設定します。
    /// 角度をスムーズに変化させるために、Quaternion.Slerpを使用しています。
    /// </summary>
    private IEnumerator SetCameraRotation()
    {
        float elapsedTime = 0.0f; // 経過時間

        // 現在と次の角度を取得
        Quaternion startRotation = mainCamraa.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(CameraRotation[currentCameraIndex]);

        // 制限時間まで補間を続ける
        // 1. 経過時間を追跡するための変数を初期化
        // 2. 経過時間が制限時間に達するまでループ
        // 3. 補間率 t を計算: t は 0 から 1 に向かう
        // 4. **Quaternion.Slerp（球面線形補間）**で回転をスムーズに補間
        // 5. 経過時間を更新
        while (elapsedTime < limitCameraMoveTime)
        {
            float t = elapsedTime / limitCameraMoveTime;
            mainCamraa.gameObject.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 補間が終わった後、目標のクォータニオンにぴったり合わせる
        mainCamraa.gameObject.transform.rotation = targetRotation;
    }
    /// <summary>
    /// 選択時に交換音を再生します
    /// 現在のカメラから次のカメラ、または前のカメラに切り替えます。
    /// シーンの名前も対応して更新します。
    /// </summary>
    private void SelectCamera()
    {
        _audio.PlayOneShot(_selectSE);
        currentSceneName = SceneNames[currentCameraIndex];
        StartCoroutine(SetCameraPosition());
        StartCoroutine(SetCameraRotation());
    }

    /// <summary>
    /// アニメーションから呼び出され、ステージ選択を可能にします。
    /// </summary>
    public void SetCanSelect()
    {
        canSlected = true;
    }

    /// <summary>
    /// カメラ遷移中のステージ選択を不可能にします。
    /// </summary>
    public void DisableCanSelect()
    {
        canSlected = false;
    }

    /// <summary>
    /// タイトル画面が開始されたことを設定します。
    /// </summary>
    public void SetIsStarted()
    {
        isStarted = true;
    }

    /// <summary>
    /// シーンを非同期で読み込みます。
    /// </summary>
    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(currentSceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
