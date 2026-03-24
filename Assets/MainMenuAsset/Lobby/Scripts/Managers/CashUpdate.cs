using TMPro;
using UnityEngine;

public class CashUpdate : MonoBehaviour
{
    public TextMeshProUGUI cashText;

    public void Update()
    {
        cashText.text = SaveDataLocal.Instance.cash.ToString();
    }
}
