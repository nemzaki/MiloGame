using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NUnit.Framework;

public class ElementsPopUpAnimation : MonoBehaviour
{
    public List<GameObject> elements = new List<GameObject>(); // Assign your image elements in the Inspector
    public float delayBetweenPopups = 0.3f; // Time between each popup
    public float popupDuration = 0.5f; // Duration of each popup animation
    public Ease popupEase = Ease.OutBounce; // Bouncy effect

    void OnEnable()
    {
        // Start the popup animation
        DisableObj();
        SortList();
        StartCoroutine(PlayPopupSequence());
    }

    //Add disable game objects lower in list
    private void SortList()
    {
        elements.Sort((a, b) =>
        {
            var aActive = a.activeSelf;
            var bActive = b.activeSelf;

            // Disabled GameObjects should come after enabled ones
            if (aActive && !bActive) return -1;
            if (!aActive && bActive) return 1;
            return 0;
        });

    }
    
    public void PopUp()
    {
        DisableObj();
        StartCoroutine(PlayPopupSequence());
    }

    private void DisableObj()
    {
        foreach (var obj in elements)
        {
            obj.transform.localScale = Vector3.zero;
        }
    }
    private IEnumerator PlayPopupSequence()
    {
        yield return new WaitForEndOfFrame();
        
        foreach (var obj in elements)
        {
            // Reset scale to (0, 0, 0)
            obj.transform.localScale = Vector3.zero;

            // Animate the scale to (1, 1, 1) with a bouncy effect
            obj.transform.DOScale(Vector3.one, popupDuration).SetEase(popupEase);

            // Wait for the delay before animating the next image
            yield return new WaitForSeconds(delayBetweenPopups);
        }
    }
}
