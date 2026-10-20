using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동하고 싶다.
public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;

    [SerializeField] private float _moveSpeed = 5f;
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

        // 3. 중력을 누적한다.
        _yVelocity += Gravity * Time.deltaTime;

        // 4. 속도를 구한다.
        Vector3 velocity = direction * _moveSpeed;
        velocity.y = _yVelocity;

        // 4. 속도(방향과 속력)에 따라 이동한다.
        //transform.position += direction * _moveSpeed * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);
    }
}