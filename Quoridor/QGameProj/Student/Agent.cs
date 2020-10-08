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

    public Agent() { }

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

        int boardWidth = bräde.horisontellaLångaVäggar.GetLength(0);

        Drag move = new Drag();

        Graph graph = new Graph(bräde);   // Generate graph of map
        A_Star pathfinder = new A_Star(); // Use A_Star to find shortest path

        List<Vertex> path = pathfinder.pathTo(
            graph.AtPos(playerPos.X, playerPos.Y),
            graph.AtPos(opponentPos.X, opponentPos.Y));

        if (path == null)
        {
            Console.WriteLine("NO PATH FOUND");
            throw new System.ArgumentOutOfRangeException();
        }

        move.typ = Typ.Flytta;
        move.point = path[0].Position;

        return move;
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