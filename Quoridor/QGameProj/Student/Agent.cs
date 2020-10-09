using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
            desiredPos = MoveOverOpponent(graph, playerPos, opponentPos);

        move.typ = Typ.Flytta;
        move.point = desiredPos;

        return move;
    }

    private Point MoveOverOpponent(Graph graph, Point playerPos, Point opponentPos)
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
        if (!graph.WithinBounds(desiredPos, graph.boardWidth, graph.boardHeight) || !oppVertex.Neighbours.Contains(graph.AtPos(desiredPos)))
        {
            // Set desired position to 'random' valid neighbour
            foreach (Vertex neighbour in plyVertex.Neighbours)
            {
                if (neighbour != oppVertex)
                {
                    desiredPos = neighbour.Position;
                    break;
                }
            }
        }

        return desiredPos;
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