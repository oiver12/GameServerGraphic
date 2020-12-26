using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using GameServer;

public class MySQL
{

	public static MySQLSettings ConnectToMySQL(MySQLSettings mySQLSettings)
	{
		mySQLSettings.connection = new MySqlConnection(CreateConnectionString(mySQLSettings));
		return ConnectToMySQLServer(mySQLSettings);
	}

	public static MySQLSettings ConnectToMySQLServer(MySQLSettings mySQLSettings)
	{
		try
		{
			mySQLSettings.connection.Open();
			Debug.Log("Succesfully connected to MySQL Server!");
			return mySQLSettings;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			return mySQLSettings;
			throw;
		}
	}

	public static void CloseConnection(MySQLSettings mySQLSettings)
	{
		mySQLSettings.connection.Close();
	}

	private static string CreateConnectionString(MySQLSettings mySQLSettings)
	{
		var db = mySQLSettings;
		string connectionString = "SERVER=" + db.server + ";"
			+ "DATABASE=" + db.database + ";" +
			"UID=" + db.user + ";" +
			"PASSWORD=" + db.password + ";";
		return connectionString;
	}

}

public struct MySQLSettings
{
	public MySqlConnection connection;
	public string server;
	public string database;
	public string user;
	public string password;
}