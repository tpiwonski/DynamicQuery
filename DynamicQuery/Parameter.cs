using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicQuery
{

	public class Parameter {

		private string _name;

		public string Name {
			get { return _name; }
		}

		public Parameter(string name) {
			_name = name;
		}

		public Statement Compile(dynamic dialect) {
			return dialect.CompileParameter(this);
		}

		public static Parameter Bind(string name) {
			return new Parameter(name);
		}
	}
}