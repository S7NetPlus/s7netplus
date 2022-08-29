using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7.Net.Types
{
    /// <summary>
    /// Allows you to specify a desired DateTime structure (DT / DTL) for S7-1500 PLC
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class S7DateTimeAttribute : Attribute
    {
        private readonly S7DateTimeType type;

        /// <summary>
        /// Allows you to specify a desired DateTime structure (DT / DTL)
        /// NOTE: S7-1200 only supports DTL natively
        /// </summary>
        /// <param name="type">Based on the struct type in PLC</param>
        /// <exception cref="ArgumentException"></exception>
        public S7DateTimeAttribute(S7DateTimeType type)
        {
            if (!Enum.IsDefined(typeof(S7DateTimeType), type))
                throw new ArgumentException("Please use a valid value for the string type");

            this.type = type;
        }

        public S7DateTimeType Type => type;

        public int ByteLength => type == S7DateTimeType.DTL ? 12 : 8;
    }

    public enum S7DateTimeType
    {
        DT = VarType.DateTime,
        DTL = VarType.DateTimeLong
    }
}
