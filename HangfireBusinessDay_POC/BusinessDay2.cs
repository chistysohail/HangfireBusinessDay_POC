using Hangfire.Common;
using Hangfire.States;

namespace HangfireBusinessDay_POC
{
    // instance method filter 
    public class BusinessDay2FilterAttribute : JobFilterAttribute, IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null && !IsBusinessDay(DateTime.Now))
            {
                var nextBusinessDay = GetNextBusinessDay(DateTime.Now);
                context.CandidateState = new ScheduledState(nextBusinessDay);
            }
        }

        private bool IsBusinessDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        private DateTime GetNextBusinessDay(DateTime currentDate)
        {
            DateTime nextBusinessDay = currentDate;
            while (!IsBusinessDay(nextBusinessDay))
            {
                nextBusinessDay = nextBusinessDay.AddDays(1);
            }
            return nextBusinessDay.Date;
        }

    }

    public class MyJobClass
    {
        [BusinessDay2Filter]
        public void MyJobMethod()
        {
            Console.WriteLine("Executing instance method with businessday2Filters.... ");
        }
    }

    // Schedule your job as usual


}
