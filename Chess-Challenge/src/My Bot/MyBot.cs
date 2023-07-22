using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
class MyBot : IChessBot
{
    int[] pieceValues = { 10, 30, 30, 50, 90 };
    int[] centerSquares = { (3 * 64 + 3), (3 * 64 + 4), (4 * 64 + 3), (4 * 64 + 4) };

    public Move Think(Board board, Timer timer)
    {
        bool weAreWhite = board.IsWhiteToMove;
        Move[] moves = board.GetLegalMoves();
        int bestMoveScore = int.MinValue;
        Move bestMove = moves[0];
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = Evaluate(board, 4, weAreWhite);
            if (score > bestMoveScore)
            {
                bestMove = move;
                bestMoveScore = score;
            }
            
            board.UndoMove(move);
        }

        return bestMove;
    }

    /// <summary>
    /// Evaluate the approximate value of this position for us
    /// </summary>
    /// <param name="board">Board state after the last move</param>
    /// <returns></returns>
    private int Score(Board board, bool weAreWhite)
    {
        // If this is mate for us, max score. Otherwise, min score
        if (board.IsInCheckmate())
        {
            return (board.IsWhiteToMove == weAreWhite) ? int.MinValue : int.MaxValue;
        }

        int score = 0;
        // Reward piece count with traditional piece value
        for (int i = 1; i < 5; i++)
        {
            score += board.GetPieceList((PieceType)i, weAreWhite).Count * pieceValues[i - 1];
        }

        // Punish piece count of our opponent
        for (int i = 1; i < 5; i++)
        {
            score -= board.GetPieceList((PieceType)i, !weAreWhite).Count * pieceValues[i - 1];
        }

        // Reward center control
        foreach (int square in centerSquares)
        {
            if (board.SquareIsAttackedByOpponent(new Square(square)))
            {
                score += board.IsWhiteToMove == weAreWhite ? -10 : 10;
            }
            board.TrySkipTurn();
            if (board.SquareIsAttackedByOpponent(new Square(square)))
            {
                score += board.IsWhiteToMove == weAreWhite ? 10 : -10;
            }
            board.UndoSkipTurn();
        }

        // Find the amount of legal moves that we have
        if(board.IsWhiteToMove == weAreWhite)
        {
            //score += board.GetLegalMoves().Length; // Reward us for having more moves
        }
        else
        {
            //score -= board.GetLegalMoves().Length; // Punish us for our opponent having more moves
        }

        return score;
    }

    private int Evaluate(Board board, int depth, bool weAreWhite)
    {
        if (depth == 0)
        {
            int score = Score(board, weAreWhite);
            //Console.WriteLine("Evaluating board " + board.GetFenString() + ", score is " + score);
            return score;
        }

        // Game tree search logic
        Move[] moves = board.GetLegalMoves(); // Find possible moves
        // If this function searches for OUR move, we're keeping the one with the highest score
        // If this function searches for OPPONENT move, we're keeping the one with the lowest score
        int bestMoveScore = (board.IsWhiteToMove == weAreWhite) ? int.MinValue : int.MaxValue;
        foreach (var move in moves)
        {
            Console.WriteLine("Evaluating move " + move.ToString() + " (ply: " + board.PlyCount + ")");
            Console.WriteLine("Board: " + board.GetFenString());
            board.MakeMove(move);
            int score = Evaluate(board, depth - 1, weAreWhite);
            board.UndoMove(move);
            if (board.IsWhiteToMove == weAreWhite)
            {
                if (score > bestMoveScore)
                {
                    bestMoveScore = score;
                }
            }
            else
            {
                if (score < bestMoveScore)
                {
                    bestMoveScore = score;
                }
            }
        }

        return bestMoveScore;
    }

        
}
