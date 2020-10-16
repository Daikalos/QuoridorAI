using System.Collections.Generic;
using System.Linq;

static class A_Star
{
    // Modified to allow multiple goal vertices, as quoridor has it
    public static List<Vertex> PathTo(Graph graph, Vertex start, params Vertex[] end)
    {
        PriorityQueue<Vertex> open = new PriorityQueue<Vertex>();

        graph.InitializeVertices();
        graph.SetEdgeWeight();

        Vertex current = start;
        open.Enqueue(current, current.F);

        current.G = 0;

        Vertex closestEnd = end.OrderBy(
            v => Graph.Distance(start, v)).FirstOrDefault(); // Which goal to prioritize search for

        while (open.Count > 0)
        {
            current = open.Dequeue();

            if (!current.IsVisited)
            {
                current.IsVisited = true;

                if (end.Contains(current))
                    return FindPath(start, current);

                foreach (Edge edge in current.Edges)
                {
                    Vertex neighbour = edge.To;

                    float gScore = current.G + edge.Weight;
                    if (gScore < neighbour.G)
                    {
                        neighbour.Parent = current;

                        neighbour.G = gScore;
                        neighbour.H = Graph.Distance(neighbour, closestEnd);

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour, neighbour.F);
                    }
                }
            }
        }

        return new List<Vertex>(); // Return empty path if none is found
    } 

    private static List<Vertex> FindPath(Vertex start, Vertex end) // Reconstruct path
    {
        List<Vertex> path = new List<Vertex>();
        Vertex current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
        }

        path.Add(start);
        path.Reverse();

        return path;
    }
}
