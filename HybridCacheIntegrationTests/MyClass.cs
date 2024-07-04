using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCacheIntegrationTests
{
    internal class MyClass
    {
        public string MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }

        public override bool Equals(object obj)
        {
            MyClass other = obj as MyClass;

            if (other == null)
                return false;

            return this.MyProperty1 == other.MyProperty1 && this.MyProperty2 == other.MyProperty2;
        }
    }
}
