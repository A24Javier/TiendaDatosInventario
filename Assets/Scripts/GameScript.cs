using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScript : MonoBehaviour
{
    [SerializeField] private TMP_Text textDoingNothin;
    [SerializeField] private TMP_Text usernameText;

    private const string TEXT_NOTHING_PT1 = "You spend ";
    private const string TEXT_NOTHING_PT2 = " seconds doing nothing";

    private const string TEXT_USERNAME = "Username: ";

    private float timeDoingNothin;

    void Start()
    {
        //usernameText.SetText(TEXT_USERNAME + UserData.Instance.GetUsername());
    }

    void Update()
    {
        timeDoingNothin += Time.deltaTime;
        textDoingNothin.SetText(TEXT_NOTHING_PT1 + timeDoingNothin.ToString("0") + TEXT_NOTHING_PT2);
    }

    public void LogOff()
    {
        /*UserData.Instance.SetUsername("");
        UserData.Instance.SetPassword("");*/

        SceneManager.LoadScene(0);
    }
}
