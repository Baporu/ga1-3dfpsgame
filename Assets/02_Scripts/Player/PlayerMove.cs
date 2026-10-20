using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동하고 싶다.
public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;
    private const int MaxJumps = 2;
    private const float MaxStamina = 100f;

    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _sprintSpeed = 12f;

    // 현재는 단순 점프만 할 것이므로 클래스 분리 없이 이 곳에 점프 작성
    [SerializeField] private float _jumpPower = 10f;

    [Header("Jump Feel")]
    // 코요테 타임: 발판에서 떨어진 직후에도 잠깐 점프를 허용한다.
    // 플레이어가 점프 타이밍을 아주 조금 놓쳐도 입력을 받아줘서 조작이 관대하게 느껴진다.
    public float coyoteTime = 0.15f;

    // 점프 버퍼: 착지하기 직전에 누른 점프 입력을 잠깐 기억한다.
    // 착지보다 조금 일찍 점프를 눌러도 즉시 다시 점프하므로 연속 점프가 부드러워진다.
    public float jumpBufferTime = 0.15f;

    // 가변 점프 높이: 상승 중 점프 키를 놓으면 현재 상승 속도를 줄인다.
    // 짧게 누르면 낮게, 길게 누르면 높게 점프할 수 있어 점프 높이를 직접 조절할 수 있다.
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f;

    [SerializeField] private float _staminaRecovery = 15f;
    [SerializeField] private float _sprintCost = 10f;
    [SerializeField] private float _jumpCost = 20f;

    [SerializeField] private CharacterController _characterController;

    private float _yVelocity;
    private int _jumpCount = 0;
    private float _stamina;

    // 코요테 타임을 계산하기 위한 남은 시간
    private float _coyoteTimeCounter;

    // 점프 버퍼를 계산하기 위한 남은 시간
    private float _jumpBufferCounter;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (_characterController == null)
        {
            Debug.LogError("[PlayerMove] 플레이어에게 CharacterController 컴포넌트가 없습니다!");
        }

        _stamina = MaxStamina;
    }

    private void Update()
    {
        // 0. 스태미나 회복
        _stamina = Mathf.Min(MaxStamina, _stamina + _staminaRecovery * Time.deltaTime);

        // 1. 키보드 입력을 받는다.
        // GetAxis를 쓰면 입력 시간에 따라 -1 ~ 1 범위로 반환된다.
        // 지금은 관성을 바라지 않으므로 -1, 0, 1을 반환하는 GetAxisRaw를 사용한다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 점프 버퍼:
        // 점프 키를 누른 순간 바로 점프할 수 없더라도 일정 시간 입력을 기억한다.
        // 착지 직전에 점프 키를 눌러도 착지하자마자 점프할 수 있어 입력 누락이 줄어든다.
        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }

        // 2. 키보드 입력에 따라 방향을 구한다.
        Vector3 direction = new Vector3(h, 0f, v);
        direction.Normalize(); // 대각선이어도 속력을 1로 바꾸기 위해
        direction = transform.TransformDirection(direction);

        // 3. 땅에 서있지 않을 때만 중력을 누적한다.
        if (_characterController.isGrounded == false)
        {
            _yVelocity += Gravity * Time.deltaTime;

            // 코요테 타임:
            // 땅에서 떨어진 뒤에도 아주 짧은 시간 동안 지상 점프 판정을 유지한다.
            // 발판 끝에서 점프를 살짝 늦게 눌렀을 때도 점프가 나가므로 조작감이 관대해진다.
            _coyoteTimeCounter -= Time.deltaTime;

            // 코요테 타임까지 지나갔다면 지상 점프 기회를 사용한 것으로 처리한다.
            // 이렇게 해야 그냥 발판에서 떨어진 뒤에도 2단 점프가 두 번 남는 것을 방지할 수 있다.
            if (_coyoteTimeCounter <= 0f && _jumpCount == 0)
            {
                _jumpCount = 1;
            }
        }
        else
        {
            _yVelocity = Gravity * Time.deltaTime;
            _jumpCount = 0;

            // 코요테 타임:
            // 땅에 닿아 있는 동안 사용할 수 있는 시간을 계속 최대값으로 갱신한다.
            _coyoteTimeCounter = coyoteTime;
        }

        // 4. 점프 버퍼에 입력이 남아있고 점프가 가능한 상태라면 점프한다.
        // 지상에서는 코요테 타임 안에 첫 번째 점프를 할 수 있고,
        // 첫 번째 점프 이후에는 기존과 동일하게 공중에서 추가 점프를 할 수 있다.
        bool canGroundJump = _jumpCount == 0 && _coyoteTimeCounter > 0f;
        bool canAirJump = _jumpCount > 0 && _jumpCount < MaxJumps;
        bool canJump = canGroundJump || canAirJump;

        if (_jumpBufferCounter > 0f && canJump && _stamina >= _jumpCost)
        {
            _yVelocity = _jumpPower;
            _jumpCount++;
            _stamina -= _jumpCost;

            // 점프 버퍼:
            // 저장해 둔 입력을 한 번 사용했으므로 제거한다.
            // 제거하지 않으면 같은 입력으로 여러 번 점프할 가능성이 있다.
            _jumpBufferCounter = 0f;

            // 코요테 타임:
            // 실제 점프를 한 뒤에는 코요테 타임을 종료한다.
            // 한 번의 지상 입력으로 다시 지상 점프 판정을 받는 것을 방지한다.
            _coyoteTimeCounter = 0f;
        }

        // 가변 점프 높이:
        // 상승 중 점프 키를 일찍 놓으면 상승 속도를 즉시 줄인다.
        // 짧게 누르면 낮은 점프, 오래 누르면 높은 점프가 되어 플레이어가 높이를 조절할 수 있다.
        if (Input.GetButtonUp("Jump") && _yVelocity > 0f)
        {
            _yVelocity *= jumpCutMultiplier;
        }

        float requiredStamina = _sprintCost * Time.deltaTime;
        bool isSprint = Input.GetButton("Sprint") && _stamina > requiredStamina;

        float currentSpeed = _moveSpeed;

        if (isSprint)
        {
            currentSpeed = _sprintSpeed;
            _stamina -= requiredStamina;
        }

        // 5. 속도를 구한다.
        Vector3 velocity = direction * currentSpeed;
        velocity.y = _yVelocity;

        // 6. 속도(방향과 속력)에 따라 이동한다.
        // transform.position += direction * _moveSpeed * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);

        if ((_characterController.collisionFlags & CollisionFlags.Above) != 0 && _yVelocity > 0f)
        {
            _yVelocity = 0f;
        }
    }
}