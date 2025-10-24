using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

public class TitleCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera titleCinameCamera; // 一番最初に優先度を上げるシネマカメラ
    [SerializeField] List<CinemachineCamera> cinemaCameras = new List<CinemachineCamera>(); // 選択画面で使うシネマカメラを管理するリスト
    [SerializeField] List<string> SceneNames = new List<string>(); // 各カメラに対応するシーン名を管理するリスト
    [SerializeField] CinemachineCamera currentCamera; // 現在選択されているカメラ
    [SerializeField] private string currentSceneName; // 現在選択されているシーン名

    private int currentCameraIndex = 0;
    private bool canSlected = false;
    private bool isStarted = false;
    private Animator _animator;

    [SerializeField] private bool isExperiment;

    private void Start()
    {
        titleCinameCamera.Priority.Value = 1;
        _animator = GetComponent<Animator>();
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
            _animator.SetTrigger("isStart");
            titleCinameCamera.Priority.Value = 0;
            cinemaCameras[currentCameraIndex].Priority.Value = 1;
            currentCamera = cinemaCameras[currentCameraIndex];
            currentSceneName = SceneNames[currentCameraIndex];
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
            _animator.SetTrigger("isRight");
            currentCameraIndex = (int)Mathf.Repeat(currentCameraIndex, cinemaCameras.Count);
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
            _animator.SetTrigger("isLeft");
            currentCameraIndex = (int)Mathf.Repeat(currentCameraIndex, cinemaCameras.Count);
            SelectCamera();
        }
    }

    /// <summary>
    /// 現在のカメラから次のカメラ、または前のカメラに切り替えます。
    /// シーンの名前も対応して更新します。
    /// </summary>
    private void SelectCamera()
    {
        currentCamera.Priority.Value = 0;
        currentCamera = cinemaCameras[currentCameraIndex];
        currentCamera.Priority.Value = 1;
        currentSceneName = SceneNames[currentCameraIndex];
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
