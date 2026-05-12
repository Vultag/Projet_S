using UnityEngine;

public struct BitwiseUtils
{
    public static byte CompareBytes(byte a, byte b)
    {
        return (byte)(((a ^ b) - 1) >> 31 & 1);
    }
}
