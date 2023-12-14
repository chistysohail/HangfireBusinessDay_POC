using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using HangfireBusinessDay_POC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services
// Replace with your preferred storage option, e.g., UseSqlServerStorage for SQL Server
builder.Services.AddHangfire(config => config.UseSqlServerStorage("Server=(localdb)\\mssqllocaldb;Database=Test;Trusted_Connection=True;MultipleActiveResultSets=true;"));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<MyJobs>();


var app = builder.Build();

// Configure Hangfire dashboard
app.UseHangfireDashboard();

// Schedule the job
//RecurringJob.AddOrUpdate(() => MyJobs.PerformImportantTask(), Cron.Daily);
using (var scope = app.Services.CreateScope())
{
    var myJobs = scope.ServiceProvider.GetRequiredService<MyJobs>();
    MyJobs.PerformImportantTask();
}
RecurringJob.AddOrUpdate(() => MyJobs2.PerformImportantTask(), "*/2 * * * *");
RecurringJob.AddOrUpdate<MyJobClass>("myJob", x => x.MyJobMethod(), "*/2 * * * *");
app.Run();

// Custom Job Filter Attribute
public class BusinessDayAttribute : JobFilterAttribute, IClientFilter, IServerFilter
{

    public string ClassName { get; }
    public string MethodName { get; }
    public string AssemblyName { get; }
    public string NameSpace { get; }
    public BusinessDayAttribute(string nameSpace, string className, string methodName, string assemblyName)
    {
        ClassName = className;
        MethodName = methodName;
        AssemblyName = assemblyName;
        NameSpace = nameSpace;
    }

    public void OnCreating(CreatingContext context)
    {

        var methodInfo = context.Job.Method;
        var attribute = methodInfo.GetCustomAttributes(typeof(BusinessDayAttribute), false)
                                  .FirstOrDefault() as BusinessDayAttribute;

        if (attribute != null)
        {
            // Now you have access to the class name, method name, and assembly name
            string className = attribute.ClassName;
            string methodName = attribute.MethodName;
            string assemblyName = attribute.AssemblyName;
            string nameSpace = attribute.NameSpace;

            // Use this information as needed

            var nextBusinessDay = GetNextBusinessDay(DateTime.Now);
            if (nextBusinessDay != DateTime.Now.Date)
            {
                context.Canceled = true;
                //BackgroundJob.Schedule(() => context.Job.Method.Invoke(null, context.Job.Args.ToArray()), nextBusinessDay - DateTime.Now);

                //ExecuteMethod(nameSpace, className, methodName, context.Job.Args.ToArray(), assemblyName);
                BackgroundJob.Schedule(() => ExecuteMethod(nameSpace, className, methodName, context.Job.Args.ToArray(), assemblyName), nextBusinessDay - DateTime.Now);


            }
        }
    }
    public static void ExecuteMethod(string nameSpace, string className, string methodName, object[] args, string assemblyName)
    {

        //Console.WriteLine(Type.GetType($"{nameSpace}.{className},{assemblyName}"));//.Assembly.GetName().Name);
        var type = Type.GetType($"{nameSpace}.{className}, {assemblyName}");
        if (type == null)
            Console.WriteLine("Type not found.......");
        var method = type.GetMethod(methodName);
        // Static method invocation
        method.Invoke(null, args);
    }
    public static void ExecuteReflectedJob(MethodInfo methodInfo, object[] args)
    {
        // For a static method, pass null as the first argument
        methodInfo.Invoke(null, args);
    }
    // Job method
    public static void YourJobMethod(string typeName, string methodName, object[] args)
    {
        var type = Type.GetType(typeName);

        var method = type.GetMethod(methodName);
        method.Invoke(null, args);
    }
    public static void ExecuteScheduledJob(MethodInfo methodInfo, object[] args)
    {
        // Assuming the method is static; pass null as the first argument
        methodInfo.Invoke(null, args);
    }


    public void OnPerforming(PerformingContext context)
    {
        if (!IsBusinessDay(DateTime.Now))
        {
            context.Canceled = true;
        }
    }

    public void OnCreated(CreatedContext filterContext) { }
    public void OnPerformed(PerformedContext filterContext) { }

    private bool IsBusinessDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Thursday;
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

// Job class
public class MyJobs
{
    // [BusinessDay("HangfireBusinessDay_POC", "MyJobs", "PerformImportantTask", "HangfireBusinessDay_POC")]
    public static void PerformImportantTask()
    {
        Console.WriteLine($"Performing an important task of jobs class {nameof(MyJobs)}!");
    }
}


/*
drop table[Test].[HangFire].AggregatedCounter
drop table [Test].[HangFire].Counter
drop table [Test].[HangFire].Hash
drop table [Test].[HangFire].Job
drop table [Test].[HangFire].JobParameter
drop table [Test].[HangFire].JobQueue
drop table [Test].[HangFire].List
drop table [Test].[HangFire].[Schema]
drop table[Test].[HangFire].Server
drop table [Test].[HangFire].[Set]
drop table[Test].[HangFire].State
 */