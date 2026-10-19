using UnityEngine;

// 역할: 마우스를 움직이면 카메라를 그 방향으로 회전하고 싶다.
public class CameraRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 200f;

    private float _accumulatedX = 0f; // 좌우 회전 각도
    private float _accumulatedY = 0f; // 상하 회전 각도


    private void Update()
    {
        // 1. 마우스 입력을 받는다.
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 2. 마우스 입력만큼 회전 값을 누적한다.
        // (카메라를 아래로 기울여야 위를 바라보므로 Y값에는 -1을 곱해준다.)
        _accumulatedX += mouseX * _rotationSpeed * Time.deltaTime;
        _accumulatedY += -mouseY * _rotationSpeed * Time.deltaTime;

        // 3. 누적하기 전에 내 변수에서 먼저 -90 ~ 90도로 제한
        _accumulatedY = Mathf.Clamp(_accumulatedY, -90f, 90f);

        // 4. 누적한 회전 값을 카메라에 적용한다.
        // (회전 적용 시에는 Pitch, Yaw, Roll 순서)
        transform.eulerAngles = new Vector3(_accumulatedY, _accumulatedX, 0f);
    }
}