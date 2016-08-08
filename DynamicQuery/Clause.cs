using System.Collections.Generic;
using System.Dynamic;
using DynamicQuery;
using System.Linq;

namespace DynamicQuery {
        public class Clause : DynamicObject {

        private dynamic _parent;
        private string _name;
        private dynamic[] _args;

		public string Name {
			get { return _name; }
		}

		public dynamic[] Args {
			get { return _args; }
		}

		public dynamic Parent {
			get { return _parent; }
		}

        public Clause(string name, dynamic[] args, dynamic parent=null) {
            _parent = parent;
            _name = name;
            _args = args;
        }
	
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            string name = binder.Name.ToLower();
            result = new Clause(name, args, this);
            return true;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {            
			result = BinaryOperator.Create (this, binder.Operation, arg);
            return true;
        }        

		public virtual Statement Compile(Dialect dialect) {
			return dialect.CompileClause(this);
		}
    }

	public class Select : Clause {

		public Select(object[] args) 
			: base("select", args){
			
		}

	}

    public class Update : Clause {
		
        public Update(Table table) 
			: base("update", new [] { table }) {
            
        }

        public Clause Set(Dictionary<Column, object> values) {
			return new Set(values, this);
        }
    }

	public class Set : Clause {

		private Dictionary<Column, object> _values;

		public Dictionary<Column, object> Values {
			get { return _values; }
		}

		public Set(Dictionary<Column, object> values, dynamic parent) 
			: base("set", null, (object)parent)
		{
			_values = values;
		}

		public override Statement Compile(Dialect dialect) {
			return dialect.CompileSet(this);
		}

	}

    public class Insert : Clause {

		public Insert(Table table) 
			: base("insert into", new [] { table }) 
		{   
        }

		public Clause Values(Dictionary<Column, object> columnValues) {
			Clause columns = new Columns(columnValues.Keys.ToArray(), this);
			Clause values = new Clause("values", columnValues.Values.ToArray(), columns);
			return values;
		}
    }

	public class Columns : Clause {

		public Columns(dynamic[] columns, dynamic parent) 
			: base("", columns, (object)parent) 
		{
			
		}

		public override Statement Compile(Dialect dialect) {
			return dialect.CompileColumns(this);
		}
	}

	public class Values : Clause {

		public Values(dynamic[] values, dynamic parent) 
			: base("values", values, (object)parent) 
		{
			
		} 

		public override Statement Compile(Dialect dialect) {
			return dialect.CompileValues(this);
		}
	}

    public class Delete : Clause {

        public Delete(Table table) 
			: base("delete from", new [] { table }) {

        }
    }
}