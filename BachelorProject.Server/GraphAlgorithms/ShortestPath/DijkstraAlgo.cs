using BachelorProject.Server.Helpers;
using BachelorProject.Server.Models.DTO;

namespace BachelorProject.Server.GraphAlgorithms.ShortestPath
{
    public static class DijkstraAlgo
    {
        static int nodesCount;

        public static GraphStepsResultDto SolveGraph(GraphDto graph, bool makeSnapshots)
        {
            string[] nodeIds = GraphDtoConvertor.ToNodeIdArray(graph);
            nodesCount = nodeIds.Length;

            var nodeIndexMap = nodeIds
                                .Select((id, index) => new { Id = id, Index = index })
                                .ToDictionary(x => x.Id, x => x.Index);

            var adjList = GraphDtoConvertor.ToAdjacencyList(graph);

            string src = graph.Src.ToString()!;
            bool targetProvided = !string.IsNullOrWhiteSpace(graph.Target?.ToString());
            string? target = targetProvided ? graph.Target.ToString() : null;

            Snapshots snapshot = new Snapshots(graph, makeSnapshots);
            int currentTotalWeight = 0;

            if (!nodeIndexMap.TryGetValue(src, out int srcIndex))
                throw new ArgumentException("Source node not found in the node list.");
            int targetIndex = -1;
            if (targetProvided)
            {
                if (!nodeIndexMap.TryGetValue(target!, out targetIndex))
                    throw new ArgumentException("Target node not found in the node list.");
            }

            int[] dist = new int[nodesCount];
            bool[] visited = new bool[nodesCount];
            int[] previous = new int[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                dist[i] = int.MaxValue;
                visited[i] = false;
                previous[i] = -1;
            }
            dist[srcIndex] = 0;

            var priorityQueue = new PriorityQueue<int, int>();
            priorityQueue.Enqueue(srcIndex, 0);

            while (priorityQueue.Count > 0)
            {
                int u = priorityQueue.Dequeue();
                if (visited[u])
                    continue;

                visited[u] = true;
                snapshot.ColorNode(u, GraphHelpers.COLOR_PROCESSING);

                string currentNodeId = nodeIds[u];

                if (adjList.TryGetValue(currentNodeId, out List<(string to, int weight)>? neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!nodeIndexMap.TryGetValue(neighbor.to, out int v))
                            continue;

                        if (!visited[v] && dist[u] != int.MaxValue &&
                            dist[u] + neighbor.weight < dist[v])
                        {
                            snapshot.ColorEdge(currentNodeId, neighbor.to, GraphHelpers.COLOR_PROCESSING);
                            int newDist = dist[u] + neighbor.weight;
                            dist[v] = newDist;
                            previous[v] = u;

                            currentTotalWeight = newDist;
                            snapshot.UpdateCurrentTotalWeight(currentTotalWeight);
                            snapshot.ColorEdge(currentNodeId, neighbor.to, GraphHelpers.COLOR_PROCESSED);

                            priorityQueue.Enqueue(v, newDist);
                        }
                    }
                }
                snapshot.ColorNode(u, GraphHelpers.COLOR_PROCESSED);
            }

            int totalWeight = 0;
            List<string> resultEdgeIds = new List<string>();
            List<string> path = new List<string>();
            if (targetProvided)
            {
                int cur = targetIndex;
                while (cur != -1)
                {
                    path.Add(nodeIds[cur]);
                    cur = previous[cur];
                }
                path.Reverse();

                if (path.Count == 0 || path[0] != src)
                    throw new InvalidOperationException("No path exists between the source and target nodes.");

                totalWeight = dist[targetIndex];

                for (int i = 0; i < path.Count - 1; i++)
                {
                    string fromId = path[i];
                    string toId = path[i + 1];
                    string? edgeId = snapshot.GetEdgeId(fromId, toId);
                    resultEdgeIds.Add(edgeId ?? Guid.NewGuid().ToString());

                    int fromIndex = nodeIndexMap[fromId];
                    int toIndex = nodeIndexMap[toId];
                    snapshot.ColorEdge(fromIndex, toIndex, GraphHelpers.COLOR_RESULT);
                    snapshot.ColorNode(fromIndex, GraphHelpers.COLOR_RESULT);
                }
                int lastIndex = nodeIndexMap[path[path.Count - 1]];
                snapshot.ColorNode(lastIndex, GraphHelpers.COLOR_RESULT);
            }
            else
            {
                for (int i = 0; i < nodesCount; i++)
                {
                    if (i == srcIndex || previous[i] != -1)
                    {
                        path.Add(nodeIds[i]);
                        if (i != srcIndex)
                        {
                            string fromId = nodeIds[previous[i]];
                            string toId = nodeIds[i];
                            string? edgeId = snapshot.GetEdgeId(fromId, toId);
                            if (edgeId == null)
                                edgeId = Guid.NewGuid().ToString();
                            resultEdgeIds.Add(edgeId);
                            totalWeight += adjList[fromId]
                                               .First(edgeTuple => edgeTuple.to == toId).weight;
                        }
                    }
                }
                for (int i = 0; i < nodesCount; i++)
                {
                    if (i == srcIndex)
                        continue;
                    if (previous[i] != -1)
                    {
                        snapshot.ColorEdge(previous[i], i, GraphHelpers.COLOR_RESULT);
                    }
                }
                for (int i = 0; i < nodesCount; i++)
                {
                    snapshot.ColorNode(i, GraphHelpers.COLOR_RESULT);
                }
            }

            GraphResultDto resultGraph = new GraphResultDto
            {
                NodeIds = path.ToArray(),
                EdgeIds = resultEdgeIds.ToArray(),
                TotalWeight = totalWeight,
                AlgoType = GraphHelpers.AlgoTypes.DIJKSTRA
            };

            GraphStepsResultDto stepDto = new GraphStepsResultDto
            {
                Steps = snapshot.Steps,
                GraphResult = resultGraph
            };

            return stepDto;
        }

    }
}
