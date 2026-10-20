using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동하고 싶다.
public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;

    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _sprintSpeed = 12f;
    // 현재는 단순 점프만 할 것이므로 클래스 분리 없이 이 곳에 점프 작성
    [SerializeField] private float _jumpPower = 10f;
    [SerializeField] private CharacterController _characterController;

    private float _yVelocity;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (_characterController == null)
        {
            Debug.LogError("[PlayerMove] 플레이어에게 CharacterController 컴포넌트가 없습니다!");
        }
    }

    private void Update()
    {
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
            _yVelocity = 0f;
        }

        // 4. 만약 점프 키를 누르면 && 땅이라면 점프를 한다.
        // (점프 키는 Edit - Project Settings - Input Manager에서 확인 가능, 기본 키는 스페이스 바)
        // (중재자 패턴)
        if (Input.GetButtonDown("Jump") && _characterController.isGrounded)
        {
            _yVelocity = _jumpPower;
        }

        float currentSpeed = _moveSpeed;
        if (Input.GetButton("Sprint"))
        {
            currentSpeed = _sprintSpeed;
        }

        // 5. 속도를 구한다.
        Vector3 velocity = direction * currentSpeed;
        velocity.y = _yVelocity;

        // 6. 속도(방향과 속력)에 따라 이동한다.
        //transform.position += direction * _moveSpeed * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);
    }
}