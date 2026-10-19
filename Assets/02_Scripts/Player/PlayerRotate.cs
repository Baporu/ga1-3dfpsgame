using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    // Todo: Scriptable Object 또는 static class로 플레이어와 카메라의 공통 변수 빼내기
    [SerializeField] private float _rotationSpeed = 200f;

    private float _accumulatedX = 0f; // 좌우 회전 각도


    private void Update()
    {
        // 카메라 회전과 원리는 똑같지만, 플레이어 오브젝트는 상하로 회전하면 안 되므로 X값만 사용한다.
        float mouseX = Input.GetAxis("Mouse X");
        _accumulatedX += mouseX * _rotationSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0f, _accumulatedX, 0f);
    }
}