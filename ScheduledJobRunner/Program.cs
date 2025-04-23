using Dapr.Workflow;
using ScheduledJobRunner;
using ScheduledJobRunner.Activities;
using ScheduledJobRunner.Workflows;

var builder = WebApplication.CreateBuilder(args);

string workflowId = "my-singleton-workflow";

builder.Services.AddHttpClient();
builder.Services.AddDaprWorkflow(options =>
    {
        options.RegisterWorkflow<MyWorkflow>();
        options.RegisterActivity<Activity_A>();
        options.RegisterActivity<Activity_B>();
    });

builder.Services.AddHttpClient<DaprJobsService>(
    client =>
    {
        client.BaseAddress = new Uri($"http://localhost:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}/v1.0-alpha1/jobs/");
    });

var app = builder.Build();


app.MapGet("/health", async (DaprJobsService jobsService) =>
{
    await jobsService.EnsureWorkflowIsRunning();
    await jobsService.EnsureJobSchedule1IsRegistered();
    await jobsService.EnsureJobSchedule2IsRegistered();
    app.Logger.LogInformation($"Health is good");
});


app.MapPost("/job/ensure-workflow-is-running", async (DaprWorkflowClient workflowClient) =>
{
    var createWorkflow = false;

    try
    {
        var instance = await workflowClient.GetWorkflowStateAsync(workflowId, false);
        if (!instance.Exists || !instance.IsWorkflowRunning)
            createWorkflow = true;
    }
    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unknown)
    {
        // TODO : refactor this when the wfruntime handles 404 workflows properly
        createWorkflow = true;
    }

    if (!createWorkflow)
        return;

    app.Logger.LogWarning($"'{workflowId}' workflow does not exist, attempting to schedule it");

    await workflowClient.ScheduleNewWorkflowAsync(nameof(MyWorkflow), workflowId, null);
});

app.MapPost("/job/schedule-1", async (DaprWorkflowClient workflowClient) =>
{
    // Optionally, you could do a GET on the workflow here to check the `CustomStatus` contains the phrase "Waiting for signal" 
    // so you know its safe to proceed.

    try
    {
        app.Logger.LogInformation($"'[Schedule-1] Raising `SIGNAL` to '{workflowId}' attempting to schedule it");
        await workflowClient.RaiseEventAsync(workflowId, "SIGNAL", "schedule-1");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error when raising SIGNAL event to Workflow. Maybe the workflow was in the wrong state to accept the signal?");
    }
});

app.MapPost("/job/schedule-2", async (DaprWorkflowClient workflowClient) =>
{
    // Optionally, you could do a GET on the workflow here to check the `CustomStatus` contains the phrase "Waiting for signal" 
    // so you know its safe to proceed.

    try
    {
        app.Logger.LogInformation($"'[Schedule-2] Raising `SIGNAL` to '{workflowId}' attempting to schedule it");
        await workflowClient.RaiseEventAsync(workflowId, "SIGNAL", "schedule-2");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error when raising SIGNAL event to Workflow. Maybe the workflow was in the wrong state to accept the signal?");
    }
});


app.MapGet("/", () => "Hello World!");

app.Run();
