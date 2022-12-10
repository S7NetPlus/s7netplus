using System;

namespace S7.Net.Types
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class S7StringAttribute : Attribute
    {
        private readonly S7StringType type;
        private readonly int reservedLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="S7StringAttribute"/> class.
        /// </summary>
        /// <param name="type">The string type.</param>
        /// <param name="reservedLength">Reserved length of the string in characters.</param>
        /// <exception cref="ArgumentException">Please use a valid value for the string type</exception>
        public S7StringAttribute(S7StringType type, int reservedLength)
        {
            if (!Enum.IsDefined(typeof(S7StringType), type))
                throw new ArgumentException("Please use a valid value for the string type");

            this.type = type;
            this.reservedLength = reservedLength;
        }

        /// <summary>
        /// Gets the type of the string.
        /// </summary>
        /// <value>
        /// The string type.
        /// </value>
        public S7StringType Type => type;

        /// <summary>
        /// Gets the reserved length of the string in characters.
        /// </summary>
        /// <value>
        /// The reserved length of the string in characters.
        /// </value>
        public int ReservedLength => reservedLength;

        /// <summary>
        /// Gets the reserved length in bytes.
        /// </summary>
        /// <value>
        /// The reserved length in bytes.
        /// </value>
        public int ReservedLengthInBytes => type == S7StringType.S7String ? reservedLength + 2 : (reservedLength * 2) + 4;
    }


    /// <summary>
    /// String type.
    /// </summary>
    public enum S7StringType
    {
        /// <summary>
        /// ASCII string.
        /// </summary>
        S7String = VarType.S7String,

        /// <summary>
        /// Unicode string.
        /// </summary>
        S7WString = VarType.S7WString
    }
}
