namespace HandyScreenshot.Interop
{
    public static class WindowInterop
    {
        public static NativeMethods.POINT GetMousePosition()
        {
            var position = new NativeMethods.POINT();
            NativeMethods.GetCursorPos(ref position);
            return position;
        }
    }
}
