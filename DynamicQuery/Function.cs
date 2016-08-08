using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;


namespace DynamicQuery {
	
    public class FunctionFactory : DynamicObject {
        
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = new Function(binder.Name.ToLower(), args);
            return true;
        }
        
    }

    public class Function : DynamicObject {

        private string _name;
        private dynamic[] _args;

		public string Name {
			get { return _name; }
		}

		public dynamic[] Args {
			get { return _args; }
		}

        public Function(string name, dynamic[] args) {
            _name = name;
            _args = args;
        }
        
		public Statement Compile(dynamic dialect) {
			return dialect.CompileFunction(this);
		}
    }

}