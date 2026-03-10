using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SlotItem
{
    public int SlotId;
    public int ItemId;
    public int Cantity;

    public SlotItem() { }
    public SlotItem(int slotId, int itemId, int cantity)
    {
        SlotId = slotId;
        ItemId = itemId;
        Cantity = cantity;
    }
}

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _inventoryContent;
    [SerializeField] private Sprite[] _objectSprites;

    //Elementos UI
    [SerializeField] private Button _logOffButton;
    [SerializeField] private Button[] _addButtons;
    [SerializeField] private Button[] _abstractButtons;
    [SerializeField] private TMP_Text usernameText;

    public static InventoryManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        Instance = this;
    }

    void Start()
    {
        _logOffButton.onClick.AddListener(delegate { SceneManager.LoadScene(0); });

        for (int i = 0; i < _addButtons.Length; i++)
        {
            int index = i;

            _addButtons[i].onClick.AddListener(() =>
            {
                DatabaseConnection.Instance.AddObject(index + 1, 1);
                UpdateUserInventory();
            });
        }

        for (int i = 0; i < _abstractButtons.Length; i++)
        {
            int index = i;

            _abstractButtons[i].onClick.AddListener(() =>
            {
                DatabaseConnection.Instance.RemoveObject(index + 1);
                UpdateUserInventory();
            });
        }

        usernameText.SetText($"Username: {UserData.Instance.Username}");
        UpdateUserInventory();
    }

    public void UpdateUserInventory()
    {
        if(_inventoryContent.childCount > 0)
        {
            Transform[] childs = _inventoryContent.GetComponentsInChildren<Transform>();

            for (int i = (childs.Length-1); i > 0; i--)
            {
                Destroy(childs[i].gameObject);
            }
        }
        
        SlotItem[] slotItems = DatabaseConnection.Instance.GetItems();

        for(int i = 0; i < slotItems.Length; i++)
        {
            GameObject slotGO = Instantiate(_slotPrefab, _inventoryContent);

            Color backgroundColor = DatabaseConnection.Instance.GetItemColor(slotItems[i].ItemId);
            slotGO.transform.GetChild(0).GetComponent<Image>().color = backgroundColor;

            int objCantity = slotItems[i].Cantity;
            slotGO.transform.GetChild(3).GetComponent<TMP_Text>().text = objCantity.ToString("0");

            slotGO.transform.GetChild(1).GetComponent<Image>().sprite = _objectSprites[slotItems[i].ItemId];

            slotGO.SetActive(true);
        }
    }
}
