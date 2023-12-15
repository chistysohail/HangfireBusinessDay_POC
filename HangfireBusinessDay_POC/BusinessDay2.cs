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
        

        // In the OnStateElection method, you would use this method like this:
        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null)
            {
                var nextBusinessDayAt8AmUtc = GetNextBusinessDay(DateTime.UtcNow);
                context.CandidateState = new ScheduledState(nextBusinessDayAt8AmUtc);
            }
        }


       
        private bool IsBusinessDay(DateTime date)
        {
            // Business day is any weekday (Monday to Friday)
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

       
        private DateTime GetNextBusinessDay(DateTime currentDate)
        {
            // If today is a business day and it's before 8 AM UTC, schedule for today at 8 AM UTC.
            if (IsBusinessDay(currentDate) && currentDate.TimeOfDay < TimeSpan.FromHours(8))
            {
                return new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 8, 0, 0, DateTimeKind.Utc);
            }

            // Otherwise, find the next business day.
            DateTime nextBusinessDay = currentDate.Date.AddDays(1);
            while (!IsBusinessDay(nextBusinessDay))
            {
                nextBusinessDay = nextBusinessDay.AddDays(1);
            }

            // Return 8 AM UTC on the next business day.
            return new DateTime(nextBusinessDay.Year, nextBusinessDay.Month, nextBusinessDay.Day, 8, 0, 0, DateTimeKind.Utc);
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
