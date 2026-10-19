using UnityEngine;

// 역할: 마우스를 움직이면 카메라를 그 방향으로 회전하고 싶다.
public class FPSCameraRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 200f;

    private float _accumulatedX = 0f; // 좌우 회전 각도
    private float _accumulatedY = 0f; // 상하 회전 각도


    private void Update()
    {
        // 1. 마우스 입력을 받는다.
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Debug.Log($"Mouse X: {mouseX}, Mouse Y: {mouseY}");

        // 2. 마우스 입력에 따라 회전할 방향을 구한다.
        //Vector3 direction = new Vector3(-mouseY, mouseX, 0);
        // 2. 마우스 입력만큼 회전 값을 누적한다.
        _accumulatedX += mouseX * _rotationSpeed * Time.deltaTime;
        _accumulatedY += -mouseY * _rotationSpeed * Time.deltaTime; // 아래로 기울여야 위를 바라보므로 Y값에는 -1을 곱해준다.

        /*
        // 3. 그 방향으로 회전시킨다.
        // 새로운 위치 = 현재 위치 + 속도 * 시간
        // 새로운 각도 = 현재 각도 + 속도 * 시간
        transform.eulerAngles += direction * _rotationSpeed * Time.deltaTime;

        // 4. 위 아래 회전값을 -90 ~ 90도로 제한한다.
        Vector3 rotation = transform.eulerAngles;

        //rotation.x = Mathf.Max(-90f, Mathf.Min(90, rotation.x));
        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

        // 정면: 0도
        // 하늘을 살짝 1도만큼 쳐다보면: -1도
        // -> eulerAngles에서는 359도로 인식 = 90도로 강제 Clamp
        // 한 줄 정리: 유니티는 0 ~ 360도 체계를 쓰기 때문에 음수는 자동으로 변환되어서 반환된다.
        transform.eulerAngles = rotation;
        */

        // 3. 누적하기 전에 내 변수에서 먼저 -90 ~ 90도로 제한
        _accumulatedY = Mathf.Clamp(_accumulatedY, -90f, 90f);

        // 4. 누적한 회전 값을 카메라에 적용한다.
        // (회전 적용 시에는 Pitch, Yaw, Roll 순서)
        transform.eulerAngles = new Vector3(_accumulatedY, _accumulatedX, 0f);
    }
}