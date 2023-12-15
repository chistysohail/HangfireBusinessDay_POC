using Hangfire.Common;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireBusinessDay_POC
{
    // instance method filter 
    public class BusinessDay2FilterAttribute : JobFilterAttribute, IElectStateFilter
    {

        private DateTime GetNextBusinessDay(DateTime currentDate)
        {
            // Check if today is a business day (Monday through Friday).
            if (IsBusinessDay(currentDate))
            {
                // If it is a business day, return the current date and time.
                return currentDate;
            }

            // If it's not a business day (Saturday or Sunday), find the next Monday.
            DateTime nextBusinessDay = currentDate;
            while (nextBusinessDay.DayOfWeek != DayOfWeek.Monday)
            {
                nextBusinessDay = nextBusinessDay.AddDays(1);
            }

            // Set the time to the same time as the current day.
            return new DateTime(nextBusinessDay.Year, nextBusinessDay.Month, nextBusinessDay.Day,
                                currentDate.Hour, currentDate.Minute, currentDate.Second, DateTimeKind.Utc);
        }

        private bool IsBusinessDay(DateTime date)
        {
            // Business day is any weekday (Monday to Friday).
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        // The OnStateElection method should look like this:
        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null)
            {
                var nextTimeToRun = GetNextBusinessDay(DateTime.UtcNow);
                context.CandidateState = new ScheduledState(nextTimeToRun);
            }
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
