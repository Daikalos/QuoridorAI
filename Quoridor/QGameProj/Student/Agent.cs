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
        Spelare player = bräde.spelare[0];
        Spelare opponent = bräde.spelare[1];

        Point playerPos = player.position;
        Point opponentPos = opponent.position;

        Drag move = new Drag();

        Graph graph = new Graph(bräde); // Generate a graph over map
        graph.GenerateGraph();

        SetEdgeWeight(graph, opponent);

        // Use A* to find shortest path
        List<Vertex> plyPath = A_Star.PathTo(graph,
            graph.AtPos(playerPos),
            graph.PlayerGoal());

        if (plyPath.Count <= 1) // No valid path found to goal
            throw new System.ArgumentOutOfRangeException();

        Point desiredPos = plyPath[1].Position;

        // If desired position is on the opponents position
        if (bräde.avanceradeRegler && desiredPos == opponentPos)
            desiredPos = new MoveOver(plyPath).MoveOverOpponent(graph, playerPos, opponentPos);

        move.typ = Typ.Flytta;
        move.point = desiredPos;

        if (player.antalVäggar > 0) // Check if possible to place a wall
        {
            if (opponent.antalVäggar > 0)
                new PredictOpponent(bräde, plyPath, move.point).PredictOpponentsMove(ref move);

            List<Vertex> oppPath = A_Star.PathTo(graph,
                graph.AtPos(opponent.position),
                graph.OpponentGoal());

            // Prioratize defense when opponent is close to goal
            if (oppPath.Count <= 3 && plyPath.Count > 3)
                new WallPlacement(bräde, oppPath, true).PlaceWall(ref move);

            if (oppPath.Count < plyPath.Count)
                new WallPlacement(bräde, oppPath, false).PlaceWall(ref move);
        }

        Console.WriteLine(move.typ + ", " + move.point);

        return move;
    }

    private void SetEdgeWeight(Graph graph, Spelare opponent)
    {
        // Set the weight of each unique edge depending on number of paths available
        for (int y = 0; y < graph.boardHeight; y++)
        {
            int x = (y % 2 == 1) ? 1 : 0;
            for (; x < graph.boardWidth; x += 2)
            {
                Vertex vertex = graph.AtPos(x, y);
                foreach (Edge edge in vertex.Edges)
                {
                    if (vertex.EdgeCount == 0)
                        break;

                    edge.Weight = 1 + ((4 / vertex.EdgeCount) * opponent.antalVäggar * 4.0f);
                }
            }
        }
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