using System;
using MySql.Data.MySqlClient;

public class DBConn {
    //private string dbname;
    public DBConn() {

    }

    public MySqlConnection GetConn() {
        string connString = 
            "server=127.0.0.1;user id = kg0;database=claytons_index";
        return new MySqlConnection(connString);
    }
}