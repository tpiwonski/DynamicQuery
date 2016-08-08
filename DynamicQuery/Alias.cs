using System.Dynamic;

namespace DynamicQuery {

	public class Alias {

		private Table _table;
		private string _name;

		public Table Table {
			get { return _table; }
		}

		public string Name {
			get { return _name; }
		}

		public Alias(Table table, string name) {
			_table = table;
			_name = name;
		}

		public Statement Compile(Dialect dialect) {
			return dialect.CompileAlias(this);
		}
	}

}