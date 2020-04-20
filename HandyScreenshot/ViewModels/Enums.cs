namespace HandyScreenshot.ViewModels
{
    public enum ClipBoxStatus
    {
        AutoDetect,
        ResizingVertex,
        ResizingLeftEdge,
        ResizingTopEdge,
        ResizingRightEdge,
        ResizingBottomEdge,
        Static,
        Moving,
    }

    /// <summary>
    /// The position of the point relative to the rectangle.
    /// </summary>
    public enum PointRectPosition
    {
        Internal,
        Left,
        LeftTop,
        Top,
        RightTop,
        Right,
        RightBottom,
        Bottom,
        LeftBottom
    }
}
