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

    public override Drag SökNästaDrag(SpelBräde bräde) // Recommended for advanced rules
    {
        Spelare player = bräde.spelare[0];
        Spelare opponent = bräde.spelare[1];

        Point playerPos = player.position;
        Point opponentPos = opponent.position;

        Drag move = new Drag();

        Graph graph = new Graph(bräde); // Generate a graph over map
        graph.GenerateGraph();
        graph.SetEdgeWeight();

        // Use A* to find shortest path
        List<Vertex> plyPath = A_Star.PathTo(graph, true,
            graph.AtPos(playerPos),
            graph.PlayerGoal());

        if (plyPath.Count == 0) // No valid path found to goal
            throw new System.ArgumentOutOfRangeException();

        Point desiredPos = plyPath[1].Position;

        // If desired position is on the opponents position
        if (bräde.avanceradeRegler && desiredPos == opponentPos)
            desiredPos = new MoveOver(plyPath).MoveOverOpponent(graph, playerPos, opponentPos);

        move.typ = Typ.Flytta;
        move.point = desiredPos;

        if (player.antalVäggar > 0) // Check if possible to place a wall
        {
            List<Vertex> oppPath = A_Star.PathTo(graph, true,
                graph.AtPos(opponentPos),
                graph.OpponentGoal());

            // Try to predict where opponent may place a wall to hinder you, and counteract it if possible
            if (opponent.antalVäggar > 0)
                new Prediction(bräde, graph, move.point).OppWallPlacement(ref move);

            // Prioratize defense when opponent is close to goal
            if (oppPath.Count <= AI_Data.prioDefense && oppPath.Count < plyPath.Count)
                new WallPlacement(bräde, graph, true).PlaceWall(ref move);

            if (oppPath.Count < plyPath.Count)
                new WallPlacement(bräde, graph, false).PlaceWall(ref move);
        }

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