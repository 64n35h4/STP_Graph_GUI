using System;
using System.Collections.Generic;
using System.Windows;

namespace SpanningTree.Classes
{
    public class Node
    {
        private Point _drawingLocation;
        private Point _center;
        private string _label;
        private double _diameter;
        private const int _fontSize = 10;
        
        private bool _visited;

        private double _totalCost;
        private Edge _edgeVisitor;
        private int _iface = 1;
        private string _root;
        private List<MyEdge> _edges;
        private int _dist;

        public Node(Point location, string label, double diameter)
        {
            this._drawingLocation = location;
            this._label = label;
            this._diameter = diameter;
            _visited = false;
            this._edges = new List<MyEdge>();
        }

        public Node(Point location, Point center, string label, double diameter)
            :this(location,label,diameter)
        {
            this._center = center;
        }

        public Node(Point location, Point center, string label, double diameter, string root, int dist)
            : this(location, label, diameter)
        {
            this._center = center;
            this._root = root;
            this._dist = dist;
        }

        public Point Location
        {
            get { return _drawingLocation; }
        }

        public Point Center
        {
            get { return _center; }
            set { this._center = value; }
        }

        public double Diameter
        {
            get { return _diameter; }
        }

        public string Label
        {
            get { return _label; }
            set { this._label = value; }
        }

        public double TotalCost
        {
            get { return _totalCost; }
            set { this._totalCost = value; }
        }

        public Edge EdgeVisitor
        {
            get { return _edgeVisitor; }
            set { this._edgeVisitor = value; }
        }
        public int iFace
        {
            get { return _iface; }
            set { this._iface = value; }
        }

        /// Calculates whether the node contains a specific point.
        public bool HasPoint(Point p)
        {
            double xSq = Math.Pow(p.X - _center.X,2);
            double ySq = Math.Pow(p.Y - _center.Y,2);
            double dist = Math.Sqrt(xSq + ySq);

            return (dist <= (_diameter/2));
        }

        public bool Visited
        {
            get { return _visited; }
            set { this._visited = value; }
        }

        public string Root
        {
            get { return _root; }
            set { this._root = value; }
        }

        public List<MyEdge> Edges
        {
            get { return _edges; }
            set { this._edges = value; }
        }

        public int Distance
        {
            get { return _dist; }
            set { this._dist = value; }
        }
    }
}
