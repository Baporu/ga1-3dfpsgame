using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _maxHealth = 100f;
    private float _health;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    [SerializeField] private UI_HitEffect _uiHitEffect;


    private void Start()
    {
        _health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_health <= 0f)
            return;

        _health -= damageInfo.Value;

        _uiHitEffect.Fade();

        if (_health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}