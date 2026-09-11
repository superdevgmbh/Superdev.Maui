using Superdev.Maui.Extensions;

namespace Superdev.Maui.Tests.Extensions
{
    public class TimeSpanExtensionsTests
    {
        [Theory]
        [ClassData(typeof(ToSecondsStringTestData))]
        public void ShouldFormatTimeSpanUsingToSecondsString(TimeSpan timeSpan, string expectedOutput)
        {
            // Act
            var formattedTimeSpan = timeSpan.ToSecondsString();

            // Assert
            formattedTimeSpan.Should().Be(expectedOutput);
        }

        public class ToSecondsStringTestData : TheoryData<TimeSpan, string>
        {
            public ToSecondsStringTestData()
            {
                this.Add(new TimeSpan(), "0.0s");
                this.Add(new TimeSpan(0, 0, 0, 0, 1), "0.001s");
                this.Add(new TimeSpan(0, 0, 0, 0, 10), "0.01s");
                this.Add(new TimeSpan(0, 0, 0, 0, 100), "0.1s");
                this.Add(new TimeSpan(0, 0, 0, 1, 0), "1.0s");
                this.Add(new TimeSpan(0, 0, 1, 0, 0), "60.0s");
                this.Add(new TimeSpan(0, 0, 10, 0, 0), "600.0s");
                this.Add(TimeSpan.MaxValue, "922337203685.5s");
            }
        }

        [Theory]
        [ClassData(typeof(ToDurationStringTestData))]
        public void ShouldConvertTimeSpanToDurationString(TimeSpan timeSpan, string expectedDurationString)
        {
            // Act
            var durationString = timeSpan.ToDurationString();

            // Assert
            durationString.Should().Be(expectedDurationString);
        }

        public class ToDurationStringTestData : TheoryData<TimeSpan, string>
        {
            public ToDurationStringTestData()
            {
                // Special cases
                this.Add(TimeSpan.MinValue, "");
                this.Add(TimeSpan.MaxValue, "10675199d");
                this.Add(TimeSpan.Zero, "0s");

                // Simple TimeSpans
                this.Add(TimeSpan.FromDays(1), "1d");
                this.Add(TimeSpan.FromHours(1), "1h");
                this.Add(TimeSpan.FromMinutes(1), "1m");
                this.Add(TimeSpan.FromSeconds(1), "1s");
                this.Add(TimeSpan.FromMilliseconds(1), "0.001s");
                this.Add(TimeSpan.FromMicroseconds(1L), "1.0µs");
                this.Add(TimeSpan.FromTicks(1L), "0.1µs");

                // Complex TimeSpans
                this.Add(new TimeSpan(1, 2, 3, 4), "1d");
                this.Add(new TimeSpan(788, 97, 66, 100, 1234), "792d");
            }
        }

        [Theory]
        [ClassData(typeof(ToStringExtendedTestData))]
        public void ShouldConvertToStringExtended(TimeSpan? timeSpan, string format, string expectedOutput)
        {
            // Act
            var output = timeSpan.ToStringExtended(format);

            // Assert
            output.Should().Be(expectedOutput);
        }

        public class ToStringExtendedTestData : TheoryData<TimeSpan?, string, string>
        {
            public ToStringExtendedTestData()
            {
                // Begin of the day
                this.Add(TimeSpan.Zero, "hh\\:mm", "00:00");
                this.Add(TimeSpan.Zero, "hh\\:mm\\:ss", "00:00:00");
                this.Add(TimeSpan.Zero, "hh:mm tt", "12:00 AM");
                this.Add(TimeSpan.Zero, "g", "0:00:00");
                this.Add(TimeSpan.Zero, "c", "00:00:00");

                // Regular day times
                this.Add(TimeSpan.FromMinutes(90), "c", "01:30:00");
                this.Add(TimeSpan.FromMinutes(90), "hh\\:mm", "01:30");
                this.Add(TimeSpan.FromMinutes(90), "h:mm tt", "1:30 AM");

                this.Add(TimeSpan.FromHours(13) + TimeSpan.FromMinutes(45), "hh\\:mm", "13:45");
                this.Add(TimeSpan.FromHours(13) + TimeSpan.FromMinutes(45), "h:mm tt", "1:45 PM");
                this.Add(TimeSpan.FromHours(13) + TimeSpan.FromMinutes(45), "g", "13:45:00");

                // End of the day
                var endOfDay = TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59) + TimeSpan.FromSeconds(59);
                this.Add(endOfDay, "c", "23:59:59");
                this.Add(endOfDay, "hh\\:mm\\:ss", "23:59:59");
                this.Add(endOfDay, "h:mm tt", "11:59 PM");

                // Nullable edge cases
                this.Add(null, "c", string.Empty);
                this.Add(null, "hh\\:mm", string.Empty);
                this.Add(TimeSpan.MinValue, "c", string.Empty);

                // Custom separator format
                this.Add(TimeSpan.FromHours(2) + TimeSpan.FromMinutes(5), "hh\\-mm", "02-05");

                // Multi-day TimeSpan
                this.Add(TimeSpan.FromDays(1) + TimeSpan.FromHours(2) + TimeSpan.FromMinutes(30), "g", "1:2:30:00");
                this.Add(TimeSpan.FromDays(1) + TimeSpan.FromHours(2) + TimeSpan.FromMinutes(30), "c", "1.02:30:00");
            }
        }
    }
}