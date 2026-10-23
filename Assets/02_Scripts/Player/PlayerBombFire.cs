using UnityEngine;
using UnityEngine.Pool;

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

    private ObjectPool<Bomb> _bombPool;


    private void Awake()
    {
        _currentAmmo = MaxAmmo;
        _throwPower = _minThrowPower;
        _chargeSpeed = (_maxThrowPower - _minThrowPower) / _chargeTime;

        // 폭탄 오브젝트 풀 생성
        _bombPool = new ObjectPool<Bomb>(CreateBomb, GetBomb, ReleaseBomb, DestroyBomb, true, MaxAmmo / 2, MaxAmmo * 2);
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
            return;

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

            // 2. Instantiate 대신 풀에서 수류탄을 가져온다.
            Bomb bomb = _bombPool.Get();
            bomb.transform.SetPositionAndRotation(_firePoint.position, Quaternion.identity);

            // 3. 생성된 수류탄을 시선 방향으로 던지기
            Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
            rigidbody.AddForce(Camera.main.transform.forward * _throwPower);

            _throwPower = _minThrowPower;
        }
    }

    // 풀이 비어 있을 때 새로운 폭탄을 생성한다.
    private Bomb CreateBomb()
    {
        Bomb bomb = Instantiate(_bombPrefab);

        // 폭탄이 충돌했을 때 어느 풀로 돌아가야 하는지 알려준다.
        bomb.SetPool(_bombPool);

        return bomb;
    }

    // 풀에서 폭탄을 꺼낼 때 실행된다.
    private void GetBomb(Bomb bomb)
    {
        bomb.ResetBomb();
        bomb.gameObject.SetActive(true);
    }

    // 폭탄을 풀에 반납할 때 실행된다.
    private void ReleaseBomb(Bomb bomb)
    {
        bomb.gameObject.SetActive(false);
    }

    // 풀의 최대 개수를 넘어간 폭탄을 제거할 때 실행된다.
    private void DestroyBomb(Bomb bomb)
    {
        Destroy(bomb.gameObject);
    }
}