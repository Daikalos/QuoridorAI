using System.Linq;
using System.Collections.Generic;

static class A_Star
{
    // Modified to allow multiple goal vertices, as quoridor has it
    // Done by doing reverse A*, we start from all goals and go to player as destination

    public static List<Vertex> PathTo(Graph graph, bool useWeight, Vertex goal, params Vertex[] outsets)
    {
        PriorityQueue<Vertex> open = new MinHeap<Vertex>();

        graph.InitializeVertices(); // Indirectly makes this algorithm O(N)

        for (int i = 0; i < outsets.Length; i++)
        {
            Vertex start = outsets[i];

            start.G = 0;
            start.H = 0;

            open.Enqueue(start, 0);
        }

        while (open.Count > 0)
        {
            Vertex current = open.Dequeue();

            if (!current.IsVisited)
            {
                current.IsVisited = true;

                if (current == goal)
                    return FindPath(current, outsets);

                foreach (Edge edge in current.Edges)
                {
                    Vertex neighbour = edge.To;

                    float edgeWeight = (useWeight) ? edge.Weight : 1;
                    float gScore = current.G + edgeWeight;

                    if (gScore < neighbour.G)
                    {
                        neighbour.Parent = current;

                        neighbour.G = gScore;
                        neighbour.H = Graph.Distance(neighbour, goal);

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour, neighbour.F);
                    }
                }
            }
        }

        return new List<Vertex>(); // Return empty path if none is found
    } 

    private static List<Vertex> FindPath(Vertex goal, params Vertex[] outsets) // Reconstruct path
    {
        List<Vertex> path = new List<Vertex>();
        Vertex current = goal;

        while (!outsets.Contains(current))
        {
            path.Add(current);
            current = current.Parent;
        }

        path.Add(current);

        return path;
    }
}
