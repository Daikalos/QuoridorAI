using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

class Agent : BaseAgent 
{
    [STAThread]
    static void Main() 
    {
        Program.Start(new Agent());
    }

    public Agent() {}

    public override Drag SökNästaDrag(SpelBräde bräde) 
    {
        return !(bräde.avanceradeRegler) ? Normal(bräde) : Advanced(bräde);
    }

    private Drag Normal(SpelBräde bräde)
    {
        throw new System.NotImplementedException();
    }

    private Drag Advanced(SpelBräde bräde)
    {
        Spelare player = bräde.spelare[0];
        Spelare opponent = bräde.spelare[1];

        Point playerPos = player.position;
        Point opponentPos = opponent.position;

        Drag move = new Drag();

        Graph graph = new Graph(bräde);   // Generate a graph over map
        A_Star pathfinder = new A_Star(); // Use A* to find shortest path

        List<Vertex> destVertices = new List<Vertex>(); // Add goal vertices to find path to
        for (int x = 0; x < graph.boardWidth; x++) 
            destVertices.Add(graph.AtPos(x, graph.boardHeight - 1));

        List<Vertex> path = pathfinder.pathTo(
            graph.AtPos(playerPos),
            destVertices.ToArray());

        if (path.Count == 0) // No valid path found to goal
            throw new System.ArgumentOutOfRangeException();

        Point desiredPos = path[0].Position;

        // If desired position is on the opponents position
        if (desiredPos == opponentPos)
            desiredPos = MoveOverOpponent(graph, path, playerPos, opponentPos);

        move.typ = Typ.Flytta;
        move.point = desiredPos;

        // PLACE WALL

        return move;
    }

    private Point MoveOverOpponent(Graph graph, List<Vertex> path, Point playerPos, Point opponentPos)
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
                return SuitableVertex(path, desVertices);

            // If not, set desired position to 'random' valid neighbour next to player
            foreach (Vertex neighbour in plyVertex.Neighbours)
            {
                if (neighbour != oppVertex)
                {
                    desVertices.Add(neighbour);
                }
            }

            if (desVertices.Count > 0)
                return SuitableVertex(path, desVertices);
        }

        return desiredPos;
    }

    /// <summary>
    /// Return position of a suitable vertex to go to
    /// </summary>
    private Point SuitableVertex(List<Vertex> path, List<Vertex> desVertices)
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

        // Else return with going to vertex closest to goal
        return Min(path, desVertices.ToArray()).Position;
    }

    /// <summary>
    /// Return vertex of closest distance to goal
    /// </summary>
    private Vertex Min(List<Vertex> path, params Vertex[] vertices)
    {
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

    public override Drag GörOmDrag(SpelBräde bräde, Drag drag)
    {
        System.Diagnostics.Debugger.Break();
        return SökNästaDrag(bräde);
    }
}
//enum Typ { Flytta, Horisontell, Vertikal }
//struct Drag {
//    public Typ typ;
//    public Point point;
//}