using System;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    public float MaxHealth => _maxHealth;

    private float _health;
    public float Health => _health;


    private void Start()
    {
        _health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_health <= 0f)
            return;

        _health -= damageInfo.Value;

        if (_health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}