using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.MinimumTree
{
    public class KruskalAlgo
    {
        public static GraphStepDto SolveGraph(CreateGraphRequestDto graph)
        {
            // Extract graph details
            string[] nodes = graph.GraphNodes;
            int[][] edges = graph.GraphEdges;
            bool graphDirected = graph.GraphDirected;

            // Kruskal's algorithm is for undirected graphs.
            if (graphDirected)
                throw new ArgumentException("Kruskal's algorithm requires an undirected graph.");

            int nodesCount = nodes.Length;

            // Build a list of all valid edges (only one direction since the graph is undirected)
            List<Edge> allEdges = new List<Edge>();
            for (int i = 0; i < nodesCount; i++)
            {
                for (int j = i + 1; j < nodesCount; j++)
                {
                    if (edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight)
                    {
                        allEdges.Add(new Edge(i, j, edges[i][j]));
                    }
                }
            }

            // Sort edges by increasing weight
            allEdges.Sort((e1, e2) => e1.Weight.CompareTo(e2.Weight));

            // Initialize union-find structure
            int[] parent = new int[nodesCount];
            for (int i = 0; i < nodesCount; i++)
                parent[i] = i;

            // Prepare snapshots and colorings similar to DijkstraAlgo
            var steps = new List<StepState>();
            var nodeColors = new string[nodesCount];
            for (int i = 0; i < nodesCount; i++)
                nodeColors[i] = Constants.ColorBase;

            var edgeColors = new Dictionary<string, string>();
            for (int i = 0; i < nodesCount; i++)
            {
                for (int j = i + 1; j < nodesCount; j++)
                {
                    if (edges[i][j] != 0 && edges[i][j] < Constants.MaxWeight)
                    {
                        string key = $"{i}->{j}";
                        edgeColors[key] = Constants.ColorBase;
                    }
                }
            }

            List<Edge> mstEdges = new List<Edge>();

            // Process each edge in sorted order
            foreach (var edge in allEdges)
            {
                // Mark this edge as being processed
                string edgeKey = $"{edge.From}->{edge.To}";
                edgeColors[edgeKey] = Constants.ColorProcessing;
                //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));

                int root1 = Find(parent, edge.From);
                int root2 = Find(parent, edge.To);

                // If adding the edge does not form a cycle, include it in the MST
                if (root1 != root2)
                {
                    mstEdges.Add(edge);
                    Union(parent, root1, root2);
                    edgeColors[edgeKey] = Constants.ColorProcessed;
                    //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                }
                else
                {
                    // Skip the edge to avoid cycle; mark it accordingly
                    edgeColors[edgeKey] = Constants.ColorResult;
                    //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));
                }
            }

            // Finalize node colors as part of the result
            for (int i = 0; i < nodesCount; i++)
                nodeColors[i] = Constants.ColorResult;
            //steps.Add(Snapshots.TakeSnapshot(nodes, nodeColors, edgeColors));

            // Build the MST as an adjacency matrix
            int[][] mstMatrix = new int[nodesCount][];
            for (int i = 0; i < nodesCount; i++)
                mstMatrix[i] = new int[nodesCount];

            foreach (var edge in mstEdges)
            {
                mstMatrix[edge.From][edge.To] = edge.Weight;
                mstMatrix[edge.To][edge.From] = edge.Weight;
            }

            // Create the final graph; source and target nodes are arbitrarily chosen here
            CreateGraphRequestDto finalGraph = new CreateGraphRequestDto
            {
                GraphNodes = nodes,
                GraphEdges = mstMatrix,
                GraphSrc = nodes[0],
                GraphTarget = nodes[nodesCount - 1],
                GraphDirected = false,
                GraphNodePositions = graph.GraphNodePositions
            };

            GraphStepDto stepDto = new GraphStepDto
            {
                Steps = steps,
                ResultGraph = finalGraph
            };

            return stepDto;
        }

        private static int Find(int[] parent, int i)
        {
            if (parent[i] != i)
                parent[i] = Find(parent, parent[i]);
            return parent[i];
        }

        private static void Union(int[] parent, int x, int y)
        {
            int xRoot = Find(parent, x);
            int yRoot = Find(parent, y);
            parent[xRoot] = yRoot;
        }

        // Private helper class to represent an edge
        private class Edge
        {
            public int From { get; }
            public int To { get; }
            public int Weight { get; }

            public Edge(int from, int to, int weight)
            {
                From = from;
                To = to;
                Weight = weight;
            }
        }
    }
}
