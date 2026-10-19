using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;


    private void Awake()
    {
        if (_target == null)
        {
            Debug.LogError("[CameraFollow] 카메라의 타겟이 없습니다!");
        }
    }

    private void LateUpdate()
    {
        transform.position = _target.position;
    }
}