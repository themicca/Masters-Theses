namespace BachelorProject.Server.Helpers
{
    public static class GraphHelpers
    {
        public const int MAX_WEIGHT = int.MaxValue;
        public const int MIN_WEIGHT = int.MinValue;

        public const string COLOR_BASE_NODE = "#3498db";
        public const string COLOR_BASE_EDGE = "black";
        public const string COLOR_PROCESSING = "orange";
        public const string COLOR_PROCESSED = "blue";
        public const string COLOR_RESULT = "green";
        public const string COLOR_DIRECTED = "red";

        public const double SATURATION = 0.8;
        public const double VALUE = 0.8;

        public static class AlgoTypes
        {
            public const string DIJKSTRA = "Dijkstra";
            public const string KRUSKAL = "Kruskal";
            public const string FELURY = "Fleury";
            public const string EDMONDS_KARP = "Edmonds-Karp";
            public const string HELD_KARP = "Held-Karp";
            public const string GREEDY_MATCHING = "Greedy Matching";
            public const string GREEDY_COLORING = "Greedy Coloring";
            public const string WELSH_POWELL = "Welsh-Powell";
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
