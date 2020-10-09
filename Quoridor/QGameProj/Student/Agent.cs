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

        int boardWidth = bräde.horisontellaLångaVäggar.GetLength(0) + 1;
        int boardHeight = bräde.horisontellaLångaVäggar.GetLength(0) + 1;

        Drag move = new Drag();

        Graph graph = new Graph(bräde);   // Generate graph of map
        A_Star pathfinder = new A_Star(); // Use A_Star to find shortest path

        List<Vertex> destVertices = new List<Vertex>();
        for (int x = 0; x < boardWidth; x++) // Add goal vertices to find path to
            destVertices.Add(graph.AtPos(x, boardHeight - 1));

        List<Vertex> path = pathfinder.pathTo(
            graph.AtPos(playerPos),
            destVertices.ToArray());

        if (path.Count == 0)
        {
            Console.WriteLine("NO PATH FOUND");
            throw new System.ArgumentOutOfRangeException();
        }

        MoveOverOpponent(graph, path, opponentPos);

        move.typ = Typ.Flytta;
        move.point = path[0].Position;

        return move;
    }

    private static void MoveOverOpponent(Graph graph, List<Vertex> path, Point opponentPos)
    {
        if (path[0].Position == opponentPos)
        {
            Vertex vertex = graph.AtPos(opponentPos);

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