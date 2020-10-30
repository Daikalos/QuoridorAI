using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class Prediction
{
    private readonly SpelBräde board;
    private readonly Graph graph;
    private readonly Spelare opponent;

    // Player's next evaluated position
    private readonly Point playerPos;

    private readonly bool[,] wallsVert;
    private readonly bool[,] wallsHori;

    private List<Vertex> plyPath;
    private List<Vertex> oppPath;

    // Find which path is the longest and use it to determine best placement against player
    private Tuple<Drag, int> bestMove;

    public Prediction(SpelBräde board, Graph graph, Point playerPos)
    {
        this.board = board;
        this.graph = graph;
        this.playerPos = playerPos;

        opponent = board.spelare[1];

        plyPath = A_Star.PathTo(graph, false,
            graph.AtPos(playerPos),
            graph.PlayerGoal());

        oppPath = A_Star.PathTo(graph, false,
            graph.AtPos(opponent.position),
            graph.OpponentGoal());

        wallsVert = board.vertikalaLångaVäggar;
        wallsHori = board.horisontellaLångaVäggar;

        bestMove = new Tuple<Drag, int>(new Drag(), 0);
    }

    public void OppWallPlacement(ref Drag move)
    {
        // Iterate through every vertex on path
        for (int i = 0; i < plyPath.Count - 1; ++i)
        {
            for (int j = -2; j <= 2; j++)
            {
                for (int k = -2; k <= 2; k++)
                {
                    TestWallPlacement(i, j, k);
                }
            }
        }

        // When all placements has been evaluated, return best placement against the player
        if (bestMove.Item2 > 0)
        {
            Drag newMove = new Drag();

            if (!BlockPlacementViable(bestMove.Item1, ref newMove))
                return;

            int x = newMove.point.X, 
                y = newMove.point.Y;

            if (wallsVert[x, y] || wallsHori[x, y])
                return;

            graph.AddWall((newMove.typ == Typ.Vertikal), x, y);

            List<Vertex> plyNewPath = A_Star.PathTo(graph, false,
                graph.AtPos(playerPos),
                graph.PlayerGoal());

            List<Vertex> oppNewPath = A_Star.PathTo(graph, false,
                graph.AtPos(opponent.position),
                graph.OpponentGoal());

            graph.RemoveWall((newMove.typ == Typ.Vertikal), x, y);

            // Do not attempt wall placement if no player can find any path to a goal
            if (oppNewPath.Count == 0 || plyNewPath.Count == 0)
                return;

            move = newMove;
        }
    }

    private void TestWallPlacement(int i, int j, int k)
    {
        Vertex vertexFrom = plyPath[i];
        Vertex vertexTo = plyPath[i + 1];

        Point offset = vertexFrom.Position - vertexTo.Position;
        Point placeAt = new Point(
            vertexFrom.Position.X + j,
            vertexFrom.Position.Y + k);

        if (!graph.WithinBoard(placeAt))
            return;

        // Which wall to place depending on if opponent is going horizontally or vertically
        bool placeVertical = (Math.Abs(offset.X) > 0);

        int x, y;

        if (placeVertical)
        {
            if (!PlaceVerticalWall(placeAt, out x, out y))
                return;

            if (wallsVert[x, y] || board.avanceradeRegler && wallsHori[x, y])
                return;
        }
        else
        {
            if (!PlaceHorizontalWall(placeAt, out x, out y))
                return;

            if (board.avanceradeRegler && wallsVert[x, y] || wallsHori[x, y])
                return;
        }

        graph.AddWall(placeVertical, x, y);

        List<Vertex> oppNewPath = A_Star.PathTo(graph, false,
            graph.AtPos(opponent.position),
            graph.OpponentGoal());

        List<Vertex> plyNewPath = A_Star.PathTo(graph, false,
            graph.AtPos(playerPos),
            graph.PlayerGoal());

        graph.RemoveWall(placeVertical, x, y);

        if (oppNewPath.Count == 0 || plyNewPath.Count == 0)
            return;

        int oppPathCount = Math.Max(0, oppNewPath.Count - oppPath.Count);
        int plyPathCount = Math.Max(0, plyNewPath.Count - plyPath.Count);

        // If this new path is much longer for player, then (x, y) is a suitable position for opponent to place a wall at
        if ((plyPathCount - oppPathCount) > AI_Data.predictionFreq)
        {
            if (plyPathCount > bestMove.Item2)
            {
                bestMove = new Tuple<Drag, int>(new Drag
                {
                    point = new Point(x, y),
                    typ = (placeVertical) ? Typ.Vertikal : Typ.Horisontell
                }, plyPathCount);
            }
        }
    }

    private bool PlaceVerticalWall(Point placeAt, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        if (!VerticalWallViable(x, y)) // Return true if it can be placed
            return false;

        return true;
    }
    private bool PlaceHorizontalWall(Point placeAt, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        if (!HorizontalWallViable(x, y)) // Return true if it can be placed
            return false;

        return true;
    }

    private bool VerticalWallViable(int x, int y)
    {
        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (graph.WithinWalls(x, y + i) && wallsVert[x, y + i])
            {
                if (graph.WithinWalls(x, y) && !wallsVert[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }
    private bool HorizontalWallViable(int x, int y)
    {
        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (graph.WithinWalls(x + i, y) && wallsHori[x + i, y])
            {
                if (graph.WithinWalls(x, y) && !wallsHori[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool BlockPlacementViable(Drag bestMove, ref Drag newMove)
    {
        if (bestMove.typ == Typ.Vertikal)
        {
            if (HorizontalWallViable(bestMove.point.X, bestMove.point.Y))
            {
                newMove.typ = Typ.Horisontell;
                newMove.point = bestMove.point;

                return true;
            }
        }
        if (bestMove.typ == Typ.Horisontell)
        {
            if (VerticalWallViable(bestMove.point.X, bestMove.point.Y))
            {
                newMove.typ = Typ.Vertikal;
                newMove.point = bestMove.point;

                return true;
            }
        }

        return false;
    }
}
