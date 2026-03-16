using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClickerSystem : MonoBehaviour
{
    [SerializeField] private int _actualCoins = 0;
    [SerializeField] private GameObject _prefabPlusOne;
    [SerializeField] private Transform _parentCanvas;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _textActualCoins;

    public static ClickerSystem Instance;

    void Awake()
    {
        if(Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _actualCoins = DatabaseConnection.Instance.GetCoins();
        Debug.Log($"Actual coins: {_actualCoins}");
        _textActualCoins.SetText(_actualCoins.ToString("0"));
    }

    public void ClickCoin()
    {
        _actualCoins++;
        _textActualCoins.SetText(_actualCoins.ToString("0"));

        GameObject plusOne = Instantiate(_prefabPlusOne, Vector3.zero, Quaternion.identity, _parentCanvas);
        plusOne.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        Destroy(plusOne, 0.5f);
    }

    public void UpdateTextActualCoins(int coins)
    {
        _textActualCoins.SetText(coins.ToString("0"));
    }

    public void SaveCoins()
    {
        DatabaseConnection.Instance.SaveCoins(_actualCoins);
    }

    public void CloseClicker()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        SaveCoins();
    }

    public void OpenClicker()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
}
