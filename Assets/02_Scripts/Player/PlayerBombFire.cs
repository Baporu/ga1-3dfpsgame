using UnityEngine;

// 마우스 오른쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
public class PlayerBombFire : MonoBehaviour
{
    private const int MaxAmmo = 10;

    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;
    [SerializeField] private float _minThrowPower = 200f;
    [SerializeField] private float _maxThrowPower = 1000f;
    [SerializeField] private float _chargeTime = 5f;

    private int _currentAmmo;
    private bool _isCharging;
    private float _throwPower;
    private float _chargeSpeed;


    private void Awake()
    {
        _currentAmmo = MaxAmmo;
        _throwPower = _minThrowPower;
        _chargeSpeed = (_maxThrowPower - _minThrowPower) / _chargeTime;
    }

    private void Update()
    {
        // 1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonDown("Fire2") && _currentAmmo > 0)
        {
            _currentAmmo--;

            _isCharging = true;
        }

        if (Input.GetButton("Fire2") && _isCharging)
        {
            _throwPower += _chargeSpeed * Time.deltaTime;
            _throwPower = Mathf.Min(_throwPower, _maxThrowPower);
        }

        if (Input.GetButtonUp("Fire2") && _isCharging)
        {
            _isCharging = false;

            // 2. 발사 위치에서 수류탄 생성하기
            Bomb bomb = Instantiate(_bombPrefab, _firePoint.position, Quaternion.identity);

            // 3. 생성된 수류탄을 시선 방향으로 던지기
            Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
            rigidbody.AddForce(Camera.main.transform.forward * _throwPower);

            _throwPower = _minThrowPower;
        }
    }
}