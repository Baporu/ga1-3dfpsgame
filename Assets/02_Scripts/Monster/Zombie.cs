using System.Collections;
using UnityEngine;

// FSM(유한 상태 머신): 유한 개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// - 상태: 객체가 가질 수 있는 어떤 조건이나 상황
// - 전이: 조건에 따라 현재 상태에서 다른 상태로 넘어가는 것
// - 위 방식에 따라 규칙을 세워둔 것을 '머신'

// 좀비가 가질 수 있는 상태
public enum ZombieState
{
    Idle, // 대기: 제자리에서 플레이어를 기다린다.
    Roam, // 배회: 시작 위치 주변의 랜덤한 위치로 이동한다.
    Trace, // 추격: 플레이어를 쫓아간다.
    Comeback, // 복귀: 처음 자리로 돌아간다.
    Attack, // 공격: 일정 시간마다 플레이어를 때린다.
    Hit, // 피격: 공격(총알, 폭탄)을 받아 잠깐 멈칫한다.
    Death, // 죽음: 쓰러지고 잠시 뒤 사라진다.
}

public class Zombie : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _detectDistance = 8f; // 플레이어를 발견하는 거리
    [SerializeField] private float _comebackDistance = 15f; // 처음 자리에서 이만큼 멀어지면 돌아가는 거리
    [SerializeField] private float _attackRange = 1.5f; // 공격 범위
    [SerializeField] private float _attackInterval = 1f; // 공격 간격
    [SerializeField] private float _moveSpeed = 5f; // 이동속도
    [SerializeField] private float _maxHealth = 30f;
    private float _health;

    [Header("배회")]
    [SerializeField] private float _roamRadius = 5f; // 시작 위치를 기준으로 돌아다닐 반경
    [SerializeField] private float _minRoamInterval = 0.5f; // 배회하기까지 최소 대기 시간
    [SerializeField] private float _maxRoamInterval = 4f; // 배회하기까지 최대 대기 시간

    [Header("이펙트")]
    [SerializeField] private GameObject _bloodSprayPrefab;

    private float _attackTimer;
    private CharacterController _controller;
    private Vector3 _startPosition;
    private GameObject _player;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    private float _roamTimer;
    private float _roamInterval;
    private Vector3 _roamPosition;

    private ZombieState _state = ZombieState.Idle;


    private void Start()
    {
        _health = _maxHealth;

        _controller = GetComponent<CharacterController>();
        _startPosition = transform.position;

        // 게임을 시작하면 플레이어를 찾는다.
        _player = GameObject.FindGameObjectWithTag("Player");

        // 처음 Idle 상태에서 기다릴 시간을 랜덤하게 정한다.
        ResetRoamTimer();
    }

    private void Update()
    {
        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
                break;

            case ZombieState.Roam:
                Roam();
                break;

            case ZombieState.Trace:
                Trace();
                break;

            case ZombieState.Comeback:
                Comeback();
                break;

            case ZombieState.Attack:
                Attack();
                break;
        }
    }

    // 대미지를 받는다.
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_state == ZombieState.Death)
            return;

        _health -= damageInfo.Value;

        StopAllCoroutines();

        if (_health > 0f)
        {
            // 맞은 부위에 피 스프레이 실행하고 싶다.
            GameObject bloodSpray = Instantiate(_bloodSprayPrefab, transform);
            bloodSpray.transform.position = damageInfo.HitPoint;
            bloodSpray.transform.forward = damageInfo.HitNormal;
            Destroy(bloodSpray, 3f);

            // 잠깐 멈칫하고 싶다.
            Debug.Log($"{_state} -> Hit");
            _state = ZombieState.Hit;

            StartCoroutine(Hit_Coroutine());
        }
        else
        {
            Debug.Log($"{_state} -> Death");
            _state = ZombieState.Death;

            StartCoroutine(Death_Coroutine());
        }
    }

    // 대기: 제자리에서 플레이어를 기다린다.
    private void Idle()
    {
        // 플레이어와 가까워지면 Trace 상태로 전이한다.
        float distance = Vector3.Distance(_player.transform.position, transform.position);
        if (distance < _detectDistance)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        // 다른 상태로 전이되지 않았다면 일정 시간 뒤 주변을 배회한다.
        _roamTimer += Time.deltaTime;

        if (_roamTimer >= _roamInterval)
        {
            // 시작 위치를 기준으로 원 안의 랜덤한 위치를 정한다.
            Vector2 randomPosition = Random.insideUnitCircle * _roamRadius;

            _roamPosition = _startPosition;
            _roamPosition.x += randomPosition.x;
            _roamPosition.z += randomPosition.y;

            Debug.Log($"{_state} -> Roam");
            _state = ZombieState.Roam;
        }
    }

    // 배회: 시작 위치 주변의 랜덤한 위치로 이동한다.
    private void Roam()
    {
        // 배회 중에도 플레이어를 발견하면 추적한다.
        float playerDistance = Vector3.Distance(_player.transform.position, transform.position);
        if (playerDistance < _detectDistance)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        // 목표 위치에 도착하면 다시 기다린다.
        if (Vector3.Distance(transform.position, _roamPosition) < 0.5f)
        {
            Debug.Log($"{_state} -> Idle");
            _state = ZombieState.Idle;

            ResetRoamTimer();
            return;
        }

        Vector3 direction = _roamPosition - transform.position;
        direction.y = 0f;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }

    // 추격: 플레이어를 쫓아간다.
    private void Trace()
    {
        // Todo: Run 애니메이션 실행

        // 처음 자리에서 너무 멀어지면 돌아간다.
        if (Vector3.Distance(transform.position, _startPosition) > _comebackDistance)
        {
            Debug.Log($"{_state} -> Comeback");
            _state = ZombieState.Comeback;
            return;
        }

        // 플레이어가 공격 거리 안이면 공격한다.
        if (Vector3.Distance(transform.position, _player.transform.position) <= _attackRange)
        {
            Debug.Log($"{_state} -> Attack");
            _state = ZombieState.Attack;
            return;
        }

        Vector3 direction = _player.transform.position - transform.position;
        direction.y = 0f;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }

    // 복귀: 처음 자리로 돌아간다.
    private void Comeback()
    {
        // 처음 자리로 돌아가면 다시 기다린다.
        if (Vector3.Distance(transform.position, _startPosition) < 1f)
        {
            Debug.Log($"{_state} -> Idle");
            _state = ZombieState.Idle;

            ResetRoamTimer();
            return;
        }

        Vector3 direction = _startPosition - transform.position;
        direction.y = 0f;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }

    // 공격: 일정 시간마다 플레이어를 때린다.
    private void Attack()
    {
        // 플레이어가 공격 거리를 벗어나면 다시 추적한다.
        if (Vector3.Distance(transform.position, _player.transform.position) > _attackRange)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _attackInterval)
        {
            // Todo: 공격 애니메이션 실행
            // Todo: 플레이어 때리기
            Debug.Log("플레이어를 때렸습니다.");
            _attackTimer = 0f;
        }
    }

    private IEnumerator Hit_Coroutine()
    {
        yield return new WaitForSeconds(0.3f); // 잠깐 멈칫한다.

        // 맞았으니 쫓아온다.
        Debug.Log($"{_state} -> Trace");
        _state = ZombieState.Trace;
    }

    private IEnumerator Death_Coroutine()
    {
        _controller.enabled = false;
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    // Idle 상태에서 Roam으로 전이되기까지의 시간을 새로 정한다.
    private void ResetRoamTimer()
    {
        _roamTimer = 0f;
        _roamInterval = Random.Range(_minRoamInterval, _maxRoamInterval);
    }
}