using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UI_HitEffect : MonoBehaviour
{
    [SerializeField] private float _maxAlpha = 1f;
    [SerializeField] private float _flashTime = 0.06f;
    [SerializeField] private float _fadeTime = 0.2f;

    private Image _image;


    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.enabled = false;
    }

    public void Flash(float alpha = 1f)
    {
        SetAlpha(alpha);

        StopAllCoroutines();
        StartCoroutine(Flash_Coroutine());
    }

    public void Fade(float alpha = 1f)
    {
        _maxAlpha = Mathf.Min(alpha, _maxAlpha);
        SetAlpha(_maxAlpha);

        StopAllCoroutines();
        StartCoroutine(Fade_Coroutine());
    }

    private IEnumerator Flash_Coroutine()
    {
        _image.enabled = true;

        yield return new WaitForSeconds(_flashTime);

        _image.enabled = false;
    }

    private IEnumerator Fade_Coroutine()
    {
        _image.enabled = true;

        float timer = 0f;

        while (timer < _fadeTime)
        {
            timer += Time.deltaTime;

            SetAlpha(Mathf.Lerp(_maxAlpha, 0, timer / _fadeTime));

            yield return null;
        }

        _image.enabled = false;
    }

    private void SetAlpha(float alpha)
    {
        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }
}