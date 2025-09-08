using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe
{
    enum PlayerType { Human, AI }

    class Program
    {
        static void Main()
        {
            Console.Title = "Tic-Tac-Toe (C# Console)";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Tic-Tac-Toe ===");
                Console.WriteLine("1) Human vs Human");
                Console.WriteLine("2) Human vs AI");
                Console.Write("Choose mode (1/2): ");
                var mode = Console.ReadLine();
                var vsAI = mode?.Trim() == "2";

                var board = new Board();
                char current = 'X';

                (PlayerType X, PlayerType O) players = vsAI
                    ? (PlayerType.Human, PlayerType.AI)
                    : (PlayerType.Human, PlayerType.Human);

                while (true)
                {
                    DrawInstructions();
                    board.Render();

                    if (board.IsWin('X') || board.IsWin('O') || board.IsDraw())
                        break;

                    if (GetPlayer(players, current) == PlayerType.Human)
                    {
                        Console.Write($"\n{current}'s turn. Enter 1–9: ");
                        if (!TryReadMove(board, out int move))
                        {
                            Console.WriteLine("Invalid input. Press any key to try again...");
                            Console.ReadKey(true);
                            Console.Clear();
                            continue;
                        }
                        board.Place(move, current);
                    }
                    else
                    {
                        Console.WriteLine($"\nAI ({current}) is thinking...");
                        int best = Minimax.FindBestMove(board, current, GetOpponent(current));
                        board.Place(best, current);
                    }

                    // Clear screen after every move
                    Console.Clear();

                    current = GetOpponent(current);

                }

                Console.Clear();
                board.Render();
                Console.WriteLine();
                if (board.IsWin('X')) Console.WriteLine("X wins! 🎉");
                else if (board.IsWin('O')) Console.WriteLine("O wins! 🎉");
                else Console.WriteLine("It's a draw. 🤝");

                Console.Write("\nPlay again? (y/n): ");
                var again = Console.ReadLine();
                if (!again?.Trim().ToLowerInvariant().StartsWith("y") ?? true)
                    break;
            }
        }

        static PlayerType GetPlayer((PlayerType X, PlayerType O) players, char mark)
            => mark == 'X' ? players.X : players.O;

        static char GetOpponent(char c) => c == 'X' ? 'O' : 'X';

        static void DrawInstructions()
        {
            Console.WriteLine("Board positions (numpad-style):");
            Console.WriteLine(" 7 | 8 | 9 ");
            Console.WriteLine("---+---+---");
            Console.WriteLine(" 4 | 5 | 6 ");
            Console.WriteLine("---+---+---");
            Console.WriteLine(" 1 | 2 | 3 \n");
        }

        static bool TryReadMove(Board board, out int cell)
        {
            cell = -1;
            var s = Console.ReadLine()?.Trim();
            if (!int.TryParse(s, out int pos)) return false;
            if (!Board.Positions.ContainsKey(pos)) return false;
            int index = Board.Positions[pos];
            if (!board.IsEmpty(index)) return false;
            cell = index;
            return true;
        }
    }

    class Board
    {
        // Map numpad-like input to indices (0..8)
        public static readonly Dictionary<int, int> Positions = new Dictionary<int, int>
        {
            {7, 0}, {8, 1}, {9, 2},
            {4, 3}, {5, 4}, {6, 5},
            {1, 6}, {2, 7}, {3, 8}
        };

        private readonly char[] cells = Enumerable.Repeat(' ', 9).ToArray();

        private static readonly int[][] Wins = new[]
        {
            new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
            new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
            new[] {0,4,8}, new[] {2,4,6}
        };
        public void Render()
        {
            // Mapping from index to numpad-style reference numbers
            string[] labels = { "7", "8", "9", "4", "5", "6", "1", "2", "3" };

            for (int r = 0; r < 3; r++)
            {
                string Row(int i)
                {
                    string Cell(int idx) =>
                        cells[idx] == ' ' ? labels[idx] : cells[idx].ToString();
                    return $" {Cell(i)} | {Cell(i+1)} | {Cell(i+2)} ";
                }

                Console.WriteLine(Row(r * 3));
                if (r < 2) Console.WriteLine("---+---+---");
            }
        }


        public bool IsEmpty(int idx) => cells[idx] == ' ';
        public void Place(int idx, char mark) { if (IsEmpty(idx)) cells[idx] = mark; }
        public void Undo(int idx) => cells[idx] = ' ';
        public IEnumerable<int> EmptyIndices() => Enumerable.Range(0, 9).Where(IsEmpty);
        public bool IsWin(char m) => Wins.Any(w => w.All(i => cells[i] == m));
        public bool IsDraw() => !IsWin('X') && !IsWin('O') && cells.All(c => c != ' ');
        public char At(int idx) => cells[idx];
    }

    static class Minimax
    {
        // Returns the best index for 'ai' to play
        public static int FindBestMove(Board board, char ai, char human)
        {
            int bestScore = int.MinValue;
            int bestMove = board.EmptyIndices().First();
            foreach (var move in board.EmptyIndices())
            {
                board.Place(move, ai);
                int score = Score(board, ai, human, false, 0);
                board.Undo(move);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            return bestMove;
        }

        // Classic minimax with small depth heuristic to prefer faster wins
        private static int Score(Board board, char ai, char human, bool maximizing, int depth)
        {
            if (board.IsWin(ai)) return 10 - depth;
            if (board.IsWin(human)) return depth - 10;
            if (board.IsDraw()) return 0;

            if (maximizing)
            {
                int best = int.MinValue;
                foreach (var move in board.EmptyIndices())
                {
                    board.Place(move, ai);
                    best = Math.Max(best, Score(board, ai, human, false, depth + 1));
                    board.Undo(move);
                }
                return best;
            }
            else
            {
                int best = int.MaxValue;
                foreach (var move in board.EmptyIndices())
                {
                    board.Place(move, human);
                    best = Math.Min(best, Score(board, ai, human, true, depth + 1));
                    board.Undo(move);
                }
                return best;
            }
        }
    }
}