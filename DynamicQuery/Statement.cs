using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

namespace DynamicQuery
{

	public class Statement {

		private string _sql = string.Empty;
		private string[] _parameters = new string[]{};

		public string Sql {
			get { return _sql; }
		}

		public string[] Parameters {
			get { return _parameters; }
		}

		public Statement(string sql, string[] parameters=null, Statement parent=null) {
			List<string> p = new List<string>();

			if (parent != null) {
				_sql = parent.Sql + " ";
				p.AddRange(parent.Parameters);
			}
			_sql += sql;
			if (parameters != null) {
				p.AddRange(parameters);
			}
			_parameters = p.ToArray();
		}
	}

}