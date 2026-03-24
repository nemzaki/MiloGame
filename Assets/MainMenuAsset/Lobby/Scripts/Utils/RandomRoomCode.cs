using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomRoomCode
{
    public static string GenerateRandomCode(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
        char[] stringChars = new char[length];
        System.Random random = new System.Random();

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}
