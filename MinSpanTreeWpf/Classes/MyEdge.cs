using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SpanningTree.Classes
{
    public class MyEdge : Edge
    {
        private int _label;
        private double _opacity;
        private Label _tbA, _tbB;

        public MyEdge(Node firstnode, Node secondnode):base(firstnode, secondnode){}
        public MyEdge(Node firstnode, Node secondnode, double length):base(firstnode, secondnode, length){}
        public MyEdge(Node firstnode, Node secondnode, double length, int edgeLabel, double opa) : 
            base(firstnode, secondnode, length){
            this._label = edgeLabel;
            this._opacity = opa;
        }
        public int Label
        {
            get { return this._label; }
            set { this._label = value; }
        }

        public double Opacity
        {
            get { return this._opacity; }
            set { this._opacity = value; }
        }
        public Label TagA
        {
            get { return this._tbA; }
            set { this._tbA = value; }
        }
        public Label TagB
        {
            get { return this._tbB; }
            set { this._tbB = value; }
        }
    }
}
