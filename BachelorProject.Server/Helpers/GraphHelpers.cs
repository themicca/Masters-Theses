namespace BachelorProject.Server.Helpers
{
    public static class GraphHelpers
    {
        public const int MaxWeight = int.MaxValue;
        public const int MinWeight = int.MinValue;

        public const string ColorBaseNode = "#3498db";
        public const string ColorBaseEdge = "black";
        public const string ColorProcessing = "orange";
        public const string ColorProcessed = "blue";
        public const string ColorResult = "green";
        public const string ColorDiscarded = "red";

        public static class AlgoTypes
        {
            public const string Dijkstra = "Dijkstra";
            public const string Kruskal = "Kruskal";
            public const string Fleury = "Fleury";
            public const string EdmondsKarp = "Edmonds-Karp";
            public const string HeldKarp = "Held-Karp";
            public const string GreedyMatching = "Greedy Matching";
            public const string GreedyColoring = "Greedy Coloring";
            public const string WelshPowell = "Welsh-Powell";
        }

        public static string ColorFromHSV(double hue, double saturation, double value)
        {
            double c = value * saturation;
            double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
            double m = value - c;
            double r_prime, g_prime, b_prime;

            if (hue < 60)
            {
                r_prime = c;
                g_prime = x;
                b_prime = 0;
            }
            else if (hue < 120)
            {
                r_prime = x;
                g_prime = c;
                b_prime = 0;
            }
            else if (hue < 180)
            {
                r_prime = 0;
                g_prime = c;
                b_prime = x;
            }
            else if (hue < 240)
            {
                r_prime = 0;
                g_prime = x;
                b_prime = c;
            }
            else if (hue < 300)
            {
                r_prime = x;
                g_prime = 0;
                b_prime = c;
            }
            else
            {
                r_prime = c;
                g_prime = 0;
                b_prime = x;
            }

            int r = (int)Math.Round((r_prime + m) * 255);
            int g = (int)Math.Round((g_prime + m) * 255);
            int b = (int)Math.Round((b_prime + m) * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
