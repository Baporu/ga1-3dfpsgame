using System;
using UnityEngine;

// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("스탯")]
    [SerializeField] private int _maxBulletCount = 30;
    [SerializeField] private float _reloadingTime = 3f;

    [SerializeField] private float _range = 100f; // 사거리
    [SerializeField] private float _damage = 10f; // 공격력
    [SerializeField] private float _fireInterval = 0.1f; // 발사 속도

    private float _fireTimer = 0f;
    private int _bulletCount;
    private bool _isReloading = false;
    private float _reloadingTimer = 0f;


    private void Awake()
    {
        Reload();
    }

    private void Update()
    {
        // 0. 발사 타이머를 지난 시간만큼 줄인다.
        _fireTimer -= Time.deltaTime;

        // 1. 마우스 왼쪽 버튼 입력 받기
        if (Input.GetButton("Fire1") && _fireTimer <= 0f && _bulletCount > 0)
        {
            _bulletCount--;
            _fireTimer = _fireInterval;
            Shoot();
        }

        // 2. R키 누르면 총알 재장전
        if (Input.GetButtonDown("Reload") && _isReloading == false)
        {
            _isReloading = true;
            _reloadingTimer = 0f;
        }

        if (_isReloading)
        {
            _reloadingTimer += Time.deltaTime;

            if (_reloadingTimer > _reloadingTime)
            {
                _isReloading = false;
                Reload();
            }
        }
    }

    private void Shoot()
    {
        // 2. 광선(Ray)를 생성하고 발사될 위치와 진행 방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 3. 광선이 부딪힌 대상의 정보를 저장할 변수 생성하기
        RaycastHit hitInfo = new RaycastHit();

        // 4. 광선을 발사하고, 만일 부딪힌 물체가 있다면 이펙트 표시하기
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);

        if (isHit)
        {
            ParticleSystem effect = Instantiate(_hitEffectPrefab);
            effect.transform.position = hitInfo.point;
            effect.transform.forward = hitInfo.normal;
            effect.Play();
        }
    }

    private void Reload()
    {
        _bulletCount = _maxBulletCount;
    }
}