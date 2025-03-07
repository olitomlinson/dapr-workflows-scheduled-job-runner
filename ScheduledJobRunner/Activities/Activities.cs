using Dapr.Workflow;

namespace ScheduledJobRunner.Activities
{
    public class Activity_A : WorkflowActivity<string, bool>
    {
        public override async Task<bool> RunAsync(WorkflowActivityContext context, string viewName)
        {
            // Pretend to do some work for 15 seconds

            await Task.Delay(TimeSpan.FromSeconds(15));
            return true;
        }
    }

    public class Activity_B : WorkflowActivity<string, bool>
    {
        public override async Task<bool> RunAsync(WorkflowActivityContext context, string viewName)
        {
            // Pretend to do some work for 15 seconds

            await Task.Delay(TimeSpan.FromSeconds(15));
            return true;
        }
    }
}