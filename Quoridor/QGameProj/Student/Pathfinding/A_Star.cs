using System;
using System.Collections.Generic;
using System.Linq;

class A_Star
{
    // Modified to allow multiple goal vertices, as quoridor has it
    public List<Vertex> pathTo(Vertex start, params Vertex[] end)
    {
        List<Vertex> path = new List<Vertex>();
        Queue<Vertex> open = new Queue<Vertex>();

        Vertex current = start;
        open.Enqueue(current);

        current.G = 0;

        Vertex closestEnd = end.OrderBy(v => Graph.Distance(start, v)).FirstOrDefault(); // Which goal to prioritize search for

        while (open.Count > 0)
        {
            open = new Queue<Vertex>(open.OrderBy(v => v.F)); // Slow cast to Priority Queue
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
                        neighbour.G = gScore;
                        neighbour.H = gScore + Graph.Distance(neighbour, closestEnd);

                        neighbour.Parent = current;

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour);
                    }
                }
            }
        }

        return new List<Vertex>(); // Return empty path if none is found
    } 

    private List<Vertex> FindPath(Vertex start, Vertex end) // Reconstruct path
    {
        List<Vertex> path = new List<Vertex>();
        Vertex current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
        }

        path.Reverse();

        return path;
    }
}
