using System;
using System.Collections.Generic;
using System.Linq;

class A_Star : Pathfinder
{
    public List<Vertex> pathTo(Vertex start, Vertex end)
    {
        List<Vertex> path = new List<Vertex>();
        Queue<Vertex> open = new Queue<Vertex>();

        Vertex current = start;
        open.Enqueue(current);

        current.G = 0;

        while (open.Count > 0)
        {
            open = new Queue<Vertex>(open.OrderBy(v => v.F)); // Slow cast to Priority Queue
            current = open.Dequeue();

            if (!current.IsVisited)
            {
                current.IsVisited = true;

                if (current.Equals(end))
                    return FindPath(start, end);

                foreach (Edge edge in current.Edges)
                {
                    Vertex neighbour = edge.To;

                    float gScore = current.G + edge.Weight;
                    if (gScore < neighbour.G)
                    {
                        neighbour.G = gScore;
                        neighbour.H = gScore + Distance(neighbour, end);

                        neighbour.Parent = current;

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour);
                    }
                }
            }
        }

        return new List<Vertex>(); 
    } 

    private List<Vertex> FindPath(Vertex start, Vertex end)
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

    private float Distance(Vertex from, Vertex to)
    {
        return (float)Math.Sqrt(
            Math.Pow(from.Position.X - to.Position.X, 2) +
            Math.Pow(from.Position.Y - to.Position.Y, 2));
    }
}
