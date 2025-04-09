namespace BachelorProject.Server.Helpers
{
    public static class Constants
    {
        public static int MaxWeight { get; private set; } = int.MaxValue;
        public static int MinWeight { get; private set; } = int.MinValue;

        public static string ColorBaseNode { get; private set; } = "#3498db";
        public static string ColorBaseEdge { get; private set; } = "black";
        public static string ColorProcessing { get; private set; } = "orange";
        public static string ColorProcessed { get; private set; } = "blue";
        public static string ColorResult { get; private set; } = "green";
        public static string ColorDiscarded { get; private set; } = "red";

        public static AlgoTypes GraphTypes { get; private set; } = new AlgoTypes
        {
            Dijkstra = "Dijkstra",
            Kruskal = "Kruskal",
            Fleury = "Fleury",
            EdmondsKarp = "Edmonds-Karp",
            HeldKarp = "Held-Karp",
            GreedyMatching = "Greedy Matching",
            GreedyColoring = "Greedy Coloring",
            WelshPowell = "Welsh-Powell"
        };
    }

    public struct AlgoTypes
    {
        public string Dijkstra;
        public string Kruskal;
        public string Fleury;
        public string EdmondsKarp;
        public string HeldKarp;
        public string GreedyMatching;
        public string GreedyColoring;
        public string WelshPowell;
    }
}
