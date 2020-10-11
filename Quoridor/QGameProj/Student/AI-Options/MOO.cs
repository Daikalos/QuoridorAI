using System.Collections.Generic;
using Microsoft.Xna.Framework;

// Move Over Opponent
class MOO
{
    private List<Vertex> path; // Player Path

    public MOO(List<Vertex> path)
    {
        this.path = path;
    }

    public Point MoveOverOpponent(Graph graph, Point playerPos, Point opponentPos)
    {
        Point offset = opponentPos - playerPos;
        Point desiredPos = playerPos + offset;

        if (offset.X == 1)  // Opponent is to the left
            desiredPos.X++;
        if (offset.X == -1) // Opponent is to the right
            desiredPos.X--;
        if (offset.Y == -1) // Opponent is up
            desiredPos.Y--;
        if (offset.Y == 1)  // Opponent is down
            desiredPos.Y++;

        Vertex plyVertex = graph.AtPos(playerPos);
        Vertex oppVertex = graph.AtPos(opponentPos);

        // If there is no path to desired position
        if (!Graph.WithinBounds(desiredPos, graph.boardWidth, graph.boardHeight) || !oppVertex.Neighbours.Contains(graph.AtPos(desiredPos)))
        {
            List<Vertex> desVertices = new List<Vertex>();

            // First see if we can move diagonally to one of opponents neighbours
            foreach (Vertex neighbour in oppVertex.Neighbours)
            {
                if (neighbour != plyVertex)
                {
                    desVertices.Add(neighbour);
                }
            }

            if (desVertices.Count > 0)
                return SuitableVertex(desVertices);

            // If not, set desired position to suitable neighbour next to player
            foreach (Vertex neighbour in plyVertex.Neighbours)
            {
                if (neighbour != oppVertex)
                {
                    desVertices.Add(neighbour);
                }
            }

            if (desVertices.Count > 0)
                return SuitableVertex(desVertices);
        }

        return desiredPos;
    }

    private Point SuitableVertex(List<Vertex> desVertices)
    {
        // If the vertex we are trying to go to is already contained in path, go to it
        for (int i = 0; i < path.Count; i++)
        {
            for (int j = 0; j < desVertices.Count; j++) // At most 3
            {
                if (path[i] == desVertices[j])
                {
                    return path[i].Position;
                }
            }
        }

        // Else return with going to the vertex closest to goal
        return Min(desVertices.ToArray()).Position;
    }

    private Vertex Min(params Vertex[] vertices)
    {
        // Return vertex of closest distance to goal

        Vertex closest = vertices[0];
        float smallest = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Graph.Distance(vertices[i], path[path.Count - 1]);
            if (distance < smallest)
            {
                closest = vertices[i];
                smallest = distance;
            }
        }

        return closest;
    }
}
