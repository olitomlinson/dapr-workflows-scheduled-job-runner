
using Dapr.Workflow;
using ScheduledJobRunner.Activities;

namespace ScheduledJobRunner.Workflows
{

    public class State
    {
        public List<string> Logs { get; set; }
    }
    public class MyWorkflow : Workflow<State?, bool>
    {
        public override async Task<bool> RunAsync(WorkflowContext context, State state)
        {
            if (state == null)
            {
                state = new State() { Logs = new List<string>() };
            }
            var status = $"[{context.CurrentUtcDateTime}] - Waiting for signal to start...";
            context.SetCustomStatus(status);
            var scheduleId = await context.WaitForExternalEventAsync<string>("SIGNAL");


            var startTime = context.CurrentUtcDateTime;
            context.SetCustomStatus($"[{context.CurrentUtcDateTime}] - {scheduleId} - Running Activity A...");
            var refreshResult = await context.CallActivityAsync<bool>(nameof(Activity_A), "vw_foo");
            if (!refreshResult)
            {
                //  TODO : uh oh, something went wrong. Raise the alarm. Send an email to someone.

                state.Logs.Add($"[{context.CurrentUtcDateTime}] - {scheduleId} - Something went wrong with Activity A...");
                context.ContinueAsNew(state, false);
                return true;
            }

            context.SetCustomStatus($"[{context.CurrentUtcDateTime}] - {scheduleId} - Running Activity B...");
            var rebuildResult = await context.CallActivityAsync<bool>(nameof(Activity_B), "vw_foo");
            if (!rebuildResult)
            {
                // TODO : uh oh, something went wrong. Raise the alarm. Send an email to someone.

                state.Logs.Add($"[{context.CurrentUtcDateTime}] - {scheduleId} - Something went wrong with Activity B...");
                context.ContinueAsNew(state, false);
                return true;
            }

            state.Logs.Add($"[{context.CurrentUtcDateTime}] - {scheduleId} - All Activities completed successfully. Total duration of Activities = {context.CurrentUtcDateTime.Subtract(startTime).TotalSeconds}s ");
            context.ContinueAsNew(state, false);
            return true;
        }
    }
}
