// PlayerController.cs
//
// プレイヤーの移動・ジャンプ・射撃処理を行います
//

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 3;            // 移動速度
    [SerializeField] private float _jumpSpeed = 7;      　// ジャンプ力
    [SerializeField] private float _gravity = 15f;        // 落下速度
    [SerializeField] private float _fallSpeed = 10f;      // 最大落下速度
    [SerializeField] private float _initFallSpeed = 2f;   // 着地時の初期落下速度

    private Transform _transform;
    private CharacterController _characterController;

    private Vector2 _inputMove;
    private float _verticalVelocity;
    private float _turnVelocity;
    private bool _isGroundedPrev;

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
