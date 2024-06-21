using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GW_1
{
    public class ProcessorElementUI
    {
        public enum Direction
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        public enum FlowDataDirection
        {
            [Description("")]
            None,
            [Description("a")]
            InputA,
            [Description("b")]
            InputB,
            [Description("c")]
            InputC,
            [Description("d")]
            InputD,
            [Description("a'")]
            OutputA,
            [Description("b'")]
            OutputB,
            [Description("c'")]
            OutputC,
            [Description("d'")]
            OutputD
        }

        public Direction LeftTopArrow { get; set; }
        public Direction LeftBottomArrow { get; set; }
        public Direction RightTopArrow { get; set; }
        public Direction RightBottomArrow { get; set; }
        public Direction TopLeftArrow { get; set; }
        public Direction TopRightArrow { get; set; }
        public Direction BottomLeftArrow { get; set; }
        public Direction BottomRightArrow { get; set; }

        public FlowDataDirection LeftTopArrowPath { get; set; }
        public FlowDataDirection LeftBottomArrowPath { get; set; }
        public FlowDataDirection RightTopArrowPath { get; set; }
        public FlowDataDirection RightBottomArrowPath { get; set; }
        public FlowDataDirection TopLeftArrowPath { get; set; }
        public FlowDataDirection TopRightArrowPath { get; set; }
        public FlowDataDirection BottomLeftArrowPath { get; set; }
        public FlowDataDirection BottomRightArrowPath { get; set; }

        public int DelayTakts { get; set; }

        public ProcessorElementUI()
        {
            InitializeDirections();
            InitializePaths();
            DelayTakts = 0;
        }

        private void InitializeDirections()
        {
            LeftTopArrow = Direction.None;
            LeftBottomArrow = Direction.None;
            RightTopArrow = Direction.None;
            RightBottomArrow = Direction.None;
            TopLeftArrow = Direction.None;
            TopRightArrow = Direction.None;
            BottomLeftArrow = Direction.None;
            BottomRightArrow = Direction.None;
        }

        private void InitializePaths()
        {
            LeftTopArrowPath = FlowDataDirection.None;
            LeftBottomArrowPath = FlowDataDirection.None;
            RightTopArrowPath = FlowDataDirection.None;
            RightBottomArrowPath = FlowDataDirection.None;
            TopLeftArrowPath = FlowDataDirection.None;
            TopRightArrowPath = FlowDataDirection.None;
            BottomLeftArrowPath = FlowDataDirection.None;
            BottomRightArrowPath = FlowDataDirection.None;
        }

        public void ProcessData()
        {
            var arrowPaths = new Dictionary<(Direction, string), FlowDataDirection>
            {
                { (Direction.Left, "LeftTop"), FlowDataDirection.OutputA },
                { (Direction.Right, "LeftTop"), FlowDataDirection.InputA },
                { (Direction.Right, "RightTop"), FlowDataDirection.OutputA },
                { (Direction.Left, "RightTop"), FlowDataDirection.InputA },
                { (Direction.Left, "LeftBottom"), FlowDataDirection.OutputC },
                { (Direction.Right, "LeftBottom"), FlowDataDirection.InputC },
                { (Direction.Right, "RightBottom"), FlowDataDirection.OutputC },
                { (Direction.Left, "RightBottom"), FlowDataDirection.InputC },
                { (Direction.Down, "TopLeft"), FlowDataDirection.InputB },
                { (Direction.Up, "TopLeft"), FlowDataDirection.OutputB },
                { (Direction.Down, "BottomLeft"), FlowDataDirection.OutputB },
                { (Direction.Up, "BottomLeft"), FlowDataDirection.InputB },
                { (Direction.Down, "TopRight"), FlowDataDirection.InputD },
                { (Direction.Up, "TopRight"), FlowDataDirection.OutputD },
                { (Direction.Down, "BottomRight"), FlowDataDirection.OutputD },
                { (Direction.Up, "BottomRight"), FlowDataDirection.InputD },
            };

            UpdateArrowPaths(arrowPaths);
        }

        private void UpdateArrowPaths(Dictionary<(Direction, string), FlowDataDirection> arrowPaths)
        {
            LeftTopArrowPath = GetArrowPath(arrowPaths, LeftTopArrow, "LeftTop");
            LeftBottomArrowPath = GetArrowPath(arrowPaths, LeftBottomArrow, "LeftBottom");
            RightTopArrowPath = GetArrowPath(arrowPaths, RightTopArrow, "RightTop");
            RightBottomArrowPath = GetArrowPath(arrowPaths, RightBottomArrow, "RightBottom");
            TopLeftArrowPath = GetArrowPath(arrowPaths, TopLeftArrow, "TopLeft");
            TopRightArrowPath = GetArrowPath(arrowPaths, TopRightArrow, "TopRight");
            BottomLeftArrowPath = GetArrowPath(arrowPaths, BottomLeftArrow, "BottomLeft");
            BottomRightArrowPath = GetArrowPath(arrowPaths, BottomRightArrow, "BottomRight");
        }

        private FlowDataDirection GetArrowPath(Dictionary<(Direction, string), FlowDataDirection> arrowPaths, Direction direction, string position)
        {
            return arrowPaths.TryGetValue((direction, position), out var path) ? path : FlowDataDirection.None;
        }
    }
}
