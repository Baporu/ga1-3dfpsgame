using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;
    private const int MaxJumps = 2;
    private const float MaxStamina = 100f;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _sprintSpeed = 12f;

    [Header("Acceleration & Deceleration")]
    // 가속과 감속:
    // 목표 속도까지 즉시 변경하지 않고 서서히 변화시킨다.
    // 출발과 정지가 자연스러워지며 이동에 무게감을 줄 수 있다.
    public float GroundAcceleration = 30f;
    public float GroundDeceleration = 40f;

    [Header("Air Control")]
    // 공중 제어:
    // 공중에서는 지상과 다른 가속도와 감속도를 적용한다.
    // 값을 낮출수록 점프 중 이동 방향을 바꾸기 어려워져 관성이 강하게 느껴진다.
    public float AirAcceleration = 10f;
    public float AirDeceleration = 5f;

    [Header("Jump")]
    [SerializeField] private float _jumpPower = 10f;

    [Header("Jump Feel")]
    // 코요테 타임:
    // 발판에서 떨어진 직후에도 잠깐 점프할 수 있도록 허용한다.
    // 점프 타이밍을 조금 놓쳐도 입력이 받아들여져 조작이 관대하게 느껴진다.
    public float CoyoteTime = 0.15f;

    // 점프 버퍼:
    // 착지하기 직전에 누른 점프 입력을 잠시 기억한다.
    // 착지 순간 자동으로 점프하여 연속 점프가 부드러워진다.
    public float JumpBufferTime = 0.15f;

    // 가변 점프 높이:
    // 상승 중 점프 키를 놓으면 상승 속도를 감소시킨다.
    // 짧게 누르면 낮게, 길게 누르면 높게 점프할 수 있다.
    [Range(0f, 1f)] public float JumpCutMultiplier = 0.5f;

    [Header("Fast Fall")]
    // 빠른 낙하:
    // 하강 중에는 중력을 더 강하게 적용한다.
    // 공중에 오래 떠 있는 느낌을 줄이고 착지까지의 속도감을 높인다.
    public float FallGravityMultiplier = 2f;

    [Header("Stamina")]
    [SerializeField] private float _staminaRecovery = 15f;
    [SerializeField] private float _sprintCost = 10f;
    [SerializeField] private float _jumpCost = 20f;

    [SerializeField] private CharacterController _characterController;

    private Vector3 _horizontalVelocity;
    private float _yVelocity;

    private int _jumpCount;
    private float _stamina;

    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (_characterController == null)
        {
            Debug.LogError("[PlayerMove] 플레이어에게 CharacterController 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        _stamina = MaxStamina;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // 0. 스태미나 회복
        _stamina = Mathf.Min(MaxStamina, _stamina + _staminaRecovery * deltaTime);

        // 1. 키보드 입력을 받는다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, 0f, v);
        direction.Normalize();
        direction = transform.TransformDirection(direction);

        // 수평 이동만 계산하므로 Y축 성분은 제거한다.
        direction.y = 0f;
        direction.Normalize();

        bool isGrounded = _characterController.isGrounded;

        // 2. 점프 버퍼
        // 점프 입력을 잠시 저장하여 착지 직전 입력도 점프로 연결한다.
        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = JumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= deltaTime;
        }

        // 3. 중력과 코요테 타임
        if (isGrounded == false)
        {
            // 빠른 낙하:
            // 상승 중에는 기본 중력을 적용하고,
            // 하강 중에는 중력에 배율을 곱해 더 빠르게 떨어지도록 한다.
            // 점프의 상승은 여유롭게, 하강은 경쾌하게 만들어 조작감을 높인다.
            float gravityMultiplier = _yVelocity < 0f ? FallGravityMultiplier : 1f;

            _yVelocity += Gravity * gravityMultiplier * deltaTime;

            // 코요테 타임:
            // 발판에서 떨어진 뒤에도 일정 시간 동안 지상 점프를 허용한다.
            _coyoteTimeCounter -= deltaTime;

            if (_coyoteTimeCounter <= 0f && _jumpCount == 0)
            {
                _jumpCount = 1;
            }
        }
        else
        {
            _yVelocity = Gravity * deltaTime;
            _jumpCount = 0;

            _coyoteTimeCounter = CoyoteTime;
        }

        // 4. 점프 처리
        bool canGroundJump = _jumpCount == 0 && _coyoteTimeCounter > 0f;
        bool canAirJump = _jumpCount > 0 && _jumpCount < MaxJumps;

        bool canJump = canGroundJump || canAirJump;

        if (_jumpBufferCounter > 0f && canJump && _stamina >= _jumpCost)
        {
            _yVelocity = _jumpPower;
            _jumpCount++;
            _stamina -= _jumpCost;

            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
        }

        // 5. 가변 점프 높이
        // 상승 중 점프 키를 놓으면 상승 속도를 줄인다.
        // 점프 키를 누른 시간에 따라 실제 점프 높이가 달라진다.
        if (Input.GetButtonUp("Jump") && _yVelocity > 0f)
        {
            _yVelocity *= JumpCutMultiplier;
        }

        // 6. 달리기 처리
        float requiredStamina = _sprintCost * deltaTime;
        bool isSprint = Input.GetButton("Sprint") && _stamina > requiredStamina;

        float currentSpeed = _moveSpeed;

        if (isSprint)
        {
            currentSpeed = _sprintSpeed;
            _stamina -= requiredStamina;
        }

        // 7. 가속과 감속
        // 입력 방향과 목표 속도를 이용해 목표 수평 속도를 계산한다.
        // 실제 속도는 목표 속도로 서서히 변화하므로 출발과 정지가 부드러워진다.
        Vector3 targetVelocity = direction * currentSpeed;

        bool hasInput = direction.sqrMagnitude > 0f;

        // 공중 제어:
        // 땅에 있을 때는 지상 가속도와 감속도를,
        // 공중에 있을 때는 공중 가속도와 감속도를 사용한다.
        // 공중 수치를 낮추면 점프 중 방향 전환이 어려워져 관성이 유지된다.
        float acceleration;

        if (isGrounded)
        {
            acceleration = hasInput ? GroundAcceleration : GroundDeceleration;
        }
        else
        {
            acceleration = hasInput ? AirAcceleration : AirDeceleration;
        }

        // 가속과 감속:
        // MoveTowards는 현재 속도를 목표 속도까지 일정한 변화량으로 이동시킨다.
        // Lerp와 달리 가속도 값이 직접 속도 변화량(단위/s²)을 결정한다.
        _horizontalVelocity = Vector3.MoveTowards(
            _horizontalVelocity,
            targetVelocity,
            acceleration * deltaTime
        );

        // 8. 최종 이동 속도를 구한다.
        Vector3 velocity = _horizontalVelocity;
        velocity.y = _yVelocity;

        // 9. 속도에 따라 이동한다.
        _characterController.Move(velocity * deltaTime);

        // 천장에 충돌해 상승이 막혔다면 상승 속도를 제거한다.
        if ((_characterController.collisionFlags & CollisionFlags.Above) != 0 && _yVelocity > 0f)
        {
            _yVelocity = 0f;
        }
    }
}