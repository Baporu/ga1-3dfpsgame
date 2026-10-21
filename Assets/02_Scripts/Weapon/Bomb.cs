using UnityEngine;
using UnityEngine.Pool;

// 무언가에 부딪히면 폭발 이펙트를 남기고 풀로 돌아가는 폭탄
public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;

    private IObjectPool<Bomb> _pool;
    private Rigidbody _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // 생성된 폭탄에게 자신이 돌아갈 풀을 알려준다.
    public void SetPool(IObjectPool<Bomb> pool)
    {
        _pool = pool;
    }

    // 풀에서 폭탄을 꺼낼 때 호출
    public void ResetBomb()
    {
        // 이전에 던졌던 폭탄의 물리 상태가 남아 있으면 안 되기 때문에 초기화한다.
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    // 다른 콜라이더와 부딪힌 순간 실행되는 메서드
    private void OnCollisionEnter(Collision collision)
    {
        ParticleSystem effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        effect.Play();

        // Destroy하는 대신 사용이 끝난 폭탄을 풀에 반납한다.
        _pool.Release(this);
    }
}