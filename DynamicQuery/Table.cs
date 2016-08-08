using System.Dynamic;

namespace DynamicQuery {
    public class Table : DynamicObject {
        
        private string _name;

		public string Name {
			get { return _name; }
		}

        public Table(string name) {
            _name = name;
        }

		public Alias Alias(string alias) {
			return new Alias(this, alias);
		}

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = new Column(binder.Name.ToLower(), this);
            return true;
        }

        public override string ToString() {
            return _name;
        }
    }

    public class TableFactory : DynamicObject {

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = new Table(binder.Name.ToLower());
            return true;
        }

    }

}