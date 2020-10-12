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

        Graph graph = new Graph(bräde); // Generate a graph over map
        graph.GenerateGraph();      

        SetEdgeWeight(graph, opponent);

        List<Vertex> destVertices = new List<Vertex>(); // Add goal vertices to find path to
        for (int x = 0; x < graph.boardWidth; x++) 
            destVertices.Add(graph.AtPos(x, graph.boardHeight - 1));

        // Use A* to find shortest path
        List<Vertex> plyPath = A_Star.PathTo(graph,
            graph.AtPos(playerPos),
            destVertices.ToArray());

        if (plyPath.Count <= 1) // No valid path found to goal
            throw new System.ArgumentOutOfRangeException();

        Point desiredPos = plyPath[1].Position;

        // If desired position is on the opponents position
        if (desiredPos == opponentPos)
            desiredPos = new MoveOver(plyPath).MoveOverOpponent(graph, playerPos, opponentPos);

        move.typ = Typ.Flytta;
        move.point = desiredPos;

        if (player.antalVäggar > 0) // Check if possible to place a wall
        {
            List<Vertex> oppDestVertices = new List<Vertex>(); // Add opponents goal vertices to find path to
            for (int x = 0; x < graph.boardWidth; x++)
                oppDestVertices.Add(graph.AtPos(x, 0));

            List<Vertex> oppPath = A_Star.PathTo(graph,
                graph.AtPos(opponent.position),
                oppDestVertices.ToArray());

            Console.WriteLine(oppPath.Count + ", " + plyPath.Count);

            // If opponent is much closer to goal than player
            if (oppPath.Count < (plyPath.Count - 1) || player.antalVäggar == 10)
                new WallPlacement(bräde, oppPath, true).PlaceWall(ref move);
            
            if (oppPath.Count < plyPath.Count || opponent.antalVäggar < player.antalVäggar)
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