﻿using Azure.Core;
using BachelorProject.Server.Helpers;
using BachelorProject.Server.Interfaces;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.MinimumTree
{
    public class KruskalAlgo : AlgoBase
    {
        private class Edge
        {
            public int From { get; }
            public int To { get; }
            public int Weight { get; }
            public string OriginalEdgeId { get; }

            public Edge(int from, int to, int weight, string originalEdgeId)
            {
                From = from;
                To = to;
                Weight = weight;
                OriginalEdgeId = originalEdgeId;
            }
        }

        public static GraphStepsResultDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {

            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            var edgeList = GraphDtoConvertor.ToEdgeListDetailed(graph);
            Snapshots snapshot = new Snapshots(graph, makeSnapshots);

            int nodesCount = nodeIds.Length;

            var nodeIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < nodesCount; i++)
                nodeIndexMap[nodeIds[i]] = i;

            List<Edge> allEdges = new List<Edge>();
            foreach (var (edgeId, sourceId, targetId, weight) in edgeList)
            {
                if (weight == 0 || weight >= GraphHelpers.MAX_WEIGHT)
                    continue;

                if (!nodeIndexMap.TryGetValue(sourceId, out int fromIndex))
                    continue;
                if (!nodeIndexMap.TryGetValue(targetId, out int toIndex))
                    continue;

                if (fromIndex > toIndex)
                {
                    (fromIndex, toIndex) = (toIndex, fromIndex);
                }

                allEdges.Add(new Edge(fromIndex, toIndex, weight, edgeId));
            }

            allEdges.Sort((e1, e2) => e1.Weight.CompareTo(e2.Weight));

            int[] parent = new int[nodesCount];
            for (int i = 0; i < nodesCount; i++)
                parent[i] = i;

            List<Edge> mstEdges = new List<Edge>();

            foreach (var edge in allEdges)
            {
                snapshot.ColorEdge(edge.From, edge.To, GraphHelpers.COLOR_PROCESSING);

                int root1 = Find(parent, edge.From);
                int root2 = Find(parent, edge.To);

                if (root1 != root2)
                {
                    mstEdges.Add(edge);
                    Union(parent, root1, root2);

                    snapshot.ColorEdge(edge.From, edge.To, GraphHelpers.COLOR_RESULT);
                }
                else
                {
                    snapshot.ColorEdge(edge.From, edge.To, GraphHelpers.COLOR_DISCARDED);
                }
            }

            var mstEdgeIds = mstEdges.Select(e => e.OriginalEdgeId).ToArray();

            var resultGraph = new GraphResultDto
            {
                NodeIds = nodeIds,
                EdgeIds = mstEdgeIds,
                AlgoType = GraphHelpers.AlgoTypes.KRUSKAL
            };

            return new GraphStepsResultDto
            {
                Steps = snapshot.Steps,
                GraphResult = resultGraph
            };
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
    }
}
