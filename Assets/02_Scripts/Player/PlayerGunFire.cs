using System;
using UnityEngine;

// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
public class PlayerGunFire : MonoBehaviour
{
    private const int MaxAmmo = 50;

    [Header("스탯")]
    [SerializeField] private float _range = 100f; // 사거리
    [SerializeField] private float _damage = 10f; // 공격력
    [SerializeField] private float _fireInterval = 0.1f; // 발사 속도
    private float _fireTimer = 0f;

    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [SerializeField] private float _reloadTime = 1.5f;
    private float _reloadTimer = 0f;
    private int _currentAmmo = 0;
    private bool _isReloading = false;


    private void Awake()
    {
        _currentAmmo = MaxAmmo;
    }

    private void Update()
    {
        // 0. 발사 타이머를 지난 시간만큼 줄인다.
        _fireTimer -= Time.deltaTime;

        // 재장전 중에는 발사 및 추가 재장전 불가능
        if (_isReloading)
        {
            _reloadTimer -= Time.deltaTime;

            if (_reloadTimer <= 0f)
            {
                _isReloading = false;
            }
            else
            {
                return;
            }
        }

        // 재장전 키를 누르거나 탄약을 다 썼으면 재장전
        if ((_currentAmmo < MaxAmmo && Input.GetButtonDown("Reload")) || _currentAmmo <= 0)
        {
            _isReloading = true;
            Reload();
        }

        // 1. 마우스 왼쪽 버튼 입력 받기
        if (Input.GetButton("Fire1") && _fireTimer <= 0f)
        {
            _fireTimer = _fireInterval;
            Shoot();
        }
    }

    private void Shoot()
    {
        _currentAmmo--;

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
        _reloadTimer = _reloadTime;
        _currentAmmo = MaxAmmo;
    }
}