namespace CommonLibrary.Extensions
{
    public static class UIntExtension
    {
        /// <summary>
        /// Converts the given 8-bit unsigned integer to a base-16 (hexadecimal) encoded string.
        /// </summary>
        /// <param name="i">The 8-bit unsigned integer to convert.</param>
        /// <returns>A base-16 encoded string.</returns>
        public static string ToBase16(this byte i)
        {
            return (new byte[] { i }).ToBase16();
        }

        /// <summary>
        /// Converts the given 16-bit unsigned integer to a base-16 (hexadecimal) encoded string.
        /// </summary>
        /// <param name="i">The 16-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the byte array to use for encoding.</param>
        /// <returns>A base-16 encoded string.</returns>
        public static string ToBase16(this ushort i, bool bigEndian)
        {
            return i.ToByteArray(bigEndian).ToBase16();
        }

        /// <summary>
        /// Converts the given 32-bit unsigned integer to a base-16 (hexadecimal) encoded string.
        /// </summary>
        /// <param name="i">The 32-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the byte array to use for encoding.</param>
        /// <returns>A base-16 encoded string.</returns>
        public static string ToBase16(this uint i, bool bigEndian)
        {
            return i.ToByteArray(bigEndian).ToBase16();
        }

        /// <summary>
        /// Converts the given 64-bit unsigned integer to a base-16 (hexadecimal) encoded string.
        /// </summary>
        /// <param name="i">The 64-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the byte array to use for encoding.</param>
        /// <returns>A base-16 encoded string.</returns>
        public static string ToBase16(this ulong i, bool bigEndian)
        {
            return i.ToByteArray(bigEndian).ToBase16();
        }


        /// <summary>
        /// Converts the given 16-bit unsigned integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 16-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this ushort i, bool bigEndian)
        {
            return GetBytes(i, sizeof(ushort), bigEndian);
        }

        /// <summary>
        /// Converts the given 32-bit unsigned integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 32-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this uint i, bool bigEndian)
        {
            return GetBytes(i, sizeof(uint), bigEndian);
        }

        /// <summary>
        /// Converts the given 64-bit unsigned integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 64-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this ulong i, bool bigEndian)
        {
            return GetBytes(i, sizeof(ulong), bigEndian);
        }


        /// <summary>
        /// Private function for converting unsigned integers to byte arrays.
        /// Will only work for 2, 4 and 8 byte unsigned integers.
        /// </summary>
        /// <param name="val">Unsigned integer value to convert.</param>
        /// <param name="size">Size of the integer to convert (can only be 2, 4 or 8)</param>
        /// <param name="bigEndian">Endianness of returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        private static byte[] GetBytes(ulong val, int size, bool bigEndian)
        {
            byte[] ba = new byte[size];
            if (bigEndian)
            {
                for (int i = 0, j = size - 1; i < size; i++, j--)
                {
                    ba[i] = (byte)(val >> (8 * j));
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    ba[i] = (byte)(val >> (8 * i));
                }
            }

            return ba;
        }

    }
}
