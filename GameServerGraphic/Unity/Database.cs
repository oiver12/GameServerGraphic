using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Globalization;
using GameServer;

class Database
{
	public static void NewAccount(string username, string password)
	{
		string query = "INSERT INTO account(username,password) VALUES('" +
			username +
			"','" + EncryptPassword(password) + "')";

		MySqlCommand cmd = new MySqlCommand(query, Server.mySQLSettings.connection);
		try
		{
			cmd.ExecuteNonQuery();
			Debug.Log("Account " + username + " was successfully created");
		}
		catch (Exception ex)
		{
			Debug.Log(ex);
			throw;
		}
	}

	public static bool AccountExist(string username)
	{
		string query = "SELECT username FROM account WHERE username='" + username + "'";
		MySqlCommand cmd = new MySqlCommand(query, Server.mySQLSettings.connection);
		MySqlDataReader reader = cmd.ExecuteReader();
		if (reader.HasRows)
		{
			reader.Close();
			return true;
		}
		else
		{
			reader.Close();
			return false;
		}
	}

	public static bool PasswordOK(string username, string password)
	{
		string query = "SELECT password FROM account WHERE username='" + username + "'";
		MySqlCommand cmd = new MySqlCommand(query, Server.mySQLSettings.connection);
		MySqlDataReader reader = cmd.ExecuteReader();

		string tempPass = string.Empty;
		while (reader.Read())
		{
			tempPass = reader["password"] + "";
		}
		reader.Close();

		if (EncryptPassword(password) == tempPass)
			return true;
		else
			return false;

	}

	public static string EncryptPassword(string password)
	{
		byte[] data = Encoding.ASCII.GetBytes(password);
		data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
		return Encoding.ASCII.GetString(data);
	}

	/*public static List<Troops> InizializeTroops()
	{
		string query = "SELECT * FROM unitydatabase.troops;";
		MySqlCommand cmd = new MySqlCommand(query, Server.mySQLSettings.connection);
		MySqlDataReader reader = cmd.ExecuteReader();
		List<Troops> list = new List<Troops>();
		while (reader.Read())
		{
			int number = (int)reader["Number"];
			list.Add(new Troops(
			number,
			(string)reader["Name"],
			(string)reader["Klasse"],
			(float)reader["Health"],
			(float)reader["attackSpeed"],
			(float)reader["moveSpeed"],
			(float)reader["PlaceRadius"],
			Server.troopGameObjects[number],
			(float)reader["damage"],
			(float)reader["attackRadius"],
			(int)reader["maxTroops"]));
		}
		reader.Close();
		return list;
	}*/

	/*public static List<FormationTable> InizializeFormationTable()
	{
		string query = "SELECT * FROM FormationTable";
		MySqlCommand cmd = new MySqlCommand(query, Server.mySQLSettings.connection);
		MySqlDataReader reader = cmd.ExecuteReader();
		List<FormationTable> list = new List<FormationTable>();
		while(reader.Read())
		{
			list.Add(new FormationTable(
			(int)reader["FormationA"],
			(int)reader["FormationB"],
			(float)reader["Multiplier"]));
		}
		reader.Close();
		return list;
	}*/
}
