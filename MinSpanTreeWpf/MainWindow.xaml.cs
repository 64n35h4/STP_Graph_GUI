using SpanningTree.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpanningTree
{
    public partial class MainWindow : Window {
        private const double _diameter = 30;
        private const double _edgeLabelSize = 20;

        private const int _fontSize = 12;
        private const int _edgeFontSize = 8;

        private List<Node> _nodes;
        private List<Edge> _edges;
        private List<Edge> _brokenEdges;
        private List<Edge> _mst;
        private List<Edge> _NOTmst;
        private List<MyEdge> _OKedges;

        private Node _edgeNode1, _edgeNode2;
        private SolidColorBrush _unvisitedBrush, _visitedBrush;
        private char _count;
        private bool tmp = false;
        private double totalCost = 0.0;
        private int edgeLabel = 0;
        private bool minTreePressed = false;
        private int min = int.MaxValue;

        public MainWindow() {
            InitializeComponent();

            drawingCanvas.SetValue(Canvas.ZIndexProperty, 0);

            _nodes = new List<Node>();
            _edges = new List<Edge>();
            _brokenEdges = new List<Edge>();
            _mst = new List<Edge>();
            _NOTmst = new List<Edge>();
            _OKedges = new List<MyEdge>();

            _count = 'A';

            _unvisitedBrush = new SolidColorBrush(Colors.Black);
            _visitedBrush = new SolidColorBrush(Colors.Red);
        }
       
        private void drawingCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
            if (minTreePressed)
                MessageBox.Show("Can't interact with program while minimal tree is displayed.\nClick on \"Restart\" or \"Clear\" to set a broken interface or to add nodes.", "Click on \"Restart\" or \"Clear\" first", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    Point clickPoint = e.GetPosition(drawingCanvas);

                    if (HasClickedOnNode(clickPoint.X, clickPoint.Y))
                    {
                        AssignEndNodes(clickPoint.X, clickPoint.Y);
                        if (_edgeNode1 != null && _edgeNode2 != null)
                        {
                            //build an edge
                            MyEdge edge = CreateEdge(_edgeNode1, _edgeNode2, 1.0, edgeLabel, 1.0);
                            _edgeNode1.Edges.Add(edge);     //add edge to List<MyEdge> on Node class
                            _edgeNode2.Edges.Add(edge);     //add edge to List<MyEdge> on Node class
                            _edges.Add(edge);
                            _OKedges.Add(edge);
                            PaintEdge(edge);
                            edgeLabel++;
                            ClearEdgeNodes();
                        }
                    }
                    else if (tmp)
                    {
                        tmp = false;
                    }
                    else
                    {
                        if (!OverlapsNode(clickPoint))
                        {
                            Node n = CreateNode(clickPoint);
                            _nodes.Add(n);
                            PaintNode(n);
                            _count++;
                            ClearEdgeNodes();
                        }
                    }
                }
            }
        }

        /// A method to detect whether the user has clicked on a node
        /// used either for edge creation or for indicating the end-points for which to find
        /// the minimum distance
        private bool HasClickedOnNode(double x, double y) {
            bool rez = false;
            for (int i = 0; i < _nodes.Count; i++) {
                if (_nodes[i].HasPoint(new Point(x, y))) {
                    rez = true;
                    break;
                }
            }
            return rez;
        }

        /// Get a node at a specific coordinate
        private Node GetNodeAt(double x, double y) {
            Node rez = null;
            for (int i = 0; i < _nodes.Count; i++) {
                if (_nodes[i].HasPoint(new Point(x, y))) {
                    rez = _nodes[i];
                    break;
                }
            }
            return rez;
        }
        /// Upon the creation of a new node,
        /// make sure that it is not overlapping an existing node
        private bool OverlapsNode(Point p) {
            bool rez = false;
            double distance;
            for (int i = 0; i < _nodes.Count; i++) {
                distance = GetDistance(p, _nodes[i].Center);
                if (distance < _diameter) {
                    rez = true;
                    break;
                }
            }
            return rez;
        }

        /// <returns>The distance between the two points</returns>
        private double GetDistance(Point p1, Point p2) {
            double xSq = Math.Pow(p1.X - p2.X, 2);
            double ySq = Math.Pow(p1.Y - p2.Y, 2);
            double dist = Math.Sqrt(xSq + ySq);

            return dist;
        }

        private void AssignEndNodes(double x, double y) {
            Node currentNode = GetNodeAt(x, y);
            if (currentNode != null) {
                if (_edgeNode1 == null) {
                    _edgeNode1 = currentNode;
                    statusLabel.Content = "You have selected node " + currentNode.Label + ". Please select another node.";
                }
                else {
                    if (currentNode != _edgeNode1) {
                        _edgeNode2 = currentNode;
                        statusLabel.Content = "Click on the canvas to create a node.";
                    }
                }
            }
        }

        /// Create a new node using the coordinates specified by a point
        private Node CreateNode(Point p) {
            double nodeCenterX = p.X - _diameter / 2;
            double nodeCenterY = p.Y - _diameter / 2;
            Node newNode = new Node(new Point(nodeCenterX, nodeCenterY), p, _count.ToString(), _diameter,_count.ToString(),0);
            return newNode;
        }

        /// Paint a single node on the canvas
        private void PaintNode(Node node) {
            //paint the node
            Ellipse ellipse = new Ellipse();
            if (node.Visited)
                ellipse.Fill = _visitedBrush;
            else
                ellipse.Fill = _unvisitedBrush;

            ellipse.Width = _diameter;
            ellipse.Height = _diameter;

            ellipse.SetValue(Canvas.LeftProperty, node.Location.X);
            ellipse.SetValue(Canvas.TopProperty, node.Location.Y);
            ellipse.SetValue(Canvas.ZIndexProperty, 2);
            //add to the canvas
            drawingCanvas.Children.Add(ellipse);
            //paint the node label 
            TextBlock tb = new TextBlock();
            tb.Text = node.Label;
            tb.Foreground = Brushes.White;
            tb.FontSize = _fontSize;
            tb.TextAlignment = TextAlignment.Center;
            tb.SetValue(Canvas.LeftProperty, node.Center.X - (_fontSize / 4 * node.Label.Length));
            tb.SetValue(Canvas.TopProperty, node.Center.Y - _fontSize / 2);
            tb.SetValue(Canvas.ZIndexProperty, 3);
            //add to canvas on top of the cirle
            drawingCanvas.Children.Add(tb);
        }

        private MyEdge CreateEdge(Node node1, Node node2, double distance, int edgeLabel, double op) {
            return new MyEdge(node1, node2, distance, edgeLabel, op);
        }

        private void PaintEdge(MyEdge edge) {
            //draw the edge
            Line line = new Line();
            line.X1 = edge.FirstNode.Center.X;
            line.X2 = edge.SecondNode.Center.X;

            line.Y1 = edge.FirstNode.Center.Y;
            line.Y2 = edge.SecondNode.Center.Y;
            line.Opacity = edge.Opacity;
           
            if (edge.Visited)
                line.Stroke = _visitedBrush;
            else
                line.Stroke = _unvisitedBrush;

            Point edgeLabelPoint = GetEdgeLabelCoordinate(edge.FirstNode, edge.SecondNode);
            Point edgeLabelPointB = GetEdgeLabelCoordinate(edge.SecondNode, edge.FirstNode);
            MyLine lineMy = new MyLine(edge, line, edgeLabelPoint, edgeLabelPointB);
            if(!minTreePressed)
                printTextBlock(edge,line, lineMy);

            line.StrokeThickness = 10;
            line.SetValue(Canvas.ZIndexProperty, 1);
            drawingCanvas.Children.Add(line);
        }

        private void printTextBlock(MyEdge edge,Line line,MyLine l)
        {
            l.TagA.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                if (l.TagA.Opacity == 1)
                {
                    if (l.TagB.Opacity == 1)
                    {
                        edge.Opacity = 0.5;
                        l.TagA.Opacity = 0.5;
                        line.Opacity = 0.5;
                        _brokenEdges.Add(edge);     //add the edge to the broken ways
                        _OKedges.Remove(edge);        //add the edge to the possible ways
                    }
                    else
                    {
                        l.TagA.Opacity = 0.5;
                    }

                }
                else if (l.TagA.Opacity == 0.5)
                {
                    if (l.TagB.Opacity == 1)
                    {
                        l.TagA.Opacity = 1;
                        line.Opacity = 1;           //set line as OK
                        edge.Opacity = 1;
                        _OKedges.Add(edge);           //add the edge to the possible ways
                        _brokenEdges.Remove(edge);  //remove the edge from the broken edges
                    }
                    else
                    {
                        l.TagA.Opacity = 1;
                    }

                }
                tmp = true;
            };

            l.TagB.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                if (l.TagB.Opacity == 1)
                {
                    if (l.TagA.Opacity == 1)
                    {
                        edge.Opacity = 0.5;
                        l.TagB.Opacity = 0.5;
                        line.Opacity = 0.5;
                        _brokenEdges.Add(edge);     //add the edge to the broken ways
                        _OKedges.Remove(edge);        //add the edge to the possible ways
                    }
                    else
                    {
                        l.TagB.Opacity = 0.5;
                    }

                }
                else if (l.TagB.Opacity == 0.5)
                {
                    if (l.TagA.Opacity == 1)
                    {
                        l.TagB.Opacity = 1;
                        line.Opacity = 1;           //set line as OK
                        edge.Opacity = 1;
                        _OKedges.Add(edge);           //add the edge to the possible ways
                        _brokenEdges.Remove(edge);  //remove the edge from the broken edges
                    }
                    else
                    {
                        l.TagB.Opacity = 1;
                    }

                }
                tmp = true;
            };
            l.TagA.Content = edge.FirstNode.iFace.ToString(); // edge.Length.ToString();
            edge.FirstNode.iFace++;

            l.TagB.Content = edge.SecondNode.iFace.ToString();
            edge.SecondNode.iFace++;

            l.TagA.Foreground = Brushes.White;
                l.TagB.Foreground = Brushes.White;

            l.TagA.Background = _unvisitedBrush;
                l.TagB.Background = _unvisitedBrush;

            l.TagA.Padding = new Thickness(5);
            l.TagA.FontSize = _edgeFontSize;

            l.TagB.Padding = new Thickness(5);
            l.TagB.FontSize = _edgeFontSize;

            l.TagA.MinWidth = l.TagA.MinHeight = _edgeLabelSize;

            l.TagB.MinWidth = l.TagB.MinHeight = _edgeLabelSize;

            l.TagA.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            l.TagA.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            l.TagB.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            l.TagB.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            drawingCanvas.Children.Add(l.TagA);
            drawingCanvas.Children.Add(l.TagB);
        }

        private void moveCord(object sender, MouseEventArgs e)
        {
            cordLabel.Content = e.GetPosition(drawingCanvas);
        }
       
        private Point GetEdgeLabelCoordinate(Node node1, Node node2) {
            double x = node1.Location.X;
            double y = node1.Location.Y;

            Vector px = new Vector();
            Vector py = new Vector();
            px.X = node2.Location.X; px.Y = node2.Location.Y;
            py.X = node1.Location.X; py.Y = node1.Location.Y;
            var dist = px - py;
            dist.Normalize();
            x = 25*dist.X + node1.Location.X;
            y = 25*dist.Y + node1.Location.Y;
            
            return new Point(x+5, y+5);
        }

        private void ClearEdgeNodes(){
            _edgeNode1 = _edgeNode2 = null;
        }

        private void findMinSpanTreeBtn_Click(object sender, RoutedEventArgs e){
            statusLabel.Content = "Calculating...";
            LaunchMinSpanTreeTask();
        }

        private void LaunchMinSpanTreeTask(){
            Task.Factory.StartNew(() =>
                FindMinSpanTree()
                )
                .ContinueWith((task) =>
                {
                    drawingCanvas.Children.Clear();
                    minTreePressed = true;
                    PaintAllNodes();
                    foreach (Node n in _nodes)
                        n.iFace = 1;
                    _NOTmst = getDiffrence(_OKedges, _mst);
                    foreach (MyEdge e in _mst)
                    {
                        e.Visited = true;
                        PaintEdge(e);
                        PaintNode(e.FirstNode);
                        PaintNode(e.SecondNode);
                    }
                    foreach (MyEdge e in _NOTmst)
                    {
                        e.Visited = false;
                        PaintEdge(e);
                    }
                    foreach (MyEdge e in _brokenEdges)
                    {
                        e.Visited = false;
                        PaintEdge(e);
                    }

                    statusLabel.Content = "Total cost: " + task.Result.ToString("n0");
                    findMinSpanTreeBtn.IsEnabled = false;
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// The method for finding the min span tree        
        private double FindMinSpanTree()
        {
            Node first, second;
            string root = "Z";
            var nodeArray = _nodes.ToArray();
            //initialize each node dist to 0 and set it's root to himself
            for (int i = 0; i < nodeArray.Length; i++)
            {
                nodeArray[i].Distance = 0;
                nodeArray[i].Root = nodeArray[i].Label;
            }        
            //assigning roots to nodes, and finding distance from each node to root.
            for (int i = 0; i < _OKedges.ToArray().Length; i++)
            {
                foreach (MyEdge currentEdge in _OKedges)
                {
                    first = currentEdge.FirstNode;
                    second = currentEdge.SecondNode;
                    if (second.Root.CompareTo(first.Root) > 0)   //if rootA > rootB ==> -1
                    {
                        second.Root = first.Root;
                        second.Distance = first.Distance + 1;

                    }
                    else if (second.Root.CompareTo(first.Root) < 0)
                    {
                        first.Root = second.Root;
                        first.Distance = second.Distance + 1;
                    }
                    else
                    {
                        if (first.Distance < second.Distance)
                            second.Distance = first.Distance + 1;
                        else if (first.Distance > second.Distance)
                            first.Distance = second.Distance + 1;
                    }
                }
            }

            //finding the min distance for each node
            for (int i = nodeArray.Length-1; i >= 0; i--)
            {
                root = "Z";             //assuming no more then 'Z' nodes
                foreach(MyEdge currentEdge in getSame(nodeArray[i].Edges, _OKedges))
                {
                    first = currentEdge.FirstNode;
                    second = currentEdge.SecondNode;
                    if(first.Label != nodeArray[i].Label)
                    {
                        first = currentEdge.SecondNode;
                        second = currentEdge.FirstNode;
                    }
                    if(second.Distance < min)
                    {
                        min = second.Distance;
                        root = second.Label;
                    }
                    else if(second.Distance == min && (second.Label.CompareTo(root) < 0))
                    {
                        min = second.Distance;
                        root = second.Label;
                    }
                }

                //setting this min edge node as path to root
                foreach (MyEdge currentEdge in getSame(nodeArray[i].Edges, _OKedges))
                {
                    first = currentEdge.FirstNode;
                    second = currentEdge.SecondNode;
                    if (first.Label != nodeArray[i].Label)
                    {
                        first = currentEdge.SecondNode;
                        second = currentEdge.FirstNode;
                    }
                    if (second.Label == root && second.Distance==min)
                    {
                        first.Visited = true;
                        second.Visited = true;
                        _mst.Add(currentEdge);
                        totalCost += currentEdge.Length;
                    }
                }
                min = int.MaxValue;
              }
            if (totalCost == 0.0)   //fix
                return 0;
            return totalCost-1;
        }

        private void PaintAllNodes(){
            foreach (Node n in _nodes)
                PaintNode(n);
        }

        private void PaintAllEdges(){
            foreach (MyEdge e in _edges)
                PaintEdge(e);
        }

        private void Clear(){
            this._nodes.Clear();
            this._edges.Clear();
            this._OKedges.Clear();
            this._brokenEdges.Clear();
            this._mst.Clear();
            this._count = 'A';
            this.totalCost = 0.0;
            this.edgeLabel = 0;
            this.minTreePressed = false;
            this.min = int.MaxValue;
        }

        private void Restart(){
            foreach (Node n in _nodes)
                n.iFace = 1;
            this.minTreePressed = false;
            this.totalCost = 0.0;
            this._mst.Clear();
            this.min = int.MaxValue;

            foreach (Node n in _nodes){
                n.Visited = false;
            }
            foreach (MyEdge e in _brokenEdges)        //adding all the broken ways to OK after restart
                _OKedges.Add(e);
            _brokenEdges.Clear();
            foreach (MyEdge e in _edges)
            {
                e.Visited = false;
                e.Opacity = 1.0;
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e){
            ExampleBtn.IsEnabled = true;
            findMinSpanTreeBtn.IsEnabled = true;
            Clear();
            statusLabel.Content = "Total cost: 0";
            drawingCanvas.Children.Clear();
        }

        private void restartBtn_Click(object sender, RoutedEventArgs e){
            findMinSpanTreeBtn.IsEnabled = true;
            Restart();
            drawingCanvas.Children.Clear();
            PaintAllNodes();
            PaintAllEdges();
            statusLabel.Content = "Total cost: 0";
        }

        private void ExampleBtn_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            drawingCanvas.Children.Clear();
            Point p = new Point();
            p.X = 305; p.Y = 25;
            Node n1 = CreateNode(p); n1.Label = _count.ToString();
            _nodes.Add(n1); _count++;
            PaintNode(n1);
            p.X = 220; p.Y = 135;
            Node n2 = CreateNode(p); n2.Label = _count.ToString();
            _nodes.Add(n2); _count++;
            PaintNode(n2);
            p.X = 220; p.Y = 285;
            Node n3 = CreateNode(p); n3.Label = _count.ToString();
            _nodes.Add(n3); _count++;
            PaintNode(n3);
            p.X = 305; p.Y = 210;
            Node n4 = CreateNode(p); n4.Label = _count.ToString();
            _nodes.Add(n4); _count++;
            PaintNode(n4);
            p.X = 390; p.Y = 285;
            Node n5 = CreateNode(p); n5.Label = _count.ToString();
            _nodes.Add(n5); _count++;
            PaintNode(n5);
            p.X = 390; p.Y = 135;
            Node n6 = CreateNode(p); n6.Label = _count.ToString();
            _nodes.Add(n6); _count++;
            PaintNode(n6);

            PrintMyEdge(n1, n2);
            PrintMyEdge(n2, n3);
            PrintMyEdge(n3, n5);
            PrintMyEdge(n5, n6);
            PrintMyEdge(n2, n4);
            PrintMyEdge(n3, n4);
            PrintMyEdge(n4, n5);
            PrintMyEdge(n4, n6);
            PrintMyEdge(n1, n6);
            PrintMyEdge(n2, n6);
            ExampleBtn.IsEnabled = false;
        }

        private void PrintMyEdge(Node n1, Node n2)
        {
            edgeLabel = 0;
            MyEdge edge = CreateEdge(n1, n2, 1.0, edgeLabel, 1.0);
            n1.Edges.Add(edge);
            n2.Edges.Add(edge);
            _edges.Add(edge);
            _OKedges.Add(edge);
            PaintEdge(edge);
            edgeLabel++;
            ClearEdgeNodes();
        }

        private List<Edge> getDiffrence(List<MyEdge> _edges, List<Edge> _mst)
        {
            List<Edge> _list = new List<Edge>();
            foreach (MyEdge e in _edges)
            {
                if (!_mst.Contains(e))
                {
                    _list.Add(e);
                }
            }
            return _list;
        }

        private List<Edge> getSame(List<MyEdge> _smaller, List<MyEdge> _larger)
        {
            List<Edge> _list = new List<Edge>();
            foreach (MyEdge e in _smaller)
            {
                if (_larger.Contains(e))
                {
                    _list.Add(e);
                }
            }
            return _list;
        }
    }
}
