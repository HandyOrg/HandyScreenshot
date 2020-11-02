namespace HandyScreenshot.Controls
{
    public class MagnifierSizeSet
    {
        private const int BaseOffsetFromMouse = 20;

        private const int BaseRegionWidth = 19;
        private const int BaseRegionHeight = 13;
        private const int BaseCentralLength = 8;

        private const int BaseHalfRegionWidth = BaseRegionWidth / 2;
        private const int BaseHalfRegionHeight = BaseRegionHeight / 2;
        private const int BaseMagnifierWidth = BaseRegionWidth * BaseCentralLength;
        private const int BaseMagnifierHeight = BaseRegionHeight * BaseCentralLength;
        private const int BaseBaselineWidth = BaseCentralLength - 2;
        private const int BaseHorizontalBaselineOffset = (BaseMagnifierHeight - BaseCentralLength) / 2;
        private const int BaseVerticalBaselineOffset = (BaseMagnifierWidth - BaseCentralLength) / 2;
        private const int BaseLeftBaselineLength = BaseVerticalBaselineOffset - 1;
        private const int BaseRightBaselineLength = BaseVerticalBaselineOffset + 1;
        private const int BaseTopBaselineLength = BaseHorizontalBaselineOffset - 1;
        private const int BaseBottomBaselineLength = BaseHorizontalBaselineOffset + 1;

        public MagnifierSizeSet(double scale = 1D)
        {
            OffsetFromMouse = BaseOffsetFromMouse * scale;
            RegionWidth = BaseRegionWidth * scale;
            RegionHeight = BaseRegionHeight * scale;
            CentralLength = BaseCentralLength * scale;
            HalfRegionWidth = BaseHalfRegionWidth * scale;
            HalfRegionHeight = BaseHalfRegionHeight * scale;
            MagnifierWidth = BaseMagnifierWidth * scale;
            MagnifierHeight = BaseMagnifierHeight * scale;
            BaselineWidth = BaseBaselineWidth * scale;
            HorizontalBaselineOffset = BaseHorizontalBaselineOffset * scale;
            VerticalBaselineOffset = BaseVerticalBaselineOffset * scale;
            LeftBaselineLength = BaseLeftBaselineLength * scale;
            RightBaselineLength = BaseRightBaselineLength * scale;
            TopBaselineLength = BaseTopBaselineLength * scale;
            BottomBaselineLength = BaseBottomBaselineLength * scale;
        }

        public double OffsetFromMouse { get; }

        public double RegionWidth { get; }

        public double RegionHeight { get; }

        public double CentralLength { get; }

        public double HalfRegionWidth { get; }

        public double HalfRegionHeight { get; }

        public double MagnifierWidth { get; }

        public double MagnifierHeight { get; }

        public double BaselineWidth { get; }

        public double HorizontalBaselineOffset { get; }

        public double VerticalBaselineOffset { get; }

        public double LeftBaselineLength { get; }

        public double RightBaselineLength { get; }

        public double TopBaselineLength { get; }

        public double BottomBaselineLength { get; }
    }
}
