using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private Image _staminaBar;


    private void Awake()
    {
        if (_playerMove == null)
        {
            GameLogger.LogError("[Stamina UI] PlayerMove 컴포넌트가 없습니다!");
        }

        if (_staminaBar != null)
            return;

        _staminaBar = GetComponent<Image>();
    }

    private void Update()
    {
        _staminaBar.fillAmount = _playerMove.Stamina / _playerMove.MaxStamina;
    }
}