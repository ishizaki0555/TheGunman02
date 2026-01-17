// PlayerController.cs
//
// プレイヤーの移動・ジャンプ・射撃処理を行います
//

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float speed = 3;                // 移動速度
    [SerializeField] private float jumpSpeed = 7;      　    // ジャンプ力
    [SerializeField] private float gravity = 15f;            // 落下速度
    [SerializeField] private float fallSpeed = 10f;          // 最大落下速度
    [SerializeField] private float initFallSpeed = 2f;       // 着地時の初期落下速度

    [Header("視点設定")]
    [SerializeField] private Camera mainCamera;              // メインカメラ
    [SerializeField] private float lookSensitivity;          // 視点移動感度
    [SerializeField] private float lookAngleMinY = -60f;     // 視点移動下限
    [SerializeField] private float lookAngleMaxY = 60f;      // 視点移動上限
    [SerializeField] private float rotationY = 0f;           // 現在の視点Y軸回転量
    [SerializeField] private float rotationX = 0f;           // 現在の視点X軸回転量
    [SerializeField] private Transform cameraTransform;      // カメラのTransform
    [SerializeField] private Transform Head;                 // 頭のTransform

    [Header("スコープ設定")]
    [SerializeField] private int normalFOV = 60;
    [SerializeField] private int scopeFOV = 20;
    [SerializeField] private float scopeLerpSpeed = 10;
    [SerializeField] private float normalSensitivity = 1f;
    [SerializeField] private float scopeSensitivity = 0.3f;
    private bool isScope = false;

    private Transform _transform;
    private CharacterController _characterController;

    private Vector2 _inputMove;                 // 移動入力値
    private Vector2 _inputLook;                 // 視点入力値
    private float _verticalVelocity;            // 垂直方向の速度
    private bool _isGroundedPrev;               // 前フレームの接地状態

    [SerializeField] private Gun _gun;
    [SerializeField] private HitManager hitManager;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; // カーソルをロックする
        _transform = transform;
        _characterController = GetComponent<CharacterController>();
        hitManager = FindAnyObjectByType<HitManager>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        cameraTransform = mainCamera.transform;
    }

    private void Update()
    {
        UpdateScope();
        PlayerMove(); // プレイヤーの移動
    }

    /// <summary>
    /// カメラ操作は確実に行いたいのでLateUpdateで処理します
    /// </summary>
    private void LateUpdate()
    {
        if (hitManager.IsStart)
        {
            // _inputLookの値を元に視点移動
            float lookX = _inputLook.x * lookSensitivity * Time.deltaTime;
            float lookY = _inputLook.y * lookSensitivity * Time.deltaTime;

            // Y軸回転角度を上下の制限範囲内に収める
            rotationY -= lookY;
            rotationY = Mathf.Clamp(rotationY, lookAngleMinY, lookAngleMaxY);
            // X軸の回転を更新
            rotationX += lookX;

            // カメラのTransformに反映
            Camera.main.transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
            Camera.main.transform.transform.position = Head.position;
        }
    }

    /// <summary>
    /// 射撃Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnShoot(InputAction.CallbackContext context)
    {
        if(context.performed && hitManager.IsStart) _gun.Shoot(); // ボタンが押された瞬間に発射
    }

    /// <summary>
    /// 移動Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        // 入力値を保持しておく
        if(hitManager.IsStart) _inputMove = context.ReadValue<Vector2>();
        else _inputMove = Vector2.zero;
    }

    /// <summary>
    /// 視点移動Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnLook(InputAction.CallbackContext context)
    {
        if (hitManager.IsStart) _inputLook = context.ReadValue<Vector2>();
        else _inputLook = Vector2.zero;
    }

    public void OnScope(InputAction.CallbackContext context)
    {
        if (!hitManager.IsStart) return;

        if (context.performed)
            isScope = true;
        else if (context.canceled)
            isScope = false;
    }

    /// <summary>
    /// 決定Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnAcsept(InputAction.CallbackContext context)
    {
        if(hitManager.EndGame) UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    /// <summary>
    /// ジャンプAction(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnJump(InputAction.CallbackContext context)
    {
        // ボタンが押された瞬間かつ着地している時だけ処理
        if ((!context.performed || !_characterController.isGrounded) || !hitManager.IsStart) return;

        _verticalVelocity = jumpSpeed;
    }

    /// <summary>
    /// 以下のプレイヤーの移動処理を行います。
    /// 前後左右の移動
    /// ジャンプ
    /// 落下処理
    /// カメラの向きに合わせた移動
    /// </summary>
    private void PlayerMove()
    {
        var isGrounded = _characterController.isGrounded;

        if (isGrounded && !_isGroundedPrev)
        {
            _verticalVelocity = -initFallSpeed;
        }
        else if (!isGrounded)
        {
            _verticalVelocity -= gravity * Time.deltaTime;
            if (_verticalVelocity < -fallSpeed)
                _verticalVelocity = -fallSpeed;
        }

        _isGroundedPrev = isGrounded;

        // カメラの向きに合わせて移動方向を変換
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Camera.main.transform.right;

        Vector3 moveDirection = cameraForward * _inputMove.y + cameraRight * _inputMove.x;
        moveDirection.Normalize();

        // 移動速度を適用
        Vector3 moveVelocity = moveDirection * speed;
        moveVelocity.y = _verticalVelocity;

        Vector3 moveDelta = moveVelocity * Time.deltaTime;
        _characterController.Move(moveDelta);

        // プレイヤーの向きを移動方向に合わせる
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, 0.1f);
        }
    }

    private void UpdateScope()
    {
        float targetFOV = isScope ? scopeFOV : normalFOV;
        float targetSensitivity = isScope ? scopeSensitivity : normalSensitivity;

        // FOVをスムーズに補間
        mainCamera.fieldOfView = Mathf.Lerp(
            mainCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * scopeLerpSpeed);

        // 感度を切り替え
        lookSensitivity = Mathf.Lerp(
            lookSensitivity,
            targetSensitivity,
            Time.deltaTime * scopeLerpSpeed);
    }
}
