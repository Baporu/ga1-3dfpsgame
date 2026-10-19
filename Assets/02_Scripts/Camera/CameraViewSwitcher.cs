using System;
using UnityEngine;

/// <summary>
/// 시점 하나에 해당하는 카메라 설정값.
/// 인스펙터의 배열 요소 이름(_name)으로 어떤 시점인지 구분할 수 있다.
/// </summary>
[Serializable]
public class CameraViewSettings
{
    [SerializeField] private string _name;

    [Tooltip("true면 기존 CameraRotate / CameraFollow가 카메라를 제어한다 (1인칭용).\n" +
             "아래 위치/회전 값은 무시된다.")]
    [SerializeField] private bool _useExistingScripts;

    [Tooltip("Player 위치(피벗) 기준으로 얼마나 떨어진 곳에 카메라를 둘지 정한다.")]
    [SerializeField] private Vector3 _positionOffset;

    [Tooltip("카메라가 바라보는 방향 (오일러 각도). X = Pitch(좌우), Y = Yaw(상하), Z = Roll(기울기).")]
    [SerializeField] private Vector3 _rotationEuler;

    [Tooltip("true면 Player가 회전할 때 카메라도 함께 따라 돈다 (3인칭, 숄더뷰용).\n" +
             "false면 월드 기준으로 고정된다 (쿼터/탑다운/사이드용).")]
    [SerializeField] private bool _followPlayerYaw;

    public string Name => _name;
    public bool UseExistingScripts => _useExistingScripts;
    public Vector3 PositionOffset => _positionOffset;
    public Vector3 RotationEuler => _rotationEuler;
    public bool FollowPlayerYaw => _followPlayerYaw;

    public CameraViewSettings(string name, bool useExistingScripts, Vector3 positionOffset, Vector3 rotationEuler, bool followPlayerYaw)
    {
        _name = name;
        _useExistingScripts = useExistingScripts;
        _positionOffset = positionOffset;
        _rotationEuler = rotationEuler;
        _followPlayerYaw = followPlayerYaw;
    }
}

/// <summary>
/// V 키를 누를 때마다 시점을 순서대로 전환한다.
/// 1인칭 → 3인칭 → 숄더뷰 → 쿼터뷰 → 탑다운 → 사이드뷰 → (다시 1인칭)
///
/// Main Camera에 붙인다.
/// 1인칭에서는 기존 CameraRotate / CameraFollow가 카메라를 제어하고,
/// 그 외 시점에서는 두 스크립트를 꺼서 이 스크립트가 위치/회전을 직접 제어한다.
/// (두 스크립트가 같은 프레임에 transform을 덮어쓰면 마지막 쓰기만 남기 때문)
/// </summary>
public class CameraViewSwitcher : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("시점의 기준이 되는 Player (캡슐) Transform")]
    [SerializeField] private Transform _target;
    [SerializeField] private CameraRotate _cameraRotate;
    [SerializeField] private CameraFollow _cameraFollow;

    [Header("입력")]
    [SerializeField] private KeyCode _switchKey = KeyCode.V;

    [Header("전환")]
    [Tooltip("시점 전환 속도. 클수록 빠르게 도착한다. 0 이하면 즉시 전환.")]
    [SerializeField] private float _transitionSpeed = 8f;

    [Header("시점 목록 (위에서부터 V 키 순서)")]
    [SerializeField] private CameraViewSettings[] _views = new CameraViewSettings[]
    {
        // 1인칭: 기존 스크립트가 제어하므로 위치/회전 값은 쓰이지 않는다.
        new CameraViewSettings("First Person", true, Vector3.zero, Vector3.zero, true),

        // 3인칭: 플레이어 뒤쪽 위에서 살짝 내려다본다. 플레이어 회전을 따라간다.
        new CameraViewSettings("Third Person", false, new Vector3(0f, 2f, -4f), new Vector3(15f, 0f, 0f), true),

        // 숄더뷰: 플레이어 오른쪽 어깨 뒤에서 가깝게 본다. 플레이어 회전을 따라간다.
        new CameraViewSettings("Shoulder View", false, new Vector3(0.6f, 1.6f, -2f), new Vector3(5f, 0f, 0f), true),

        // 쿼터뷰: 월드 기준 대각선 위에서 45도로 내려다본다. 회전하지 않고 고정.
        new CameraViewSettings("Quarter View", false, new Vector3(0f, 8f, -8f), new Vector3(45f, 0f, 0f), false),

        // 탑다운: 플레이어 바로 위에서 수직으로 내려다본다. 회전하지 않고 고정.
        new CameraViewSettings("Top-down", false, new Vector3(0f, 12f, 0f), new Vector3(90f, 0f, 0f), false),

        // 사이드뷰: 플레이어의 옆(월드 -X 쪽)에서 +X 방향을 본다. 회전하지 않고 고정.
        new CameraViewSettings("Side View", false, new Vector3(-8f, 1.5f, 0f), new Vector3(0f, 90f, 0f), false),
    };

    private int _currentIndex;

    private void Reset()
    {
        // 컴포넌트를 처음 붙일 때 같은 오브젝트의 스크립트를 자동으로 찾는다.
        _cameraRotate = GetComponent<CameraRotate>();
        _cameraFollow = GetComponent<CameraFollow>();
    }

    private void Start()
    {
        ApplyView(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(_switchKey))
        {
            // 마지막 시점 다음에는 다시 첫 번째(1인칭)로 돌아온다.
            ApplyView((_currentIndex + 1) % _views.Length);
        }
    }

    private void LateUpdate()
    {
        CameraViewSettings view = _views[_currentIndex];

        // 1인칭은 CameraFollow가 위치를, CameraRotate가 회전을 처리한다.
        if (view.UseExistingScripts)
        {
            return;
        }

        ComputePose(view, out Vector3 targetPosition, out Quaternion targetRotation);

        if (_transitionSpeed <= 0f)
        {
            transform.SetPositionAndRotation(targetPosition, targetRotation);
            return;
        }

        // 프레임레이트와 무관한 지수 보간. 이전 시점에서 부드럽게 이동/회전한다.
        float t = 1f - Mathf.Exp(-_transitionSpeed * Time.deltaTime);
        transform.SetPositionAndRotation(
            Vector3.Lerp(transform.position, targetPosition, t),
            Quaternion.Slerp(transform.rotation, targetRotation, t));
    }

    /// <summary>
    /// 시점을 바꾸고, 기존 카메라 스크립트를 켜거나 끈다.
    /// </summary>
    private void ApplyView(int index)
    {
        _currentIndex = index;
        bool useExisting = _views[index].UseExistingScripts;

        // 1인칭에서만 켜고, 나머지 시점에서는 꺼서 transform 쓰기 충돌을 막는다.
        if (_cameraRotate != null)
        {
            _cameraRotate.enabled = useExisting;
        }

        if (_cameraFollow != null)
        {
            _cameraFollow.enabled = useExisting;
        }
    }

    /// <summary>
    /// 시점 설정으로부터 카메라가 도착해야 할 위치와 회전을 계산한다.
    /// </summary>
    private void ComputePose(CameraViewSettings view, out Vector3 position, out Quaternion rotation)
    {
        // Player의 좌우 회전(Yaw)만 뽑아서 기준 회전으로 쓴다. 고정 시점이면 회전 없음.
        Quaternion baseRotation = view.FollowPlayerYaw
                                ? Quaternion.Euler(0f, _target.eulerAngles.y, 0f)
                                : Quaternion.identity;

        // 위치: Player 위치 + (기준 회전을 적용한 오프셋)
        position = _target.position + baseRotation * view.PositionOffset;

        // 방향: 기준 회전 위에 인스펙터의 오일러 각도를 더한다.
        rotation = baseRotation * Quaternion.Euler(view.RotationEuler);
    }
}