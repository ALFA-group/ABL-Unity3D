using UnityEngine;

namespace Utilities.Unity
{
    public static class CustomPrinter
    {
        public static void PrintLine(object toPrint)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            MonoBehaviour.print(toPrint);
#else
            System.Console.PrintLine(toPrint);
#endif
        }

        public static void PrintBreak()
        {
            PrintLine("/////////////////////////////////////////");
        }
    }
}