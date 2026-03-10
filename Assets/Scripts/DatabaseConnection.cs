using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.Collections.Generic;
using System.IO;
using Unity.Jobs;
using static UnityEditor.Progress;

public class DatabaseConnection : MonoBehaviour
{
    private IDbConnection _dbConnection;
    private string _dbUri;

    public static DatabaseConnection Instance;

    void Awake()
    {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(Instance);

        _dbUri = "URI=file:" + Application.dataPath + "/Database.sqlite";
        _dbConnection = new SqliteConnection(_dbUri);
    }

    void Start()
    {
        if (!File.Exists(Application.dataPath + "/Database.sqlite"))
        {
            CreateAndReadTable();
        }

        //CreateAndUseTest();
    }

    private void CreateAndReadTable()
    {
        _dbConnection.Open();

        var command = _dbConnection.CreateCommand();

        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        // ===== TABLA RAREZA =====
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS rareza (
            id_rareza INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            color TEXT NOT NULL
        );";
        command.ExecuteNonQuery();

        // ===== TABLA OBJETO =====
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS objeto (
            id_obj INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            descripcion TEXT,
            stack_limit INTEGER NOT NULL,
            id_rareza INTEGER NOT NULL,
            precio INTEGER NOT NULL,
            FOREIGN KEY (id_rareza) REFERENCES rareza(id_rareza),
            CHECK (precio > 0)
        );";
        command.ExecuteNonQuery();

        // ===== TABLA INVENTARIO =====
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS inventario (
            id_inv INTEGER PRIMARY KEY AUTOINCREMENT,
            coins INTEGER NOT NULL DEFAULT 0,
            CHECK (coins >= 0)
        );";
        command.ExecuteNonQuery();

        // ===== TABLA USUARIOS =====
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS usuarios (
            id_usu INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            password TEXT NOT NULL,
            id_inv INTEGER UNIQUE NOT NULL,
            FOREIGN KEY (id_inv) REFERENCES inventario(id_inv)
        );";
        command.ExecuteNonQuery();

        // ===== TABLA INVENTARIO_OBJETO =====
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS inventario_objeto (
            id_slot INTEGER PRIMARY KEY AUTOINCREMENT,
            id_inv INTEGER NOT NULL,
            id_obj INTEGER NOT NULL,
            cantidad INTEGER NOT NULL,
            FOREIGN KEY (id_inv) REFERENCES inventario(id_inv),
            FOREIGN KEY (id_obj) REFERENCES objeto(id_obj)
        );";
        command.ExecuteNonQuery();

        CreateRarities();
        CreateObjects();

        _dbConnection.Close();
    }

    private void CreateRarities()
    {
        var command = _dbConnection.CreateCommand();
        command.CommandText = "INSERT INTO rareza (nombre, color) VALUES ('común', 'gris');"; // id 1
        command.ExecuteNonQuery();

        command.CommandText = "INSERT INTO rareza (nombre, color) VALUES ('raro', 'azul');"; // id 2
        command.ExecuteNonQuery();

        command.CommandText = "INSERT INTO rareza (nombre, color) VALUES ('epico', 'purpura');"; // id 3
        command.ExecuteNonQuery();

        command.CommandText = "INSERT INTO rareza (nombre, color) VALUES ('legendario', 'dorado');"; // id 4
        command.ExecuteNonQuery();
    }

    private void CreateObjects()
    {
        var command = _dbConnection.CreateCommand();
        command.CommandText = "INSERT INTO objeto (nombre, descripcion, stack_limit, id_rareza, precio) VALUES ('Manzana', 'Fruta comestible que es redonda y sale de los manzanos', 5, 1, 2)";
        command.ExecuteNonQuery();

        command.CommandText = "INSERT INTO objeto (nombre, descripcion, stack_limit, id_rareza, precio) VALUES ('Lingote', 'Cosa que todo el mundo adora', 1, 4, 15);";
        command.ExecuteNonQuery();

        command.CommandText = "INSERT INTO objeto (nombre, descripcion, stack_limit, id_rareza, precio) VALUES ('Poción crecepelo', '¡Si te lo echas encima de crecera el pelo!', 3, 3, 5);";
        command.ExecuteNonQuery();
    }

    public bool UserExists(string usernameInp)
    {
        _dbConnection.Open();

        bool exists = false;

        var command = _dbConnection.CreateCommand();

        command.CommandText = "SELECT nombre, password FROM usuarios";
        IDataReader dataReader = command.ExecuteReader();

        while (dataReader.Read())
        {
            string username = dataReader.GetString(0);

            if (usernameInp == username)
            {
                exists = true;
                break;
            }
        }

        dataReader.Close();
        _dbConnection.Close();

        return exists;
    }

    public bool CheckLogin(string usernameInp, string passwordInp, out int idInv , out string warningText)
    {
        _dbConnection.Open();
        bool isCorrect = false;

        var command = _dbConnection.CreateCommand();

        command.CommandText = "SELECT nombre, password, id_inv FROM usuarios";
        IDataReader dataReader = command.ExecuteReader();

        warningText = "Incorrect username and password";
        idInv = 1;

        while (dataReader.Read())
        {
            string username = dataReader.GetString(0);
            string password = dataReader.GetString(1);
            int id_inv = dataReader.GetInt32(2);

            if(username == usernameInp)
            {
                warningText = "Incorrect password";

                if (password == passwordInp)
                {
                    idInv = id_inv;
                    isCorrect = true;
                }
            }
        }

        dataReader.Close();
        _dbConnection.Close();

        return isCorrect;
    }

    public void CreateUser(string username, string password)
    {
        _dbConnection.Open();

        var command = _dbConnection.CreateCommand();

        command.CommandText = "INSERT INTO inventario DEFAULT VALUES;";
        command.ExecuteNonQuery();

        command.CommandText = "SELECT last_insert_rowid();";
        long idInventario = (long)command.ExecuteScalar();

        command.CommandText = $"INSERT INTO usuarios (nombre, password, id_inv) VALUES ('{username}', '{password}', {idInventario});";
        command.ExecuteNonQuery();

        _dbConnection.Close();
    }

    public void AddObject(int idObjeto, int cantidad)
    {
        _dbConnection.Open();

        var command = _dbConnection.CreateCommand();

        // Stack limit
        command.CommandText =
            "SELECT stack_limit FROM objeto WHERE id_obj = " + idObjeto + ";";
        int stackLimit = System.Convert.ToInt32(command.ExecuteScalar());

        int idInventario = UserData.Instance.IdInv;

        int restante = cantidad;

        while (restante > 0)
        {
            int idSlot = -1;
            int cantidadActual = 0;

            // Buscar stack no lleno
            command.CommandText = $@"
            SELECT id_slot, cantidad FROM inventario_objeto
            WHERE id_inv = {idInventario}
            AND id_obj = {idObjeto}
            AND cantidad < {stackLimit}
            LIMIT 1;";

            IDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                idSlot = reader.GetInt32(0);
                cantidadActual = reader.GetInt32(1);
            }
            reader.Close();

            if (idSlot != -1)
            {
                int espacio = stackLimit - cantidadActual;
                int aMeter = Mathf.Min(espacio, restante);

                command.CommandText = $@"
                UPDATE inventario_objeto
                SET cantidad = cantidad + {aMeter}
                WHERE id_slot = {idSlot};";

                command.ExecuteNonQuery();
                restante -= aMeter;
            }
            else
            {
                int aCrear = Mathf.Min(stackLimit, restante);

                command.CommandText =
                    $"INSERT INTO inventario_objeto (id_inv, id_obj, cantidad) " +
                    $"VALUES ({idInventario}, {idObjeto}, {aCrear});";

                command.ExecuteNonQuery();
                restante -= aCrear;
            }
        }

        _dbConnection.Close();
    }

    public void RemoveObject(int idObjeto)
    {
        _dbConnection.Open();
        var command = _dbConnection.CreateCommand();

        int idInventario = UserData.Instance.IdInv;

        // Buscar el último slot del objeto
        command.CommandText = $@"
        SELECT id_slot, cantidad
        FROM inventario_objeto
        WHERE id_inv = {idInventario}
        AND id_obj = {idObjeto}
        ORDER BY id_slot DESC
        LIMIT 1;
        ";

        IDataReader reader = command.ExecuteReader();

        int slotId = -1;
        int cantidadActual = 0;

        if (reader.Read())
        {
            slotId = reader.GetInt32(0);
            cantidadActual = reader.GetInt32(1);
        }

        reader.Close();

        if (slotId != -1)
        {
            if (cantidadActual > 1)
            {
                // Restar 1
                command.CommandText = $@"
                UPDATE inventario_objeto
                SET cantidad = cantidad - 1
                WHERE id_slot = {slotId};
                ";
                command.ExecuteNonQuery();
            }
            else
            {
                // Si solo queda 1 -> eliminar slot
                command.CommandText = $@"
                DELETE FROM inventario_objeto
                WHERE id_slot = {slotId};
                ";
                command.ExecuteNonQuery();
            }
        }

        _dbConnection.Close();
    }


    #region AddItemsButtons
    public void AddApple()
    {
        AddObject(1, 1);
        InventoryManager.Instance.UpdateUserInventory();
    }

    public void RemoveApple()
    {
        RemoveObject(1);
        InventoryManager.Instance.UpdateUserInventory();
    }

    public void AddGold()
    {
        AddObject(2, 1);
        InventoryManager.Instance.UpdateUserInventory();
    }

    public void RemoveGold()
    {
        RemoveObject(2);
        InventoryManager.Instance.UpdateUserInventory();
    }

    public void AddPotion()
    {
        AddObject(3, 1);
        InventoryManager.Instance.UpdateUserInventory();
    }

    public void RemovePotion()
    {
        RemoveObject(3);
        InventoryManager.Instance.UpdateUserInventory();
    }
    #endregion

    public void CreateAndUseTest()
    {
        CreateUser("Test", "12345678");
        UserData.Instance.Username = "Test";
        UserData.Instance.Password = "12345678";
        UserData.Instance.IdInv = 1;
    }

    public SlotItem[] GetItems()
    {
        List<SlotItem> items = new List<SlotItem>();
        _dbConnection.Open();

        // Obtenemos el id del inventario actual
        int inventoryId = UserData.Instance.IdInv;

        var command = _dbConnection.CreateCommand();

        command.CommandText = $@"
        SELECT id_obj, cantidad
        FROM inventario_objeto
        WHERE id_inv = {inventoryId}";

        IDataReader reader = command.ExecuteReader();

        int slotId = 0;

        while (reader.Read())
        {
            int idObj = reader.GetInt32(0);
            int cantidad = reader.GetInt32(1);

            SlotItem item = new SlotItem(slotId, idObj, cantidad);
            items.Add(item);
            slotId++;
        }

        reader.Close();

        command.ExecuteNonQuery();

        _dbConnection.Close();
        return items.ToArray();
    }

    public Color GetItemColor(int idObj)
    {
        Color backgroundColor = Color.white;

        var command = _dbConnection.CreateCommand();
        _dbConnection.Open();

        command.CommandText = $@"
        SELECT r.color
        FROM objeto o
        JOIN rareza r ON o.id_rareza = r.id_rareza
        WHERE o.id_obj = {idObj};
        ";

        object result = command.ExecuteScalar();

        if (result != null)
        {
            string colorString = result.ToString().ToLower();

            switch (colorString)
            {
                case "gris":
                    backgroundColor = Color.gray;
                    break;

                case "azul":
                    backgroundColor = Color.blue;
                    break;

                case "purpura":
                    backgroundColor = new Color(0.5f, 0f, 0.5f);
                    break;

                case "dorado":
                    backgroundColor = new Color(1f, 0.84f, 0f);
                    break;
            }
        }

        _dbConnection.Close();
        return backgroundColor;
    }

    public int GetCoins()
    {
        int coins = 0;

        // Obtenemos el id del inventario actual
        int inventoryId = UserData.Instance.IdInv;

        var command = _dbConnection.CreateCommand();
        _dbConnection.Open();

        command.CommandText = $@"
        SELECT coins
        FROM inventario
        WHERE id_inv = {inventoryId}";

        IDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            coins = reader.GetInt32(0);
        }

        reader.Close();

        command.ExecuteNonQuery();

        _dbConnection.Close();

        return coins;
    }

    public void SaveCoins(int coins)
    {
        _dbConnection.Open();

        int idInv = UserData.Instance.IdInv;

        var command = _dbConnection.CreateCommand();
        command.CommandText = $@"
        UPDATE inventario
        SET coins = {coins}
        WHERE id_inv = {idInv}";

        command.ExecuteNonQuery();

        _dbConnection.Close();
    }

    public int GetItemPrice(string itemName)
    {
        int price = 0;

        var command = _dbConnection.CreateCommand();
        _dbConnection.Open();

        command.CommandText = $@"
        SELECT precio
        FROM objeto
        WHERE nombre = '{itemName}'";

        IDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            price = reader.GetInt32(0);
        }

        reader.Close();

        command.ExecuteNonQuery();

        _dbConnection.Close();

        return price;
    }
}
