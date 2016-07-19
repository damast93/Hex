using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using static System.Math;
using static System.Linq.Enumerable;


namespace Hex
{
    public partial class MainWindow : Window
    {

        public static readonly int n = 11;

        private static readonly double s = Sqrt(3) / 2.0;
        private static readonly double d = 10;
        private static readonly double r = d / (2.0 * s);

        public static readonly double w = 2;
        private static readonly double degs = PI / 180.0;
        private static readonly double cx = r * Tan(60.0 * degs);
        private static readonly double cy = cx * Tan(30.0 * degs);
        private static readonly double dx2 = r * Tan(30.0 * degs);

        private static readonly Random rnd = new Random();

        private enum Turn
        {
            RedTurn, BlueTurn
        }

        private enum CellState
        {
            RedPlayer, BluePlayer, Empty
        }

        private static Brush Highlight(CellState st)
        {
            switch (st)
            {
                case CellState.RedPlayer:
                    return Brushes.Red;
                case CellState.BluePlayer:
                    return Brushes.Blue;
                default:
                    return Brushes.White;
            }
        }

        private static Brush Soft(CellState st)
        {
            switch (st)
            {
                case CellState.RedPlayer:
                    return Brushes.LightPink;
                case CellState.BluePlayer:
                    return Brushes.LightBlue;
                default:
                    return Brushes.White;
            }
        }

        private class Cell
        {
            private CellState m_State;
            public int I { get; }
            public int J { get; }
            public Polygon Hexagon { get; set; }

            public CellState State
            {
                get { return m_State; }
                set
                {
                    m_State = value;
                    Hexagon.Fill = Highlight(m_State);
                }
            }
            
            public Cell(int i, int j, Polygon hexagon)
            {
                I = i;
                J = j;
                Hexagon = hexagon;
                State = CellState.Empty;
            }
        }

        private Turn m_Turn;
        private Turn m_StartingTurn;

        private Turn CurrentTurn
        {
            get { return m_Turn; }
            set
            {
                m_Turn = value;
                Canvas.Background = m_Turn == Turn.BlueTurn ? Brushes.LightBlue : Brushes.LightPink;
            }
        }

        private bool IsValidIJ(int i, int j)
        {
            return (i >= 0) && (i < n) && (j >= 0) && (j < n);
        }

        private IEnumerable<Cell> Neighbours(Cell c)
        {
            int i = c.I, j = c.J;
            if (IsValidIJ(i - 1, j)) yield return cells[i - 1, j];
            if (IsValidIJ(i - 1, j + 1)) yield return cells[i - 1, j + 1];
            if (IsValidIJ(i , j + 1)) yield return cells[i, j + 1];
            if (IsValidIJ(i + 1, j)) yield return cells[i + 1, j];
            if (IsValidIJ(i + 1, j - 1)) yield return cells[i + 1, j - 1];
            if (IsValidIJ(i, j - 1)) yield return cells[i, j - 1];
        }

        private Cell[,] cells;

        public MainWindow()
        {
            InitializeComponent();

            m_StartingTurn  = rnd.Next() % 2 == 0 ? Turn.BlueTurn : Turn.RedTurn;
            CurrentTurn = m_StartingTurn;

            Canvas.Width = rhombic(n - 1, n - 1).X + w*cx;
            Canvas.Height = rhombic(n - 1, n - 1).Y + w*cy;
            
            var tl = new Point(0, 0);
            var bl = new Point(rhombic(0, n - 1).X - w*dx2, rhombic(0, n - 1).Y + w*r);
            var br = new Point(rhombic(n - 1, n - 1).X + w*cx, rhombic(n - 1, n - 1).Y + w*cy);
            var tr = new Point(rhombic(n - 1, 0).X + w*dx2, 0);

            var m = new Point((tl.X + br.X) / 2, (tl.Y + br.Y) / 2);

            Action<Point, Point, Brush> addBoundary = (p1, p2, color) =>
            {
                var tri = new Polygon();
                tri.Points.Add(p1); tri.Points.Add(m); tri.Points.Add(p2);
                tri.Fill = color;
                tri.Stroke = Brushes.Black;
                Canvas.Children.Add(tri);
            };

            addBoundary(tl, bl, Brushes.Red);
            addBoundary(tr, br, Brushes.Red);
            addBoundary(tl, tr, Brushes.Blue);
            addBoundary(bl, br, Brushes.Blue);

            cells = new Cell[n, n];

            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    var newCell = new Cell(i, j, HexagonV(rhombic(i, j)));
                    Canvas.Children.Add(newCell.Hexagon);

                    int itmp = i, jtmp = j;
                    newCell.Hexagon.MouseDown += (e, a) => Cell_Click(itmp, jtmp);

                    cells[i, j] = newCell;
                }
            }
            
        }
        
        private void Cell_Click(int i, int j)
        {
            var cell = cells[i, j];
            if (cell.State != CellState.Empty) return;

            cell.State = CurrentTurn == Turn.BlueTurn ? CellState.BluePlayer : CellState.RedPlayer;
            CurrentTurn = CurrentTurn == Turn.BlueTurn ? Turn.RedTurn : Turn.BlueTurn;

            var redL = from k in Range(0, n) let c = cells[0,k] where c.State == CellState.RedPlayer select c;
            var redR = from k in Range(0, n) let c = cells[n - 1, k] where c.State == CellState.RedPlayer select c;

            var blueT = from k in Range(0, n) let c = cells[k, 0] where c.State == CellState.BluePlayer select c;
            var blueB = from k in Range(0, n) let c = cells[k, n - 1] where c.State == CellState.BluePlayer select c;

            var winningRPath = Algorithms.BFS(redL, redR, cl => Neighbours(cl).Where(c => c.State == CellState.RedPlayer));
            var winningBPath = Algorithms.BFS(blueT, blueB, cl => Neighbours(cl).Where(c => c.State == CellState.BluePlayer));

            if (winningRPath != null)
            {
                foreach (var c in cells)
                    c.Hexagon.Fill = Soft(c.State);

                foreach (var c in winningRPath)
                    c.Hexagon.Fill = Brushes.Red;

                MessageBox.Show(
                    $"Red player won with a length {winningRPath.Count()} path.",
                    "Congratulations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                
                m_StartingTurn = Turn.BlueTurn;
                CurrentTurn = m_StartingTurn;
                foreach (var c in cells) c.State = CellState.Empty;
            }
            if (winningBPath != null)
            {
                foreach (var c in cells)
                    c.Hexagon.Fill = Soft(c.State);

                foreach (var c in winningBPath)
                    c.Hexagon.Fill = Brushes.Blue;

                MessageBox.Show(
                    $"Blue player won with a length {winningBPath.Count()} path.",
                    "Congratulations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                m_StartingTurn = Turn.RedTurn;
                CurrentTurn = m_StartingTurn;
                foreach (var c in cells) c.State = CellState.Empty;
            }
        }

        private static Point rhombic(int i, int j)
        {
            var dx = d;
            var dy = s * d;

            return new Point(w*cx + i * dx + (d / 2) * j, w*cy + j * dy);       
        }

        private static Point hexAngle(Point c, int i)
        {
            return new Point(
                    c.X + r * Cos((90.0 + i * 60.0) * degs),
                    c.Y + r * Sin((90.0 + i * 60.0) * degs)
                );
        }

        private static Polygon HexagonV(Point c)
        {
            var hex = new Polygon();

            for (int i = 0; i < 6; ++i)
            {
                var vertex = hexAngle(c, i);
                hex.Points.Add(vertex);
            }

            hex.Stroke = Brushes.Black;
            hex.StrokeThickness = 0.55;

            hex.Fill = Brushes.White;
            
            return hex;    
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Connect the sides of your color with each other before the other player does.",
                "Game of Hex", 
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Really start a new game?",
                "Game of Hex",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            if (res == MessageBoxResult.Yes)
            {
                foreach (var cell in cells)
                    cell.State = CellState.Empty;

                m_StartingTurn = m_StartingTurn == Turn.RedTurn ? Turn.BlueTurn : Turn.RedTurn;
                CurrentTurn = m_StartingTurn;
            }
        }
    }
    
}
