using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicQuery {
	
    public class BinaryOperator : DynamicObject {

		private static Dictionary<ExpressionType, string> Operators = new Dictionary<ExpressionType, string> {
			{ ExpressionType.Equal, "=" },	
			{ ExpressionType.NotEqual, "<>" },
			{ ExpressionType.GreaterThan, ">" },
			{ ExpressionType.LessThan, "<" },
			{ ExpressionType.And, "and" },
			{ ExpressionType.Or, "or" }
		};

        private string _name;
        private dynamic _arg1;
        private dynamic _arg2;

		public string Name {
			get { return _name; }
		}

		public dynamic Arg1 {
			get { return _arg1; }
		}

		public dynamic Arg2 {
			get { return _arg2; }
		}

		public static BinaryOperator Create(object arg1, ExpressionType expression, object arg2) {
			string op;
			try {
				op = BinaryOperator.Operators[expression];
			}
			catch(KeyNotFoundException) {
				throw new NotImplementedException(string.Format ("Binary operator {0} not implamented", expression));
			}
			return new BinaryOperator(op, arg1, arg2);
		}

        public BinaryOperator(string name, dynamic arg1, dynamic arg2) {
            _name = name;
            _arg1 = arg1;
            _arg2 = arg2;
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
			return dialect.CompileBinaryPredicate(this);
		}
    }

}