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
		private dynamic dynamicQuery;
		private Dialect dialect;

		[TestFixtureSetUp]
		public void SetUp() {
			dynamicQuery = new DynamicQuery();
			dialect = new Dialect();
		}
		
		[Test ()]
		public void TestSelect ()
		{
			Clause clause = dynamicQuery.
				Select(dynamicQuery.Table.users.id, dynamicQuery.Table.users.name, dynamicQuery.Table.users.dateOfBirth).
				From(dynamicQuery.Table.users).
				Where(dynamicQuery.Table.users.name.like(Parameter.Bind("name"))).
				OrderBy(dynamicQuery.Table.users.id.desc);

			Statement statement = clause.Compile(dialect);

			Assert.AreEqual("SELECT users.id, users.name, users.dateofbirth FROM users WHERE users.name LIKE @name ORDER BY users.id DESC",
				statement.Sql);
			Assert.AreEqual(1, statement.Parameters.Length);
			Assert.AreSame("name", statement.Parameters [0]);
		}

		[Test]
		public void TestJoins() {
			Clause clause = dynamicQuery.
				Select(dynamicQuery.Table.a.c1.As("foo"), dynamicQuery.Table.b.c1.As("boo"), dynamicQuery.Table.c.c1).
				From(dynamicQuery.Table.a).
				Join(dynamicQuery.Table.b).On(dynamicQuery.Table.b.id == dynamicQuery.Table.a.id).
				LeftJoin(dynamicQuery.Table.c).On(dynamicQuery.Table.c.id == dynamicQuery.Table.b.id);

			Statement statement = clause.Compile(dialect);
			Assert.AreEqual("SELECT a.c1 AS 'foo', b.c1 AS 'boo', c.c1 FROM a JOIN b ON (b.id = a.id) LEFT JOIN c ON (c.id = b.id)", 
				statement.Sql);
		}

		[Test]
		public void TestPredicate() {
			Clause clause = dynamicQuery.
				Where(dynamicQuery.Table.a.c1 != Parameter.Bind("param1")
	                & (!(dynamicQuery.Table.a.c2 == 10)
						| dynamicQuery.Table.a.c3.between(Parameter.Bind("minValue"), Parameter.Bind("maxValue")))
	                & dynamicQuery.Table.a.c4.in_("1", 2, 3, 4)
	                & dynamicQuery.Table.a.c5.regexp("^[A-Z]+$"));

			Statement statement = clause.Compile(dialect);

			Assert.AreEqual("WHERE ((((a.c1 <> @param1) AND ((NOT (a.c2 = 10)) OR a.c3 BETWEEN @minValue AND @maxValue)) AND a.c4 IN ('1', 2, 3, 4)) AND a.c5 REGEXP '^[A-Z]+$')",
				statement.Sql);
			Assert.AreEqual(3, statement.Parameters.Length);
			Assert.AreEqual(new string[] { "param1", "minValue", "maxValue" }, statement.Parameters);
		}

		[Test]
		public void TestInsert() {

			var values = new Dictionary<Column, object> {
				{ dynamicQuery.Table.users.name, "tomek" },
				{ dynamicQuery.Table.users.age, Parameter.Bind("age") }
			};

			Clause clause = dynamicQuery.
				Insert(dynamicQuery.Table.users).
				Values(values);
			
			Statement statement = clause.Compile(dialect);

			Assert.AreEqual ("INSERT INTO users (users.name, users.age) VALUES ('tomek', @age)", 
				statement.Sql);
			Assert.AreEqual (1, statement.Parameters.Length);
			Assert.AreSame ("age", statement.Parameters [0]);
		}

		[Test]
		public void TestUpdate() {

			var values = new Dictionary<Column, object> {
				{ dynamicQuery.Table.users.name, "jola" },
				{ dynamicQuery.Table.users.age, dynamicQuery.Table.users.age + 1 },
				{ dynamicQuery.Table.users.city, Parameter.Bind("city") }
			};

			Clause clause = dynamicQuery.
				Update(dynamicQuery.Table.users).
				Set(values).
				Where(dynamicQuery.Table.users.id == 123);
			
			Statement statement = clause.Compile(dialect);

			string expectedSql = "UPDATE users SET users.name = 'jola', users.age = (users.age + 1), users.city = @city WHERE (users.id = 123)";
			Assert.AreEqual (expectedSql, statement.Sql);
			Assert.AreEqual (1, statement.Parameters.Length);
			Assert.AreSame ("city", statement.Parameters [0]);
		}

		[Test]
		public void TestDelete() {

			Clause clause = dynamicQuery.
				Delete(dynamicQuery.Table.users).
				Where(dynamicQuery.Table.users.name == "tomek" & dynamicQuery.Table.users.age < Parameter.Bind("age"));
			
			Statement statement = clause.Compile(dialect);

			string expectedSql = "DELETE FROM users WHERE ((users.name = 'tomek') AND (users.age < @age))";
			Assert.AreEqual (expectedSql, statement.Sql);
			Assert.AreEqual (1, statement.Parameters.Length);
			Assert.AreSame ("age", statement.Parameters [0]);
		}
	}
}
