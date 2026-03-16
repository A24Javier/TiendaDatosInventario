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
    public Button SellButton;
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
                        DatabaseConnection.Instance.SaveCoins(playerCoins, false);
                        DatabaseConnection.Instance.AddApple();
                    }
                });

                item.SellButton.onClick.AddListener(delegate
                {
                    int playerCoins = DatabaseConnection.Instance.GetCoins();

                    if (InventoryManager.Instance.HasObject(1))
                    {
                        playerCoins += price;
                        ClickerSystem.Instance.UpdateTextActualCoins(playerCoins);
                        DatabaseConnection.Instance.SaveCoins(playerCoins, false);
                        DatabaseConnection.Instance.RemoveApple();
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
                        DatabaseConnection.Instance.SaveCoins(playerCoins, false);
                        DatabaseConnection.Instance.AddGold();
                    }
                });

                item.SellButton.onClick.AddListener(delegate
                {
                    int playerCoins = DatabaseConnection.Instance.GetCoins();

                    if (InventoryManager.Instance.HasObject(2))
                    {
                        playerCoins += price;
                        ClickerSystem.Instance.UpdateTextActualCoins(playerCoins);
                        DatabaseConnection.Instance.SaveCoins(playerCoins, false);
                        DatabaseConnection.Instance.RemoveGold();
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
                        DatabaseConnection.Instance.SaveCoins(playerCoins, false);
                        DatabaseConnection.Instance.AddPotion();
                    }
                });

                item.SellButton.onClick.AddListener(delegate
                {
                    int playerCoins = DatabaseConnection.Instance.GetCoins();

                    if (InventoryManager.Instance.HasObject(3))
                    {
                        playerCoins += price;
                        ClickerSystem.Instance.UpdateTextActualCoins(playerCoins);
                        DatabaseConnection.Instance.SaveCoins(playerCoins, false);
                        DatabaseConnection.Instance.RemovePotion();
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
