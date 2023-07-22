using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
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

            // Reward piece count with traditional piece value
            int score = ((board.GetPieceList(PieceType.Pawn, weAreWhite).Count * 1) +
                    (board.GetPieceList(PieceType.Rook, weAreWhite).Count * 5) +
                    (board.GetPieceList(PieceType.Knight, weAreWhite).Count * 3) +
                    (board.GetPieceList(PieceType.Bishop, weAreWhite).Count * 3) +
                    (board.GetPieceList(PieceType.Queen, weAreWhite).Count * 9))
                    * 10; // The weight of the piece values is set here

            // Find the amount of legal moves that we have
            if (board.IsWhiteToMove == weAreWhite)
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
}