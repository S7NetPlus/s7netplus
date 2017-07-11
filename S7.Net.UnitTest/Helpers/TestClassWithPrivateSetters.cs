using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S7.Net.UnitTest.Helpers
{
    class TestClassWithPrivateSetters : TestClass
    {
        public const int PRIVATE_SETTER_VALUE = 42;
        public const int PROTECTED_SETTER_VALUE = 1337;
        public const int INTERNAL_SETTER_VALUE = 31137;
        public const int JUST_A_GETTER_VALUE = 4711;

        public int PrivateSetterProperty
        {
            get { return PRIVATE_SETTER_VALUE; }
            private set { throw new NotSupportedException("Shouldn't access private setter"); }
        }

        public int ProtectedSetterProperty
        {
            get { return PROTECTED_SETTER_VALUE; }
            private set { throw new NotSupportedException("Shouldn't access protected setter"); }
        }

        public int InternalSetterProperty
        {
            get { return INTERNAL_SETTER_VALUE; }
            private set { throw new NotSupportedException("Shouldn't access internal setter"); }
        }

        public int JustAGetterProperty
        {
            get { return JUST_A_GETTER_VALUE; }
        }
    }
}
