using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동하고 싶다.
public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;

    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _staminaRegeneration = 20f;

    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _sprintStamina = 20f;

    // 현재는 단순 점프만 할 것이므로 클래스 분리 없이 이 곳에 점프 작성
    [SerializeField] private float _jumpPower = 10f;
    [SerializeField] private float _jumpStamina = 50f;
    [SerializeField] private int _maxJumpCount = 2;

    [SerializeField] private CharacterController _characterController;

    private bool _isJumping = false;
    private bool _isSprinting = false;
    private float _stamina;
    private float _speed;
    private float _yVelocity;
    private int _currentJumpCount = 0;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (_characterController == null)
        {
            Debug.LogError("[PlayerMove] 플레이어에게 CharacterController 컴포넌트가 없습니다!");
        }

        _stamina = _maxStamina;
        _speed = _moveSpeed;
    }

    private void Update()
    {
        // 달리고 있지 않고 && 점프 중이 아니면 스태미나를 회복한다.
        if (_isSprinting == false && _isJumping == false)
        {
            _stamina += _staminaRegeneration * Time.deltaTime;
            _stamina = Mathf.Clamp(_stamina, 0, _maxStamina);
        }

        // 1. 키보드 입력을 받는다.
        // GetAxis를 쓰면 입력 시간에 따라 -1 ~ 1 범위로 반환된다.
        // 지금은 관성을 바라지 않으므로 -1, 0, 1을 반환하는 GetAxisRaw를 사용한다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 키보드 입력에 따라 방향을 구한다.
        Vector3 direction = new Vector3(h, 0f, v);
        direction.Normalize(); // 대각선이어도 속력을 1로 바꾸기 위해
        direction = transform.TransformDirection(direction);

        // 3. 땅에 서있지 않을 때만 중력을 누적한다.
        if (_characterController.isGrounded == false)
        {
            _yVelocity += Gravity * Time.deltaTime;
        }
        else
        {
            // 땅에 서 있으면 점프 상태가 아님
            _isJumping = false;
            _yVelocity = 0f;
            _currentJumpCount = 0;
        }

        // 4. 만약 점프 키를 누르면 && 땅이라면 && 스태미나가 충분하면 점프를 한다.
        // (점프 키는 Edit - Project Settings - Input Manager에서 확인 가능, 기본 키는 스페이스 바)
        // (중재자 패턴)
        if (Input.GetButtonDown("Jump") && _currentJumpCount < _maxJumpCount && _stamina >= _jumpStamina)
        {
            // 점프 시 점프 상태 진입
            _isJumping = true;
            _stamina -= _jumpStamina;
            _yVelocity = _jumpPower;
            _currentJumpCount++;
        }

        // 달리기 키를 누르면 달리기 상태로 진입
        if (Input.GetButtonDown("Sprint"))
        {
            _isSprinting = true;
            _speed = _sprintSpeed;
        }
        // 달리기 키를 누르고 있으면 && 달리기 상태면 && 스태미나가 충분하면 달린다.
        else if (Input.GetButton("Sprint") && _isSprinting && _stamina >= _sprintStamina * Time.deltaTime)
        {
            _stamina -= _sprintStamina * Time.deltaTime;
        }
        // 달리기 키를 누르고 있지만 스태미나가 모자라면 다시 걷는다.
        else if (Input.GetButton("Sprint") && _isSprinting && _stamina < _sprintStamina * Time.deltaTime)
        {
            _speed = _moveSpeed;
        }
        // 달리기 키를 떼면 달리기 상태를 멈춘다.
        else if (Input.GetButtonUp("Sprint"))
        {
            _isSprinting = false;
            _speed = _moveSpeed;
        }

        // 5. 속도를 구한다.
        Vector3 velocity = direction * _speed;
        velocity.y = _yVelocity;

        // 6. 속도(방향과 속력)에 따라 이동한다.
        //transform.position += direction * _moveSpeed * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);
    }
}