using System.Collections.Generic;
using Microsoft.Xna.Framework;

class MoveOver
{
    private readonly Graph graph;

    private readonly List<Vertex> plyPath; // Player Path

    public MoveOver(Graph graph, List<Vertex> plyPath)
    {
        this.graph = graph;
        this.plyPath = plyPath;
    }

    public Point MoveOverOpponent(Point playerPos, Point opponentPos)
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
        if (!graph.WithinBoard(desiredPos) || !oppVertex.Neighbours.Contains(graph.AtPos(desiredPos)))
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
        for (int i = 0; i < desVertices.Count; i++) // At most 3
        {
            if (plyPath.Count < 3)
                break;

            if (plyPath[2] == desVertices[i])
            {
                return plyPath[2].Position;
            }
        }

        // Else return with going to the vertex closest to goal
        return Min(desVertices);
    }

    /// <summary>
    /// Return vertex of closest distance to goal
    /// </summary>
    private Point Min(List<Vertex> desVertices)
    {
        Vertex closest = desVertices[0];
        float smallest = float.MaxValue;

        for (int i = 0; i < desVertices.Count; i++)
        {
            float distance = Graph.Distance(desVertices[i], plyPath[plyPath.Count - 1]);
            if (distance < smallest)
            {
                closest = desVertices[i];
                smallest = distance;
            }
        }

        return closest.Position;
    }
}
