using System;

namespace LikeAVersion
{
    public class HumanFeedback
    {
        public static void ToHuman(string output)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " : " + output);
        }

        public static void WriteMenu()
        {
            HumanFeedback.ToHuman("q = quit, a = touch all, w = list watched, i = list ignored");
        }
    }
}