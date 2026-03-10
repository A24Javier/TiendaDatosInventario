using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScript : MonoBehaviour
{
    private string usernameInp;
    private string passwordInp;

    [Header("Cosas UI")]
    [SerializeField] private CanvasGroup warningGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Button changeModeButton;

    void Start()
    {
        ChangeMode(true);
    }

    public void UsernameInpChanged(string username)
    {
        usernameInp = username;
    }

    public void PasswordInpChanged(string password)
    {
        passwordInp = password;
    }

    public void CheckLogin()
    {
        string warningText = "";
        int idInv = 1;
        bool isLoginCorrect = DatabaseConnection.Instance.CheckLogin(usernameInp, passwordInp, out idInv, out warningText);

        if (isLoginCorrect)
        {
            UserData.Instance.Username = usernameInp;
            UserData.Instance.Password = passwordInp;
            UserData.Instance.IdInv = idInv;

            SceneManager.LoadScene(1);
        }
        else
        {
            ShowWarning(warningText);
        }
    }

    private void ShowWarning(string warningText)
    {
        warningGroup.alpha = 1;
        warningGroup.interactable = true;
        warningGroup.blocksRaycasts = true;

        warningGroup.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().SetText(warningText);
    }

    public void CloseWarning()
    {
        warningGroup.alpha = 0;
        warningGroup.interactable = false;
        warningGroup.blocksRaycasts = false;
    }

    private void RegisterUser()
    {
        bool existingUser = DatabaseConnection.Instance.UserExists(usernameInp);

        /*IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT Username, Password FROM Users";

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            string username = dataReader.GetString(0);

            if (usernameInp == username)
            {
                existingUser = true;
                break;
            }
        }

        dataReader.Close();*/

        if (!existingUser)
        {
            DatabaseConnection.Instance.CreateUser(usernameInp, passwordInp);

            ShowWarning($"Registration successful");
        }
        else
        {
            ShowWarning($"User with the name {usernameInp} exists");
        }
    }

    public void OnInputFieldChanged()
    {
        actionButton.interactable = ((usernameInp.Length > 0) && (passwordInp.Length > 7));
    }

    private void ChangeMode(bool login)
    {
        actionButton.onClick.RemoveAllListeners();
        changeModeButton.onClick.RemoveAllListeners();
        changeModeButton.onClick.AddListener(delegate { ChangeMode(!login); });

        if (login)
        {
            titleText.SetText("LOGIN");
            actionButton.onClick.AddListener(delegate { CheckLogin(); });
        }
        else
        {
            titleText.SetText("REGISTER");
            actionButton.onClick.AddListener(delegate { RegisterUser(); });
        }

        actionButton.GetComponentInChildren<TMP_Text>().SetText(login ? "Login" : "Register");
        changeModeButton.GetComponentInChildren<TMP_Text>().SetText(login ? "Register" : "Login");
    }
}
