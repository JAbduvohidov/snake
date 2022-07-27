using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace snake
{
    internal static class Program
    {
        private class Coordination
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Coordination(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        private class Snake
        {
            private List<Coordination> Body { get; set; }
            public Direction Direction { get; set; }

            public Snake()
            {
                Body = new List<Coordination>
                {
                    new(7, 4),
                    new(6, 4),
                    new(5, 4),
                    new(4, 4),
                    new(3, 4)
                };
                Direction = Direction.Right;
            }

            public void Start(ref bool running, Boundary boundary)
            {
                var food = new Food();
                while (running)
                {
                    Console.CursorVisible = false;

                    if (!food.IsAvailable)
                    {
                        var foodPosition = food.GenerateFood(boundary);
                        var ok = CheckFoodPosition(this, foodPosition);
                        while (!ok)
                        {
                            foodPosition = food.GenerateFood(boundary);
                            ok = CheckFoodPosition(this, foodPosition);
                        }

                        food.IsAvailable = true;
                    }


                    boundary.Render();
                    food.Render();
                    Render();

                    var collides = CheckSnakeAndFoodCollision(this, food);
                    if (collides)
                    {
                        food.IsAvailable = false;
                        Grow();
                    }

                    var collidesToBoundary = CheckSnakeAndBoundaryCollision(this, boundary);
                    if (collidesToBoundary)
                    {
                        running = false;
                        return;
                    }

                    var collidesToSelf = CheckSnakeSelfCollision(this);
                    if (collidesToSelf)
                    {
                        running = false;
                        return;
                    }

                    Thread.Sleep(200);
                    Console.Clear();
                    Move();
                }
            }

            private static bool CheckSnakeSelfCollision(Snake snake)
            {
                var firstCell = snake.Body.First();
                for (var i = 1; i < snake.Body.Count; i++)
                    if (firstCell.X == snake.Body[i].X && firstCell.Y == snake.Body[i].Y)
                        return true;

                return false;
            }

            private static bool CheckSnakeAndBoundaryCollision(Snake snake, Boundary boundary)
            {
                var onBoundaryAb = boundary.Start.Y == snake.Body.First().Y &&
                                   snake.Body.First().X >= boundary.Start.X && snake.Body.First().X <= boundary.End.X;
                var onBoundaryDc = boundary.End.Y == snake.Body.First().Y &&
                                   snake.Body.First().X >= boundary.Start.X && snake.Body.First().X <= boundary.End.X;
                var onBoundaryBc = boundary.End.X == snake.Body.First().X && snake.Body.First().Y >= boundary.Start.Y &&
                                   snake.Body.First().Y <= boundary.End.Y;
                var onBoundaryAd = boundary.Start.X == snake.Body.First().X &&
                                   snake.Body.First().X >= boundary.Start.X &&
                                   snake.Body.First().X <= boundary.End.X;
                return onBoundaryAb || onBoundaryBc || onBoundaryDc || onBoundaryAd;
            }

            private void Grow() => Body.Add(new Coordination(Body.Last().X + 1, Body.Last().Y));

            private static bool CheckSnakeAndFoodCollision(Snake snake, Food food) =>
                snake.Body.Any(c => c.X == food.Position.X && c.Y == food.Position.Y);

            private static bool CheckFoodPosition(Snake snake, Coordination foodPosition) =>
                snake.Body.All(t => t.X != foodPosition.X || t.Y != foodPosition.Y);

            private void Move()
            {
                switch (Direction)
                {
                    case Direction.Left:
                        for (var i = Body.Count - 1; i >= 0; i--)
                        {
                            if (i == 0)
                            {
                                Body[i].X -= 1;
                                Body[i].Y = Body[i].Y;
                                break;
                            }

                            Body[i].X = Body[i - 1].X;
                            Body[i].Y = Body[i - 1].Y;
                        }

                        break;
                    case Direction.Right:
                        for (var i = Body.Count - 1; i >= 0; i--)
                        {
                            if (i == 0)
                            {
                                Body[i].X += 1;
                                Body[i].Y = Body[i].Y;
                                break;
                            }

                            Body[i].X = Body[i - 1].X;
                            Body[i].Y = Body[i - 1].Y;
                        }

                        break;
                    case Direction.Up:
                        for (var i = Body.Count - 1; i >= 0; i--)
                        {
                            if (i == 0)
                            {
                                Body[i].X = Body[i].X;
                                Body[i].Y -= 1;
                                break;
                            }

                            Body[i].X = Body[i - 1].X;
                            Body[i].Y = Body[i - 1].Y;
                        }

                        break;
                    case Direction.Down:
                        for (var i = Body.Count - 1; i >= 0; i--)
                        {
                            if (i == 0)
                            {
                                Body[i].X = Body[i].X;
                                Body[i].Y += 1;
                                break;
                            }

                            Body[i].X = Body[i - 1].X;
                            Body[i].Y = Body[i - 1].Y;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void Render()
            {
                Body.ForEach(c =>
                {
                    Console.SetCursorPosition(c.X, c.Y);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("█");
                    Console.ForegroundColor = ConsoleColor.White;
                });
            }
        }

        private class Food
        {
            public Coordination Position { get; private set; }
            public bool IsAvailable { get; set; }

            public Coordination GenerateFood(Boundary boundary)
            {
                var rand = new Random();

                var x = rand.Next(boundary.Start.X + 1, boundary.End.X - 1);
                var y = rand.Next(boundary.Start.Y + 1, boundary.End.Y - 1);

                Position = new Coordination(x, y);

                return Position;
            }

            public void Render()
            {
                Console.SetCursorPosition(Position.X, Position.Y);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("■");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private class Boundary
        {
            public Coordination Start { get; }
            public Coordination End { get; }

            public Boundary(Coordination start, Coordination end)
            {
                Start = start;
                End = end;
            }

            public void Render()
            {
                for (var i = Start.Y; i <= End.Y; i++)
                {
                    for (var j = Start.X; j <= End.X; j++)
                    {
                        Console.SetCursorPosition(j, i);
                        if (i == Start.Y || i == End.Y)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write("█");
                            Console.ForegroundColor = ConsoleColor.White;
                            continue;
                        }

                        if (j == Start.X || j == End.X)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write("█");
                            Console.ForegroundColor = ConsoleColor.White;
                            continue;
                        }

                        Console.Write(" ");
                    }

                    Console.WriteLine();
                }
            }
        }

        private static void Main()
        {
            var running = true;

            var boundary = new Boundary(new Coordination(0, 0), new Coordination(60, 20));
            var snake = new Snake();

            new Thread(delegate() { snake.Start(ref running, boundary); }).Start();

            while (running)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                    {
                        snake.Direction = Direction.Up;
                        break;
                    }
                    case ConsoleKey.DownArrow:
                    {
                        snake.Direction = Direction.Down;
                        break;
                    }
                    case ConsoleKey.LeftArrow:
                    {
                        snake.Direction = Direction.Left;
                        break;
                    }
                    case ConsoleKey.RightArrow:
                    {
                        snake.Direction = Direction.Right;
                        break;
                    }
                    case ConsoleKey.Escape:
                    {
                        running = false;
                        break;
                    }
                }
            }

            Console.Clear();
        }
    }
}