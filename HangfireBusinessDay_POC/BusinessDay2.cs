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
            // Assuming business days are Monday to Friday.
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }


        private DateTime GetNextBusinessDay(DateTime currentDate)
        {
            // Check if today is a business day and the time is before 8 AM UTC.
            if (IsBusinessDay(currentDate) && currentDate.TimeOfDay < TimeSpan.FromHours(8))
            {
                // If it's before 8 AM, set the time to 8 AM UTC today.
                return new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 8, 0, 0, DateTimeKind.Utc);
            }

            // If it's not a business day or past 8 AM, find the next business day.
            DateTime nextBusinessDay = currentDate.AddDays(1);
            // Loop to skip weekends.
            while (nextBusinessDay.DayOfWeek == DayOfWeek.Saturday || nextBusinessDay.DayOfWeek == DayOfWeek.Sunday)
            {
                nextBusinessDay = nextBusinessDay.AddDays(1);
            }

            // Set the time to 8 AM UTC on the next business day.
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
