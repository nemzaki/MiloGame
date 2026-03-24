namespace Quantum
{
    public static partial class ByteExtensions
    {
        public static bool IsBitSet(this ref ushort flags, int bit)
        {
            return (flags & (1 << bit)) == (1 << bit);
        }

        public static ushort SetBit(this ref ushort flags, int bit, bool value)
        {
            if (value == true)
            {
                return flags |= (ushort)(1 << bit);
            }
            else
            {
                return flags &= unchecked((ushort)~(1 << bit));
            }
        }
    }
}