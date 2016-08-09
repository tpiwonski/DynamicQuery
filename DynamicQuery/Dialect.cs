using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

namespace DynamicQuery
{
	public class Dialect : DynamicObject {

		public dynamic _clause = new ExpandoObject();
		public dynamic _function = new ExpandoObject();

		public dynamic Clause {
			get { return _clause; }
		}

		public dynamic Function {
			get { return _function; }
		}

		public Dialect() {
			_clause.in_ = CreateFormatter ("IN ({0})");			
			_clause.between = ((Func<Dialect, string[], string>)((dialect, arguments) => 
				string.Format("BETWEEN {0} AND {1}", arguments[0], arguments[1])));
			_clause.orderby = CreateFormatter ("ORDER BY {0}");
			_clause.leftjoin = CreateFormatter ("LEFT JOIN {0}");
			_clause.values = CreateFormatter ("VALUES ({0})");
		}

		public static Func<Dialect, string[], string> CreateFormatter(string format) {
			return ((Func<Dialect, string[], string>)((dialect, arguments) => 
				string.Format(format, dialect.Join(arguments))));
		}

		public Statement CompileClause(Clause clause) {
			Statement parent = null;
			if (((object)clause.Parent) != null) {
				parent = clause.Parent.Compile(this);
			}

			Statement[] arguments = new Statement[] {};
			string[] parameters = null;
			if (clause.Args != null) {
				arguments = clause.Args.Select(arg => this.Compile(arg)).Cast<Statement>().ToArray();
				parameters = arguments.SelectMany(arg => arg.Parameters).ToArray();
			}

			string sql;
			string format = null;
			try {
				dynamic formatter = ((IDictionary<string, object>)_clause)[clause.Name];
				try {
					sql = formatter(this, arguments.Select(arg => arg.Sql).ToArray());
					return new Statement(sql, parameters, parent);
				}
				catch(RuntimeBinderException) {
					format = formatter.ToString();
				}
			} catch(KeyNotFoundException) {
			}

			if (clause.Args != null) {
				if (format == null) {
					format = string.Format("{0} {{0}}", clause.Name.ToUpper());
				}
				sql = string.Format(format, Join(arguments.Select(arg => arg.Sql).ToArray()));
			}
			else {
				if (format == null) {
					format = string.Format("{0}", clause.Name.ToUpper());
				}
				sql = format;
			}

			return new Statement(sql, parameters, parent);
		}

		public Statement CompileColumns(Columns clause) {
			Statement parent = null;
			if (((object)clause.Parent) != null) {
				parent = clause.Parent.Compile(this);
			}

			Statement[] columns = clause.Args.Select(arg => this.CompileColumn(arg)).Cast<Statement>().ToArray();
			string format = "({0})";
			string sql = string.Format(format, Join(columns.Select(arg => arg.Sql).ToArray()));
			return new Statement(sql, null, parent);
		}

		public Statement CompileValues(Values values) {
			return CompileClause(values);
		}

		public Statement CompileSet(Set clause) {

			Statement parent = null;
			if (((object)clause.Parent) != null) {
				parent = clause.Parent.Compile(this);
			}

			dynamic[] values = clause.Values.Select(kv => new {
				Column=CompileColumn(kv.Key),
				Value=Compile(kv.Value)
			}).ToArray();

			string[] parameters = values.SelectMany(v => v.Value.Parameters).Cast<string>().ToArray();

			string sql = string.Format("SET {0}", Join(values.Select(v => string.Format("{0} = {1}", v.Column.Sql, v.Value.Sql)).Cast<string>().ToArray()));

			return new Statement(sql, parameters, parent);
		}

		public Statement CompileColumn(Column column) {
			string sql = string.Format("{0}.{1}", column.Table.Name, column.Name);
			return new Statement(sql);
		}

		public Statement CompileFunction(Function function) {
			Statement[] arguments = new Statement[] {};
			string[] parameters = null;
			if (function.Args != null) {
				arguments = function.Args.Select(arg => this.Compile(arg)).Cast<Statement>().ToArray();
				parameters = arguments.SelectMany(arg => arg.Parameters).ToArray();
			}

			string sql;
			string format = null;
			try {
				dynamic formatter = ((IDictionary<string, object>)_function)[function.Name];
				try {
					sql = formatter(this, arguments.Select(arg => arg.Sql).ToArray());
					return new Statement(sql, parameters);
				}
				catch(RuntimeBinderException) {
					format = formatter.ToString();
				}
			}
			catch(KeyNotFoundException){
			}
				
			if (function.Args != null) {
				if (format == null) {
					format = string.Format("{0}({{0}})", function.Name.ToUpper());
				}
				sql = string.Format(format, Join(arguments.Select(arg => arg.Sql).ToArray()));
			}
			else {
				if (format == null) {
					format = string.Format("{0}()", function.Name.ToUpper());
				}
				sql = format;
			}

			return new Statement(sql, parameters);
		}

		public Statement CompileBinaryPredicate(BinaryOperator predicate) {
			Statement arg1 = this.Compile(predicate.Arg1);
			Statement arg2 = this.Compile(predicate.Arg2);

			List<string> parameters = new List<string>();
			parameters.AddRange(arg1.Parameters);
			parameters.AddRange(arg2.Parameters);

			string sql = string.Format("({0} {1} {2})", arg1.Sql, predicate.Name.ToUpper(), arg2.Sql);

			return new Statement(sql, parameters.ToArray());
		}

		public Statement CompileUnaryPredicate(UnaryOperator predicate) {
			Statement arg = this.Compile(predicate.Arg);

			string sql = string.Format("({0} {1})", predicate.Name.ToUpper(), arg.Sql);

			return new Statement(sql, arg.Parameters);
		}

		public Statement CompileParameter(Parameter parameter) {
			return new Statement(string.Format("@{0}", parameter.Name), new [] { parameter.Name });
		}

		public Statement CompileAlias(Alias alias) {
			string sql = string.Format("{0} {1}", alias.Table.Name, alias.Name);
			return new Statement(sql);
		}

		private Statement Compile(dynamic obj) {
			try {
				return obj.Compile(this);
			} catch (RuntimeBinderException) {
				if (((object)obj) == null) {
					return new Statement("NULL");
				}
				else if (obj is string) {
					return new Statement(string.Format("'{0}'", obj));
				} 
				else {
					return new Statement(obj.ToString());
				}
			}
		}

		public string Join(string[] arguments) {
			return string.Join(", ", arguments);
		}
	}
}