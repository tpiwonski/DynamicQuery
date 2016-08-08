using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicQuery
{
    public class DynamicQuery : DynamicObject {
        
        private TableFactory _tables;
        private FunctionFactory _functions;

        public DynamicQuery() {
            _tables = new TableFactory();
            _functions = new FunctionFactory();
        }
        
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            string clauseName = binder.Name.ToLower();
            result = new Clause(clauseName, args);
            return true;
        }

        public TableFactory Table {
            get { 
                return _tables;
            }
        }

        public FunctionFactory Function {
            get {
                return _functions;
            }
        }

		public Select Select(object[] args) {
			return new Select (args);
		}

        public Update Update(Table table) {
            return new Update(table);
        }

		public Insert Insert(Table table) {
            return new Insert(table);
        }

        public Delete Delete(Table table) {
            return new Delete(table);
        } 
    }

}