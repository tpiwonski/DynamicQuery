using NUnit.Framework;
using System;
using MySql.Data.MySqlClient;
using Dapper;
using DynamicQuery;
using System.Collections.Generic;
using System.Dynamic;
using System.Data;

namespace DynamicQuery.MySQL
{
	[TestFixture()]
	public class Test
	{
		private IDbConnection _connection;
		
		[TestFixtureSetUp]
		public void SetUp() {
			MySqlConnectionStringBuilder connectionString = new MySqlConnectionStringBuilder();
			connectionString.Server = "localhost";
			connectionString.Port = 3306;
			connectionString.UserID = "root";
			connectionString.Password = "test";
			connectionString.Database = "test";
			_connection = new MySqlConnection(connectionString.GetConnectionString(true));
			_connection.Open();
		}

		[TestFixtureTearDown]
		public void TearDown() {
			_connection.Close();
		}

		[Test()]
		public void TestCase ()
		{
			dynamic dq = new DynamicQuery(); 
			Clause clause = dq.Insert(dq.Table.test).Values(new Dictionary<Column, object> {
				{ dq.Table.test.name, Parameter.Bind("name") }
			});
			Statement stmt = clause.Compile(new Dialect());

			var parameters = new Dictionary<string, object>();
			parameters["name"] = "tomek";

			_connection.Execute(stmt.Sql, parameters);
		}
	}
}

