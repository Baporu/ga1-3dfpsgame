using System;
using UnityEngine;

public class CameraViewSwitcher2 : MonoBehaviour
{
    private enum CameraViewType
    {
        FirstPerson,
        ThirdPerson,
        Shoulder,
        Quarter,
        TopDown,
        Side
    }

    [Serializable]
    private class CameraViewSetting
    {
        [Header("Camera Position")]
        [Tooltip("플레이어를 기준으로 한 카메라 위치 오프셋")]
        [SerializeField] private Vector3 _positionOffset;

        [Tooltip("플레이어의 회전을 카메라 위치에 반영할지 여부")]
        [SerializeField] private bool _usePlayerRotationForPosition = true;

        [Header("Camera Rotation")]
        [Tooltip("카메라가 바라볼 방향")]
        [SerializeField] private Vector3 _rotation;

        [Tooltip("카메라 회전을 플레이어 회전 기준으로 계산할지 여부")]
        [SerializeField] private bool _usePlayerRotationForRotation = true;

        public Vector3 PositionOffset => _positionOffset;
        public bool UsePlayerRotationForPosition => _usePlayerRotationForPosition;

        public Vector3 Rotation => _rotation;
        public bool UsePlayerRotationForRotation => _usePlayerRotationForRotation;
    }

    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _cameraPosition;

    [Header("Camera Scripts")]
    [SerializeField] private CameraRotate _cameraRotate;
    [SerializeField] private CameraFollow _cameraFollow;

    [Header("View Settings")]
    [SerializeField] private CameraViewSetting _thirdPerson;
    [SerializeField] private CameraViewSetting _shoulder;
    [SerializeField] private CameraViewSetting _quarter;
    [SerializeField] private CameraViewSetting _topDown;
    [SerializeField] private CameraViewSetting _side;

    private CameraViewType _currentView = CameraViewType.FirstPerson;


    private void Start()
    {
        ApplyView();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ChangeView();
        }
    }

    private void LateUpdate()
    {
        // 1인칭에서는 기존 CameraFollow와 CameraRotate가
        // 카메라 위치와 회전을 담당한다.
        if (_currentView == CameraViewType.FirstPerson)
        {
            return;
        }

        CameraViewSetting setting = GetCurrentSetting();

        if (setting == null)
        {
            return;
        }

        UpdateCameraTransform(setting);
    }

    private void ApplyView()
    {
        bool isFirstPerson = _currentView == CameraViewType.FirstPerson;

        // 1인칭:
        // CameraFollow가 CameraPosition의 위치를 따라간다.
        // CameraRotate가 마우스 입력에 따라 카메라의 상하/좌우 회전을 담당한다.
        _cameraFollow.enabled = isFirstPerson;
        _cameraRotate.enabled = isFirstPerson;

        if (isFirstPerson)
        {
            // 다른 시점에서 변경된 카메라 Transform을
            // 다시 1인칭 눈 위치로 되돌린다.
            transform.position = _cameraPosition.position;
            transform.rotation = _cameraPosition.rotation;

            return;
        }

        CameraViewSetting setting = GetCurrentSetting();

        if (setting != null)
        {
            UpdateCameraTransform(setting);
        }
    }

    private void ChangeView()
    {
        int nextView = (int)_currentView + 1;

        if (nextView >= Enum.GetValues(typeof(CameraViewType)).Length)
        {
            nextView = 0;
        }

        _currentView = (CameraViewType)nextView;

        ApplyView();
    }

    private CameraViewSetting GetCurrentSetting()
    {
        switch (_currentView)
        {
            case CameraViewType.ThirdPerson:
                // 3인칭:
                // 플레이어 뒤쪽으로 카메라 위치를 이동하고
                // 캐릭터를 바라보는 방향으로 회전시킨다.
                return _thirdPerson;

            case CameraViewType.Shoulder:
                // 숄더뷰:
                // 플레이어의 뒤쪽 + 좌우 한쪽으로 카메라 위치를 이동하고
                // 어깨 너머를 바라보도록 회전시킨다.
                return _shoulder;

            case CameraViewType.Quarter:
                // 쿼터뷰:
                // 플레이어 위쪽의 대각선 위치로 카메라를 이동하고
                // 아래쪽 대각선 방향을 바라보게 한다.
                return _quarter;

            case CameraViewType.TopDown:
                // 탑다운:
                // 플레이어의 바로 위쪽으로 카메라를 이동하고
                // 지면을 향해 아래쪽을 바라보게 한다.
                return _topDown;

            case CameraViewType.Side:
                // 사이드뷰:
                // 플레이어의 측면으로 카메라를 이동하고
                // 옆에서 플레이어 방향을 바라보게 한다.
                return _side;

            default:
                return null;
        }
    }

    private void UpdateCameraTransform(CameraViewSetting setting)
    {
        Vector3 positionOffset = setting.PositionOffset;

        // 카메라 위치:
        // true라면 플레이어가 회전했을 때 카메라 위치도 함께 회전한다.
        // false라면 월드 좌표 기준의 고정된 방향에서 플레이어를 따라간다.
        if (setting.UsePlayerRotationForPosition)
        {
            positionOffset = _player.TransformDirection(positionOffset);
        }

        transform.position = _player.position + positionOffset;

        // 카메라 방향:
        // true라면 플레이어의 Y축 회전을 기준으로 카메라 방향을 계산한다.
        // false라면 월드 좌표 기준의 고정된 방향을 사용한다.
        if (setting.UsePlayerRotationForRotation)
        {
            transform.rotation =
                Quaternion.Euler(0f, _player.eulerAngles.y, 0f) *
                Quaternion.Euler(setting.Rotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(setting.Rotation);
        }
    }
}