using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemsShop
{
    public string ObjectName;
    public TMP_Text PriceText;
    public Button BuyButton;
}

public class ShopSystem : MonoBehaviour
{
    [SerializeField] private ItemsShop[] _shopItems;
    [SerializeField] private CanvasGroup _shopCanvasGroup;

    void Start()
    {
        int index = 0;

        foreach (ItemsShop item in _shopItems)
        {
            int price = DatabaseConnection.Instance.GetItemPrice(item.ObjectName);
            item.PriceText.SetText(price.ToString("0"));

            if(index == 0)
            {
                item.BuyButton.onClick.AddListener(delegate
                {
                    int playerCoins = DatabaseConnection.Instance.GetCoins();

                    if (playerCoins >= price)
                    {
                        playerCoins -= price;
                        ClickerSystem.Instance.UpdateTextActualCoins(playerCoins);
                        DatabaseConnection.Instance.SaveCoins(playerCoins);
                        DatabaseConnection.Instance.AddApple();
                    }
                });
            }
            else if(index == 1)
            {
                item.BuyButton.onClick.AddListener(delegate
                {
                    int playerCoins = DatabaseConnection.Instance.GetCoins();

                    if (playerCoins >= price)
                    {
                        playerCoins -= price;
                        ClickerSystem.Instance.UpdateTextActualCoins(playerCoins);
                        DatabaseConnection.Instance.SaveCoins(playerCoins);
                        DatabaseConnection.Instance.AddGold();
                    }
                });
            }
            else if(index == 2)
            {
                item.BuyButton.onClick.AddListener(delegate
                {
                    int playerCoins = DatabaseConnection.Instance.GetCoins();

                    if (playerCoins >= price)
                    {
                        playerCoins -= price;
                        ClickerSystem.Instance.UpdateTextActualCoins(playerCoins);
                        DatabaseConnection.Instance.SaveCoins(playerCoins);
                        DatabaseConnection.Instance.AddPotion();
                    }
                });
            }
            index++;
            
        }
    }

    public void CloseShop()
    {
        _shopCanvasGroup.alpha = 0f;
        _shopCanvasGroup.interactable = false;
        _shopCanvasGroup.blocksRaycasts = false;
    }

    public void OpenShop()
    {
        _shopCanvasGroup.alpha = 1f;
        _shopCanvasGroup.interactable = true;
        _shopCanvasGroup.blocksRaycasts = true;
    }
}
