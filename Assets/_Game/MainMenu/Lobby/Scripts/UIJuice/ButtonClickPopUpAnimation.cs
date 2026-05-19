using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;

public class ButtonClickPopUpAnimation : MonoBehaviour
{
    private RectTransform _rectTransform;

    public bool textField;
    
    private Button _button;
    private TMP_InputField _inputField;
    
    public float popUpDuration = 0.2f;
    public Ease popUpEase = Ease.OutBounce;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        if (textField)
        {
            _inputField = GetComponent<TMP_InputField>();
            _inputField.onSelect.AddListener(OnInputFieldEndEdit);
        }
        else
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick); 
        }
    }

    void OnButtonClick()
    {
        BounceEffect();    
    }

    void OnInputFieldEndEdit(string text)
    {
        BounceEffect();
    }
    private void BounceEffect()
    {
        _rectTransform.localScale = Vector3.one;

        // Animate the scale with a bouncy effect
        _rectTransform.DOScale(new Vector3(1.2f, 1.2f, 1), popUpDuration) // Scale up slightly
            .SetEase(popUpEase)
            .OnComplete(() =>
            {
                // Scale back to the original size
                _rectTransform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InOutQuad);
            });
    }
}
