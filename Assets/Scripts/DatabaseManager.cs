using System.Runtime.InteropServices;
using UnityEngine;

public class DatabaseManager
{
    [DllImport("__Internal")]
    private static extern void Test();

    public static void TestJS()
    {
        if (!Application.isEditor)
            Test();
    }
}
