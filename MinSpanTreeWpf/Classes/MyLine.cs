using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpanningTree.Classes
{
    public class MyLine
    {
        private Label _lbl1;
        private Label _lbl2;

        public MyLine(MyEdge e, Line l, Point p1, Point p2){
            _lbl1 = new Label();
            _lbl2 = new Label();

            _lbl1.SetValue(Canvas.LeftProperty, p1.X);
            _lbl1.SetValue(Canvas.TopProperty, p1.Y);
            _lbl1.SetValue(Canvas.ZIndexProperty, 2);

            _lbl2.SetValue(Canvas.LeftProperty, p2.X);
            _lbl2.SetValue(Canvas.TopProperty, p2.Y);
            _lbl2.SetValue(Canvas.ZIndexProperty, 2);

        }
        public Label TagA
        {
            get { return this._lbl1; }
            set { this._lbl1 = value; }
        }
        public Label TagB
        {
            get { return this._lbl2; }
            set { this._lbl2 = value; }
        }
    }
}
