using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using UnityEngine;

public class SQLiteTest : MonoBehaviour
{
    IDbConnection dbConnection;

    [SerializeField] private TMP_Text timesClickedText;
    private int hitCount;

    void Start()
    {
        ReadAllValuesFromDatabase();
    }

    private void ReadAllValuesFromDatabase()
    {
        string dbUri = "URI=file:" + Application.persistentDataPath + "/MyDatabase.sqlite";

        dbConnection = new SqliteConnection(dbUri);

        dbConnection.Open();

        IDbCommand dbCommandCreateTable = dbConnection.CreateCommand();
        dbCommandCreateTable.CommandText = "CREATE TABLE IF NOT EXISTS HitCountTableSimple (id INTEGER PRIMARY KEY, hits INTEGER )";
        dbCommandCreateTable.ExecuteNonQuery();

        IDbCommand dbCommandReadValues = dbConnection.CreateCommand();
        dbCommandReadValues.CommandText = "SELECT * FROM HitCountTableSimple";

        IDataReader dataReader = dbCommandReadValues.ExecuteReader();

        while (dataReader.Read())
        {
            hitCount = dataReader.GetInt32(1); // Leemos específicamente la segunda columna como a Integer32
        }

        timesClickedText.SetText(hitCount.ToString());

        Debug.Log("Base creada");
        dbConnection.Close();
    }

    public void ButtonClicked()
    {
        hitCount++;
        timesClickedText.SetText(hitCount.ToString());

        dbConnection.Open();

        IDbCommand dbCommandInsertValue = dbConnection.CreateCommand();
        dbCommandInsertValue.CommandText = "INSERT OR REPLACE INTO HitCountTableSimple (id, hits) VALUES (0, " + hitCount + ")";
        dbCommandInsertValue.ExecuteNonQuery();
        Debug.Log("Botón pulsado");

        dbConnection.Close();
    }
}
