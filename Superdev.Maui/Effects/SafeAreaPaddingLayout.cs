using System.ComponentModel;
using System.Diagnostics;

namespace Superdev.Maui.Effects
{
    [DebuggerDisplay("SafeAreaPaddingLayout: {this.ToString()}")]
    [TypeConverter(typeof(SafeAreaPaddingLayoutConverter))]
    public sealed class SafeAreaPaddingLayout
    {
        private static readonly PaddingPosition[] All = Enum.GetValues(typeof(PaddingPosition)).OfType<PaddingPosition>().ToArray();

        public SafeAreaPaddingLayout(params PaddingPosition[] positions)
        {
            this.Positions = positions;
        }

        public PaddingPosition[] Positions { get; }

        public override string ToString()
        {
            return string.Format($"{string.Join(",", this.Positions.Select(p => $"{p}"))}");
        }

        public enum PaddingPosition
        {
            Left,
            Top,
            Right,
            Bottom,
        }

        public Thickness Transform(Thickness padding)
        {
            var notPresentPositions = All.Except(this.Positions);
            foreach (var paddingPosition in notPresentPositions)
            {
                if (paddingPosition == PaddingPosition.Left)
                {
                    padding.Left = 0;
                }
                else if (paddingPosition == PaddingPosition.Top)
                {
                    padding.Top = 0;
                }
                else if (paddingPosition == PaddingPosition.Right)
                {
                    padding.Right = 0;
                }
                else if (paddingPosition == PaddingPosition.Bottom)
                {
                    padding.Bottom = 0;
                }
            }

            return padding;
        }
    }
}