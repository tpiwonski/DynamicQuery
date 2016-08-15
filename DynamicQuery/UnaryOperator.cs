using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;


namespace DynamicQuery {
	
    public class UnaryOperator : DynamicObject {

		private static Dictionary<ExpressionType, string> Operators = new Dictionary<ExpressionType, string> {
			{ ExpressionType.Not, "not" }	
		};

        private string _name;
        private object _arg;

		public string Name {
			get { return _name; }
		}

		public dynamic Arg {
			get { return _arg; }
		}

		public static UnaryOperator Create(ExpressionType expression, object arg) {
			string op;
			try {
				op = UnaryOperator.Operators[expression];
			}
			catch(KeyNotFoundException) {
				throw new NotImplementedException (string.Format ("Unary operator {0} not implemented", expression));
			}
			return new UnaryOperator(op, arg);
		}

        public UnaryOperator(string name, object arg) {
            _name = name;
            _arg = arg;
        }

		public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {            
			result = BinaryOperator.Create (this, binder.Operation, arg);
			return true;
		}

		public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
			result = UnaryOperator.Create (binder.Operation, this);
			return true;
		}

		public Statement Compile(Dialect dialect) {
			return dialect.CompileUnaryPredicate(this);
		}
    }

}