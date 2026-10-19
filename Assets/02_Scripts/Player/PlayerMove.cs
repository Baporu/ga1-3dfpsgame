using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동하고 싶다.
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;


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

        // 3. 방향과 속력에 따라 이동한다.
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }
}