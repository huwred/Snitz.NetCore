namespace SnitzEvents.Helpers
{
    public static class CalEnums
    {
        public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek)
        {
            int start = (int)from.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return from.AddDays(target - start);
        }

        public enum CalRecur
        {
            None=0,
            EveryDay,
            EveryWeek=5,
            EveryOtherWeek,
            EveryMonth=9,
            EveryYear=11
        }

        public enum CalDays
        {
            Sun=0,
            Mon,
            Tues,
            Wed,
            Thur,
            Fri,
            Sat
        }

        public enum CalAllowed
        {
            None=0,
            Members=2,
            AdminsModerators=3,
            Admins=4
        }

    }
}