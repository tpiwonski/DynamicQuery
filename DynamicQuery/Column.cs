using System.Dynamic;


namespace DynamicQuery {
    public class Column : DynamicObject {
        
        private string _name;
        private Table _table;

		public string Name {
			get { return _name; }
		}

		public Table Table {
			get { return _table; }
		}

        public Column(string name, Table table) {
            _name = name;
            _table = table;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = new Clause(binder.Name.ToLower(), null, this);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = new Clause(binder.Name.ToLower(), args, this);
            return true;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
			result = BinaryOperator.Create (this, binder.Operation, arg);
            return true;
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
			result = UnaryOperator.Create (binder.Operation, this);
            return true;
        }

        public static BinaryOperator operator ==(object c1, Column c2) {
            return new BinaryOperator("=", c1, c2);
        }
        
        public static BinaryOperator operator !=(object c1, Column c2) {
            return new BinaryOperator("<>", c1, c2);
        }

		public Statement Compile(dynamic dialect) {
			return dialect.CompileColumn(this);
		}
    }


}