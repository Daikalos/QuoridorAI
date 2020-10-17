using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class Prediction
{
    private Graph graph;

    private Spelare player;
    private Spelare opponent;

    private List<Vertex> oppPath;
    private List<Vertex> plyPath;

    // Find which path is the longest and use it to determine worst placement for player
    private List<Tuple<Drag, int>> bestMoves;

    // Player's next evaluated position
    private Point playerPos;

    private bool[,] wallsVert;
    private bool[,] wallsHori;

    public Prediction(SpelBräde board, Graph graph, Point playerPos)
    {
        this.graph = graph;
        this.playerPos = playerPos;

        player = board.spelare[0];
        opponent = board.spelare[1];

        oppPath = A_Star.PathTo(graph, false,
            graph.AtPos(opponent.position),
            graph.OpponentGoal());

        plyPath = A_Star.PathTo(graph, false,
            graph.AtPos(playerPos),
            graph.PlayerGoal());

        wallsVert = board.vertikalaLångaVäggar;
        wallsHori = board.horisontellaLångaVäggar;

        bestMoves = new List<Tuple<Drag, int>>();
    }

    public void OppWallPlacement(ref Drag move)
    {
        // Iterate through every vertex on path
        for (int i = 0; i < plyPath.Count - 1; ++i)
        {
            TestWallPlacement(i, 0, 0);

            for (int j = -3; j <= 3; ++j)
                if (j != 0) TestWallPlacement(i, j, 0);

            for (int k = -3; k <= 3; ++k)
                if (k != 0) TestWallPlacement(i, 0, k);

            for (int l = -3; l <= 3; ++l)
                if (l != 0) TestWallPlacement(i, l, l);

            for (int l = -3; l <= 3; ++l)
                if (l != 0) TestWallPlacement(i, -l, l);
        }

        // When all placements has been evaluated, return best placement against the player
        if (bestMoves.Count > 0)
        {
            Drag bestMove = BestPlacement();
            Drag newMove = new Drag();

            if (!BlockPlacementViable(bestMove, ref newMove))
                return;

            graph.AddWall((newMove.typ == Typ.Vertikal),
                newMove.point.X, newMove.point.Y);

            List<Vertex> plyNewPath = A_Star.PathTo(graph, false,
                graph.AtPos(playerPos),
                graph.PlayerGoal());

            List<Vertex> oppNewPath = A_Star.PathTo(graph, false,
                graph.AtPos(opponent.position),
                graph.OpponentGoal());

            graph.RemoveWall((newMove.typ == Typ.Vertikal),
                newMove.point.X, newMove.point.Y);

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

        // Which wall to place depending on if opponent is going horizontally or vertically
        bool placeVertical = (Math.Abs(offset.X) > 0);

        int x = -1;
        int y = -1;

        if (Graph.WithinBounds(placeAt, graph.boardWidth, graph.boardHeight))
        {
            if (placeVertical)
            {
                if (!PlaceVerticalWall(placeAt, offset, out x, out y))
                    return;
            }
            else
            {
                if (!PlaceHorizontalWall(placeAt, offset, out x, out y))
                    return;
            }
        }

        if (x == -1 || y == -1 || wallsVert[x, y] || wallsHori[x, y])
            return;

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
            bestMoves.Add(new Tuple<Drag, int>(new Drag
            {
                point = new Point(x, y),
                typ = (placeVertical) ? Typ.Vertikal : Typ.Horisontell
            }, plyPathCount));
        }
    }

    private bool PlaceVerticalWall(Point placeAt, Point offset, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        for (int i = -1; i <= 1; i += 2)
        {
            if (Graph.WithinBounds(x, y + i, graph.wallLengthX, graph.wallLengthY) && wallsVert[x, y + i])
            {
                if (Graph.WithinBounds(x, y - i, graph.wallLengthX, graph.wallLengthY))
                {
                    y = y - i;
                }
            }
        }

        if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && wallsHori[x, y])
        {
            if (Graph.WithinBounds(x - offset.X, y, graph.wallLengthX, graph.wallLengthY))
            {
                x += -offset.X;
            }
        }

        if (!VerticalWallViable(x, y))
            return false;

        return true;
    }
    private bool PlaceHorizontalWall(Point placeAt, Point offset, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        for (int i = -1; i <= 1; i += 2)
        {
            if (Graph.WithinBounds(x + i, y, graph.wallLengthX, graph.wallLengthY) && wallsHori[x + i, y])
            {
                if (Graph.WithinBounds(x - i, y, graph.wallLengthX, graph.wallLengthY))
                {
                    x = x - i;
                }
            }
        }

        if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && wallsVert[x, y])
        {
            if (Graph.WithinBounds(x, y - offset.Y, graph.wallLengthX, graph.wallLengthY))
            {
                y += -offset.Y;
            }
        }

        if (!HorizontalWallViable(x, y))
            return false;

        return true;
    }

    private bool VerticalWallViable(int x, int y)
    {
        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x, y + i, graph.wallLengthX, graph.wallLengthY) && wallsVert[x, y + i])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !wallsVert[x, y])
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
            if (Graph.WithinBounds(x + i, y, graph.wallLengthX, graph.wallLengthY) && wallsHori[x + i, y])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !wallsHori[x, y])
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

    private Drag BestPlacement()
    {
        Drag move = new Drag();

        int largest = 0;
        foreach (Tuple<Drag, int> bestMove in bestMoves)
        {
            if (bestMove.Item2 > largest)
            {
                move = bestMove.Item1;
                largest = bestMove.Item2;
            }
        }

        return move;
    }
}
