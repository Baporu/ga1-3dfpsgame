using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;
    [SerializeField] private Player _playerHealth;
    [SerializeField] private PlayerMove _playerMove;


    private void LateUpdate()
    {
        // 0 ~ 100% -> 0 ~ 1
        _healthSlider.value = _playerHealth.Health / _playerHealth.MaxHealth;
        _staminaSlider.value = _playerMove.Stamina / PlayerMove.MaxStamina;
    }
}