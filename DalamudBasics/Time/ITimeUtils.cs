using System;

namespace DalamudBasics.Time
{
    public interface ITimeUtils
    {
        DateTime GetLocalDateTime();

        long GetNowUnixTimestamp(bool useUtc = true);
    }
}
