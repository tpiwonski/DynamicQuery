using NUnit.Framework;
using System;
using System.Collections.Generic;
using DynamicQuery;
using System.Linq;

namespace DynamicQuery.Test
{
	[TestFixture ()]
	public class Test
	{
		private DynamicQuery _dynamicQuery;
		private Dialect _dialect;

		[TestFixtureSetUp]
		public void SetUp() {
			_dynamicQuery = new DynamicQuery();
			_dialect = new Dialect();
		}
		
		[Test ()]
		public void TestSelect ()
		{			
			dynamic dq = _dynamicQuery;

			dynamic stmt = dq.
				Select(dq.Table.users.name, dq.Function.ToLower(dq.Table.users.age, 100, "ala ma kota")).
				From(dq.Table.users).
				Join(dq.Table.employees).
				On(dq.Table.employees.userid == dq.Table.users.id).
				LeftJoin(dq.Table.users.Alias("alias1")).
				On(dq.Table.alias1.foo == dq.Table.users.boo).
				Where((dq.Table.employees.salary != Parameter.Bind("param1"))
			                    & !(dq.Table.employees.years == 10)
					| (dq.Table.employees.foo.between(Parameter.Bind("minValue"), Parameter.Bind("maxValue")))
					& (dq.Table.employees.boo.in_("1", 2, 3, 4)) & dq.Table.xxx.yyy.regexp("^[A-Z]+$")).
				OrderBy(dq.Table.users.name.desc, dq.Table.users.age.asc);

			Statement statement = stmt.Compile(_dialect);

			Console.WriteLine(statement.Sql);
		}

		[Test]
		public void TestInsert() {
			dynamic dq = _dynamicQuery;

			var values = new Dictionary<Column, object> {
				{ dq.Table.users.name, "tomek" },
				{ dq.Table.users.age, Parameter.Bind("age") }
			};

			Clause clause = dq.
				Insert(dq.Table.users).
				Values(values);
			
			Statement statement = clause.Compile(_dialect);

			Assert.AreEqual ("INSERT INTO users (users.name, users.age) VALUES ('tomek', @age)", 
				statement.Sql);
			Assert.AreEqual (1, statement.Parameters.Length);
			Assert.AreSame ("age", statement.Parameters [0]);
		}

		[Test]
		public void TestUpdate() {
			dynamic dq = _dynamicQuery;

			var values = new Dictionary<Column, object> {
				{ dq.Table.users.name, "jola" },
				{ dq.Table.users.age, dq.Table.users.age + 1 },
				{ dq.Table.users.city, Parameter.Bind("city") }
			};

			Clause clause = dq.
				Update(dq.Table.users).
				Set(values).
				Where(dq.Table.users.id == 123);
			
			Statement statement = clause.Compile(_dialect);

			string expectedSql = "UPDATE users SET users.name = 'jola', users.age = (users.age + 1), users.city = @city WHERE (users.id = 123)";
			Assert.AreEqual (expectedSql, statement.Sql);
			Assert.AreEqual (1, statement.Parameters.Length);
			Assert.AreSame ("city", statement.Parameters [0]);
		}

		[Test]
		public void TestDelete() {
			dynamic dq = _dynamicQuery;

			Clause clause = dq.
				Delete(dq.Table.users).
				Where(dq.Table.users.name == "tomek" & dq.Table.users.age < Parameter.Bind("age"));
			
			Statement statement = clause.Compile(_dialect);

			string expectedSql = "DELETE FROM users WHERE ((users.name = 'tomek') AND (users.age < @age))";
			Assert.AreEqual (expectedSql, statement.Sql);
			Assert.AreEqual (1, statement.Parameters.Length);
			Assert.AreSame ("age", statement.Parameters [0]);
		}
	}
}
