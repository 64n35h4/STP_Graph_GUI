using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading.Tasks;
using SpanningTree.Classes;

namespace SpanningTree{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window{
        private const double _diameter = 30;
        private const double _edgeLabelSize = 20;

        private const int _fontSize = 12;
        private const int _edgeFontSize = 10;

        private List<Node> _nodes;
        private List<Edge> _edges;
        private List<Edge> _brokenEdges;
        private List<Edge> _mst;
        private List<Cluster> _clusters;
        private List<Edge> _NOTmst;
        private List<Edge> _OKedges;

        private Node _edgeNode1, _edgeNode2;
        private SolidColorBrush _unvisitedBrush, _visitedBrush;
        private int _count;
        private bool tmp = false;
        private double totalCost = 0.0;
        private int edgeLabel = 0;
        private bool minTreePressed = false;


        public MainWindow(){
            InitializeComponent();

            drawingCanvas.SetValue(Canvas.ZIndexProperty, 0);

            _nodes = new List<Node>();
            _edges = new List<Edge>();
            _brokenEdges = new List<Edge>();
            _mst = new List<Edge>();
            _NOTmst = new List<Edge>();
            _OKedges = new List<Edge>();
            _clusters = new List<Cluster>();

            _count = 1;

            _unvisitedBrush = new SolidColorBrush(Colors.Black);
            _visitedBrush = new SolidColorBrush(Colors.Red);
        }

        private void drawingCanvas_MouseUp(object sender, MouseButtonEventArgs e){
            if (e.LeftButton == MouseButtonState.Released){
                Point clickPoint = e.GetPosition(drawingCanvas);

                if (HasClickedOnNode(clickPoint.X, clickPoint.Y)){
                    AssignEndNodes(clickPoint.X, clickPoint.Y);
                    if (_edgeNode1 != null && _edgeNode2 != null){
                        //build an edge
                        // distance = 1;// GetEdgeDistance();
                        //if (distance != 0.0){
                        MyEdge edge = CreateEdge(_edgeNode1, _edgeNode2, 1.0, edgeLabel, 1.0);
                        _edges.Add(edge);
                        _OKedges.Add(edge);
                        PaintEdge(edge);
                        edgeLabel++;
                        //}
                        ClearEdgeNodes();
                    }
                }
                else if (tmp){
                    tmp = false;
                }
                else{
                    if (!OverlapsNode(clickPoint)){
                        Node n = CreateNode(clickPoint);
                        _nodes.Add(n);
                        PaintNode(n);
                        _count++;
                        ClearEdgeNodes();
                    }
                }
            }
        }

        /// <summary>
        /// A method to detect whether the user has clicked on a node
        /// used either for edge creation or for indicating the end-points for which to find
        /// the minimum distance
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns>Whether a user is clicked on a existing node</returns>
        private bool HasClickedOnNode(double x, double y){
            bool rez = false;
            for (int i = 0; i < _nodes.Count; i++){
                if (_nodes[i].HasPoint(new Point(x,y))){
                    rez = true;
                    break;
                }
            }
            return rez;
        }

        /// <summary>
        /// Get a node at a specific coordinate
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns>The node that has been found or null if there is no node at the speicied coordinates</returns>
        private Node GetNodeAt(double x, double y){
            Node rez = null;
            for (int i = 0; i < _nodes.Count; i++){
                if (_nodes[i].HasPoint(new Point(x, y))){
                    rez = _nodes[i];
                    break;
                }
            }
            return rez;
        }
        /// <summary>
        /// Upon the creation of a new node,
        /// make sure that it is not overlapping an existing node
        /// </summary>
        /// <param name="p">A x,y point</param>
        /// <returns>Whether there is an overlap with an existing node</returns>
        private bool OverlapsNode(Point p){
            bool rez = false;
            double distance;
            for (int i = 0; i < _nodes.Count; i++){
                distance = GetDistance(p, _nodes[i].Center);
                if (distance < _diameter){
                    rez = true;
                    break;
                }
            }
            return rez;
        }

        /// <summary>
        /// Use an additional dialog window to get the distance
        /// for an edge as specified by the user
        /// </summary>
        /// <returns>The distance value specified by the user</returns>
        /// 
        /*private double GetEdgeDistance(){
            double distance = 0.0;
            DistanceDialog dd = new DistanceDialog();
            dd.Owner = this;

            dd.ShowDialog();
            distance = dd.Distance;

            return distance;
        }*/
        
        /// <summary>
        /// Calculate the Eucledean distance between two points
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns>The distance between the two points</returns>
        private double GetDistance(Point p1, Point p2){
            double xSq = Math.Pow(p1.X - p2.X, 2);
            double ySq = Math.Pow(p1.Y - p2.Y, 2);
            double dist = Math.Sqrt(xSq + ySq);

            return dist;
        }

        private void AssignEndNodes(double x, double y){
            Node currentNode = GetNodeAt(x, y);
            if (currentNode != null){
                if (_edgeNode1 == null){
                    _edgeNode1 = currentNode;
                    statusLabel.Content = "You have selected node " + currentNode.Label + ". Please select another node.";
                }
                else{
                    if (currentNode != _edgeNode1){
                        _edgeNode2 = currentNode;
                        statusLabel.Content = "Click on the canvas to create a node.";
                    }
                }
            }
        }

        /// <summary>
        /// Create a new node using the coordinates specified by a point
        /// </summary>
        /// <param name="p">A Point object that carries the coordinates for Node creation</param>
        /// <returns></returns>
        private Node CreateNode(Point p){
            double nodeCenterX = p.X - _diameter / 2;
            double nodeCenterY = p.Y - _diameter / 2;
            Node newNode = new Node(new Point(nodeCenterX, nodeCenterY), p, _count.ToString(), _diameter);
            Cluster c = new Cluster(newNode.Label);
            c.AddNode(newNode);
            newNode.Cluster = c;
            _clusters.Add(c);

            return newNode;
        }

        /// <summary>
        /// Paint a single node on the canvas
        /// </summary>
        /// <param name="node">A node object carrying the coordinates</param>
        private void PaintNode(Node node){
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

        private MyEdge CreateEdge(Node node1, Node node2, double distance, int edgeLabel, double op){
            return new MyEdge(node1, node2, distance, edgeLabel, op);
        }

        private void PaintEdge(MyEdge edge){
            //draw the edge
            Line line = new Line();
            line.X1 = edge.FirstNode.Center.X;
            line.X2 = edge.SecondNode.Center.X;

            line.Y1 = edge.FirstNode.Center.Y;
            line.Y2 = edge.SecondNode.Center.Y;
            line.Opacity = edge.Opacity;
            line.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                int something = edge.Label;
                if (minTreePressed)
                    MessageBox.Show("Can't place error on a link when MinTree is active.\nClick on \"Restart\" to set this link as broken.", "Error on run", MessageBoxButton.OK,MessageBoxImage.Exclamation);
                else
                {
                    if (line.Opacity == 0.5)
                    {
                        line.Opacity = 1;           //set line as OK
                        edge.Opacity = 1;
                        _OKedges.Add(edge);           //add the edge to the possible ways
                        _brokenEdges.Remove(edge);  //remove the edge from the broken edges

                    }
                    else if (line.Opacity == 1)
                    {
                        line.Opacity = 0.5;         //set line as broken
                        edge.Opacity = 0.5;
                        _brokenEdges.Add(edge);     //add the edge to the broken ways
                        _OKedges.Remove(edge);        //add the edge to the possible ways
                    }
                    tmp = true;
                }
                //if (minTreePressed)
                //{
                  //  RestartMinTree(line);
                    //restartBtn_Click(sender, e);
                    //findMinSpanTreeBtn_Click(sender, e);
                    //LaunchMinSpanTreeTask();
                //}
            };
            if (edge.Visited)
                line.Stroke = _visitedBrush;
            else
                line.Stroke = _unvisitedBrush;

            line.StrokeThickness = 7;
            line.SetValue(Canvas.ZIndexProperty, 1);
            drawingCanvas.Children.Add(line);

            //draw the distance label
            //Point edgeLabelPoint = GetEdgeLabelCoordinate(edge);
            /*TextBlock tb = new TextBlock();
            tb.Text = "1"; // edge.Length.ToString();
            tb.Foreground = Brushes.White;

            if (edge.Visited)
                tb.Background = _visitedBrush;
            else
                tb.Background = _unvisitedBrush;

            tb.Padding = new Thickness(5);
            tb.FontSize = _edgeFontSize;

            tb.MinWidth = _edgeLabelSize;
            tb.MinHeight = _edgeLabelSize;

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            tb.TextAlignment = TextAlignment.Center;

            tb.SetValue(Canvas.LeftProperty, edgeLabelPoint.X);
            tb.SetValue(Canvas.TopProperty, edgeLabelPoint.Y);
            tb.SetValue(Canvas.ZIndexProperty, 2);
            drawingCanvas.Children.Add(tb);*/
        }

        /// <summary>
        /// Calculate the coordinates where an edge label is to be drawn
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        /* private Point GetEdgeLabelCoordinate(Edge edge){
             double x = Math.Abs(edge.FirstNode.Location.X - edge.SecondNode.Location.X) / 2;
             double y = Math.Abs(edge.FirstNode.Location.Y - edge.SecondNode.Location.Y) / 2;

             if (edge.FirstNode.Location.X > edge.SecondNode.Location.X)
                 x += edge.SecondNode.Location.X;
             else
                 x += edge.FirstNode.Location.X;

             if (edge.FirstNode.Location.Y > edge.SecondNode.Location.Y)
                 y += edge.SecondNode.Location.Y;
             else
                 y += edge.FirstNode.Location.Y;

             return new Point(x, y);
         }*/

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
                    //List<Edge> temp = _mst;  
                    drawingCanvas.Children.Clear();
                    PaintAllNodes();
                
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
                    minTreePressed = true;
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// The method for finding the min span tree
        /// </summary>
        private double FindMinSpanTree(){
            //_mst.Clear();
            //the forest contains all visited nodes
            List<Node> forest = new List<Node>();
            //sort the edges by their length
            //double totalCost = 0.0;
            foreach (Edge currentEdge in _OKedges){
                if (_clusters.Count == 1)
                    break;

                Cluster cluster1 = currentEdge.FirstNode.Cluster;
                Cluster cluster2 = currentEdge.SecondNode.Cluster;

                if (cluster1.Label != cluster2.Label){
                    _mst.Add(currentEdge);
                    //add the length to the total cost
                    totalCost += currentEdge.Length;
                    currentEdge.FirstNode.Visited = true;
                    currentEdge.SecondNode.Visited = true;
                    //merge clusters in a single one
                    MergeClusters(cluster1, cluster2);
                }
            }

            return totalCost;
        }

        /// <summary>
        /// Merge two cluster and make a single one of them
        /// </summary>
        /// <param name="cluster1">First Cluster</param>
        /// <param name="cluster2">Second Cluster</param>
        private void MergeClusters(Cluster cluster1, Cluster cluster2){
            List<Node> nodeList = cluster2.GetNodeList();
            foreach (Node n in nodeList){
                cluster1.AddNode(n);
                n.Cluster = cluster1;
            }
            _clusters.Remove(cluster2);
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
            this._mst.Clear();
            this._clusters.Clear();
            this._count = 1;
            this.totalCost = 0.0;
            this.edgeLabel = 0;
            this.minTreePressed = false;
        }

        private void Restart(){
            this.minTreePressed = false;
            this.totalCost = 0.0;
            this._mst.Clear();
            this._clusters.Clear();

            foreach (Node n in _nodes){
                n.Visited = false;
                //create a new cluster
                n.Cluster = new Cluster(n.Label);
                //set the point to the cluster
                n.Cluster.AddNode(n);
                //add the node to the cluster
                _clusters.Add(n.Cluster);
            }
            foreach (Edge e in _brokenEdges)        //adding all the broken ways to OK after restart
                _OKedges.Add(e);
            _brokenEdges.Clear();
            foreach (MyEdge e in _edges)
            {
                e.Visited = false;
                e.Opacity = 1.0;
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e){
            Clear();
            drawingCanvas.Children.Clear();
        }

        private void restartBtn_Click(object sender, RoutedEventArgs e){
            Restart();
            drawingCanvas.Children.Clear();
            PaintAllNodes();
            PaintAllEdges();
        }

        private void problemBtn_Click(object sender, RoutedEventArgs e){
            DistanceDialog dd = new DistanceDialog();
            InitializeComponent();
            double distance;
            dd.Owner = this;

            dd.ShowDialog();
            distance = dd.Distance;
            //Clear();
            //drawingCanvas.Children.Clear();
        }

        private List<Edge> getDiffrence(List<Edge> _edges, List<Edge> _mst)
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
    }
}
