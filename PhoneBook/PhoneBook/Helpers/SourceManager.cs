using PhoneBook.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneBook.Helpers
{
	public class SourceManager
	{
		// SELECT

		/// <summary>
		/// 	Pobiera listę osób
		/// </summary>
		/// <param name="start"></param>
		/// <param name="take"></param>
		/// <param name="textForSearching"></param>
		/// <returns>
		/// 	List<PersonModel>
		/// </returns>

		public static List<PersonModel> Get(int start, int take, string textForSearching = null)
		{
			var personList = new List<PersonModel>();

			using (var connection = SqlConnectionHelper.GetConnection())
			{
				var sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;

				// Bez parametru
				if (string.IsNullOrWhiteSpace(textForSearching))
				{
					sqlCommand.CommandText = "SELECT * FROM People ORDER BY ID OFFSET @Start ROWS FETCH NEXT @Take ROWS ONLY;";
				}
				// Z parametrem w postaci nazwiska osoby
				else
				{
					sqlCommand.CommandText = "SELECT * FROM People WHERE LastName Like @Search ORDER BY ID OFFSET @Start ROWS FETCH NEXT @Take ROWS ONLY;";
					AddTextForSearchingParam(sqlCommand, textForSearching);
				}

				// Dodanie Parametrów
				AddStartAndTakeParam(sqlCommand, start, take);

				// Zapytanie
				SqlDataReader data = sqlCommand.ExecuteReader();

				// Uzupełnienie listy
				while (data.HasRows && data.Read())
				{
					// Dla daty będącej null-em
					if (data["Updated"] == DBNull.Value)
					{
						personList.Add(new PersonModel(
						(int)data["ID"],
						data["FirstName"].ToString(),
						data["LastName"].ToString(),
						data["Phone"].ToString(),
						data["Email"].ToString(),
						(DateTime)data["Created"],
						null
						));
					}
					// Dla daty, która nie jest null-em
					else
					{
						personList.Add(new PersonModel(
						(int)data["ID"],
						data["FirstName"].ToString(),
						data["LastName"].ToString(),
						data["Phone"].ToString(),
						data["Email"].ToString(),
						(DateTime)data["Created"],
						(DateTime)data["Updated"]
						));
					}
				}
			}

			return personList;
		}

		// SELECT

		/// <summary>
		/// 	Pobiera ilość wpisów w bazie
		/// </summary>
		/// <param name="textForSearching"></param>
		/// <returns>
		/// 	int (ilość wpisów)
		/// </returns>

		public static int GetCount(string textForSearching = null)
		{
			int count = 0;

			using (var connection = SqlConnectionHelper.GetConnection())
			{
				var sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;

				// Bez parametru
				if (string.IsNullOrWhiteSpace(textForSearching))
				{
					sqlCommand.CommandText = "SELECT COUNT(*) FROM People;";
				}
				// Z parametrem w postaci nazwiska osoby
				else
				{
					sqlCommand.CommandText = "SELECT COUNT(*) FROM People WHERE LastName Like @Search;";
					AddTextForSearchingParam(sqlCommand, textForSearching);
				}

				// Zapytanie
				SqlDataReader data = sqlCommand.ExecuteReader();

				// Pobranie wartości
				while (data.HasRows && data.Read())
				{
					count = data.GetInt32(0);
				}
			}

			return count;
		}

		// SELECT

		/// <summary>
		/// 	Pobierz osobę po ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns>
		/// 	PersonModel / null
		/// </returns>

		public static PersonModel GetPerson(int id)
		{
			PersonModel personModel = null;

			using (var connection = SqlConnectionHelper.GetConnection())
			{
				var sqlCommand = new SqlCommand();
				sqlCommand.Connection = connection;

				// Komenda
				sqlCommand.CommandText = "SELECT * FROM People WHERE ID = @ID;";
				// Dodanie parametru ID
				AddIDParam(sqlCommand, id);

				// Zapytanie
				SqlDataReader data = sqlCommand.ExecuteReader();

				// Pobranie wartości
				data.Read();
				try
				{
					// Dla daty będącej null-em
					if (data["Updated"] == DBNull.Value)
					{
						personModel = new PersonModel(
						(int)data["ID"],
						data["FirstName"].ToString(),
						data["LastName"].ToString(),
						data["Phone"].ToString(),
						data["Email"].ToString(),
						(DateTime)data["Created"],
						null
						);
					}
					// Dla daty, która nie jest null-em
					else
					{
						personModel = new PersonModel(
						(int)data["ID"],
						data["FirstName"].ToString(),
						data["LastName"].ToString(),
						data["Phone"].ToString(),
						data["Email"].ToString(),
						(DateTime)data["Created"],
						(DateTime)data["Updated"]
						);
					}
				}
				catch (Exception)
				{
					personModel = null;
				}
			}

			return personModel;
		}

		// INSERT - TRANSAKCJA

		/// <summary>
		/// 	Dodaje nowy wpis -> Transakcja
		/// </summary>
		/// <param name="personModel"></param>

		public static void Add(PersonModel personModel)
		{
			var sqlCommand = new SqlCommand();

			// Gdy ktoś nie poda mail-a
			if (string.IsNullOrWhiteSpace(personModel.Email))
			{
				sqlCommand.CommandText = @"INSERT INTO People (FirstName, LastName, Phone, Email, Created, Updated)
					VALUES (@FirstName, @LastName, @Phone, null, GETDATE(), null); SELECT CAST(scope_identity() AS int)";
			}
			// Gdy ktoś poda mail
			else
			{
				sqlCommand.CommandText = @"INSERT INTO People (FirstName, LastName, Phone, Email, Created, Updated)
					VALUES (@FirstName, @LastName, @Phone, @Email, GETDATE(), null); SELECT CAST(scope_identity() AS int)";
				// Dodawanie mail-a
				AddEmailParam(sqlCommand, personModel);
			}

			// Dodawanie pozostałych parametrów
			AddNamesAndPhoneParam(sqlCommand, personModel);

			// Transakcja
			SqlTransactionTool.Transaction(sqlCommand);
		}

		// UPDATE - TRANSAKCJA

		/// <summary>
		/// 	Edytuje wpis -> Transakcja
		/// </summary>
		/// <param name="personModel"></param>

		public static void Edit(PersonModel personModel)
		{
			var sqlCommand = new SqlCommand();

			// Gdy ktoś nie poda mail-a
			if (string.IsNullOrWhiteSpace(personModel.Email))
			{
				sqlCommand.CommandText = @"UPDATE People SET FirstName = @FirstName, LastName = @LastName, Phone = @Phone, Email = null, Updated = GETDATE()
					 WHERE ID = @ID;";
			}
			// Gdy ktoś poda mail
			else
			{
				sqlCommand.CommandText = @"UPDATE People SET FirstName = @FirstName, LastName = @LastName, Phone = @Phone, Email = @Email, Updated = GETDATE()
					 WHERE ID = @ID;";
				// Dodawanie mail-a
				AddEmailParam(sqlCommand, personModel);
			}

			// Dodawanie pozostałych parametrów
			AddNamesAndPhoneParam(sqlCommand, personModel);
			// Dodanie parametru ID
			AddIDParam(sqlCommand, personModel.ID);

			// Transakcja
			SqlTransactionTool.Transaction(sqlCommand);
		}

		// DELETE - TRANSAKCJA

		/// <summary>
		/// 	Usuń wpis -> Transakcja
		/// </summary>
		/// <param name="personModel"></param>

		public static void Remove(PersonModel personModel)
		{
			var sqlCommand = new SqlCommand();

			// Komenda
			sqlCommand.CommandText = @"DELETE FROM People WHERE ID = @ID;";

			// Dodanie parametru ID
			AddIDParam(sqlCommand, personModel.ID);

			// Transakcja
			SqlTransactionTool.Transaction(sqlCommand);
		}

		////////////////////////////////////////////////////////////
		// Prywatne metody //
		////////////////////////////////////////////////////////////

		private static void AddNamesAndPhoneParam(SqlCommand sqlCommand, PersonModel personModel)
		{
			var sqlFirstNameParam = new SqlParameter { DbType = System.Data.DbType.AnsiString, Value = personModel.FirstName, ParameterName = "@FirstName" };
			sqlCommand.Parameters.Add(sqlFirstNameParam);

			var sqlLastNameParam = new SqlParameter { DbType = System.Data.DbType.AnsiString, Value = personModel.LastName, ParameterName = "@LastName" };
			sqlCommand.Parameters.Add(sqlLastNameParam);

			var sqlPhoneParam = new SqlParameter { DbType = System.Data.DbType.AnsiString, Value = personModel.Phone, ParameterName = "@Phone" };
			sqlCommand.Parameters.Add(sqlPhoneParam);
		}

		private static void AddEmailParam(SqlCommand sqlCommand, PersonModel personModel)
		{
			var sqlEmailParam = new SqlParameter { DbType = System.Data.DbType.AnsiString, Value = personModel.Email, ParameterName = "@Email" };
			sqlCommand.Parameters.Add(sqlEmailParam);
		}

		private static void AddStartAndTakeParam(SqlCommand sqlCommand, int start, int take)
		{
			SqlParameter sqlStartParam = new SqlParameter { DbType = System.Data.DbType.Int32, Value = (start - 1) * take, ParameterName = "@Start" };
			sqlCommand.Parameters.Add(sqlStartParam);

			SqlParameter sqlTakeParam = new SqlParameter { DbType = System.Data.DbType.Int32, Value = take, ParameterName = "@Take" };
			sqlCommand.Parameters.Add(sqlTakeParam);
		}

		private static void AddTextForSearchingParam(SqlCommand sqlCommand, string textForSearching)
		{
			SqlParameter sqlSearchParam = new SqlParameter { DbType = System.Data.DbType.AnsiString, Value = $"%{textForSearching}%", ParameterName = "@Search" };
			sqlCommand.Parameters.Add(sqlSearchParam);
		}

		private static void AddIDParam(SqlCommand sqlCommand, int id)
		{
			SqlParameter sqlIDParam = new SqlParameter { DbType = System.Data.DbType.Int32, Value = id, ParameterName = "@ID" };
			sqlCommand.Parameters.Add(sqlIDParam);
		}
	}
}
