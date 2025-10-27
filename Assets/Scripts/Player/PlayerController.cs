// PlayerController.cs
//
// プレイヤーの移動・ジャンプ・射撃処理を行います
//

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float _speed = 3;            // 移動速度
    [SerializeField] private float _jumpSpeed = 7;      　// ジャンプ力
    [SerializeField] private float _gravity = 15f;        // 落下速度
    [SerializeField] private float _fallSpeed = 10f;      // 最大落下速度
    [SerializeField] private float _initFallSpeed = 2f;   // 着地時の初期落下速度

    [Header("視点設定")]
    [SerializeField] private Camera _mainCamera;             // メインカメラ
    [SerializeField] private float _lookSensitivity = 1.5f;  // 視点移動感度
    [SerializeField] private float _lookAngleMinY = -60f;    // 視点移動下限
    [SerializeField] private float _lookAngleMaxY = 60f;     // 視点移動上限
    [SerializeField] private float _rotationY = 0f;          // 現在の視点Y軸回転量
    [SerializeField] private float _rotationX = 0f;          // 現在の視点X軸回転量
    [SerializeField] private Transform _cameraTransform;     // カメラのTransform
    [SerializeField] private Transform Head;                 // 頭のTransform

    private Transform _transform;
    private CharacterController _characterController;

    private Vector2 _inputMove;                 // 移動入力値
    private Vector2 _inputLook;                 // 視点入力値
    private float _verticalVelocity;            // 垂直方向の速度
    private float _turnVelocity;                // 回転速度
    private bool _isGroundedPrev;               // 前フレームの接地状態

    [SerializeField] private Gun _gun;
    [SerializeField] private HitManager _hitManager;

    /// <summary>
    /// 射撃Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnShoot(InputAction.CallbackContext context)
    {
        if(context.performed && _hitManager.IsStart) _gun.Shoot(); // ボタンが押された瞬間に発射
    }

    /// <summary>
    /// 移動Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        // 入力値を保持しておく
        if(_hitManager.IsStart) _inputMove = context.ReadValue<Vector2>();
        else _inputMove = Vector2.zero;
    }

    /// <summary>
    /// 視点移動Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnLook(InputAction.CallbackContext context)
    {
        if (_hitManager.IsStart) _inputLook = context.ReadValue<Vector2>();
        else _inputLook = Vector2.zero;
    }

    /// <summary>
    /// 決定Action(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnAcsept(InputAction.CallbackContext context)
    {
        if(_hitManager.EndGame) UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    /// <summary>
    /// ジャンプAction(PlayerInput側から呼ばれる)
    /// </summary>
    /// <param name="context">InputSystemからの入力値です</param>
    public void OnJump(InputAction.CallbackContext context)
    {
        // ボタンが押された瞬間かつ着地している時だけ処理
        if ((!context.performed || !_characterController.isGrounded) || !_hitManager.IsStart) return;

        _verticalVelocity = _jumpSpeed;
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; // カーソルをロックする
        _transform = transform;
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        PlayerMove(); // プレイヤーの移動
    }

    /// <summary>
    /// カメラ操作は確実に行いたいのでLateUpdateで処理します
    /// </summary>
    private void LateUpdate()
    {
        if (_hitManager.IsStart)
        {
            // _inputLookの値を元に視点移動
            float lookX = _inputLook.x * _lookSensitivity * Time.deltaTime;
            float lookY = _inputLook.y * _lookSensitivity * Time.deltaTime;

            // Y軸を中心に視点移動量を加算・減算
            _mainCamera.transform.Rotate(Vector3.up * lookX);
            _mainCamera.transform.Rotate(Vector3.right * -lookY);

            // X軸を中心に視点移動量を加算・減算
            _rotationY -= lookY;
            _rotationX += lookX;

            // Y軸回転角度を上下の制限範囲内に収める
            _rotationY = Mathf.Clamp(_rotationY, _lookAngleMinY, _lookAngleMaxY);

            // カメラのTransformに反映
            _cameraTransform.localEulerAngles = new Vector3(_rotationY, _rotationX, 0);
            _mainCamera.transform.position = Head.position;
        }
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
            _verticalVelocity = -_initFallSpeed;
        }
        else if (!isGrounded)
        {
            _verticalVelocity -= _gravity * Time.deltaTime;
            if (_verticalVelocity < -_fallSpeed)
                _verticalVelocity = -_fallSpeed;
        }

        _isGroundedPrev = isGrounded;

        // カメラの向きに合わせて移動方向を変換
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Camera.main.transform.right;

        Vector3 moveDirection = cameraForward * _inputMove.y + cameraRight * _inputMove.x;
        moveDirection.Normalize();

        // 移動速度を適用
        Vector3 moveVelocity = moveDirection * _speed;
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
}
