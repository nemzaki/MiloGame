using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextPopUpAnimation : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro; // Assign in inspector or via code
    private string _lastValue;

    public Ease ease = Ease.OutBack;
    public float scale = 1.2f;
    public float duration = 0.3f;
    
    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }
    
    public void UpdateValue(string newValue)
    {
        if (newValue == _lastValue) return;

        _lastValue = newValue;
        textMeshPro.text = newValue;

        // Cancel any ongoing tweens on the transform
        transform.DOKill();

        // Animate scale bounce
        transform.localScale = Vector3.one * scale; // start slightly bigger
        transform.DOScale(Vector3.one, duration).SetEase(ease);
    }
}
