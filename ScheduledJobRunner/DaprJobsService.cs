namespace ScheduledJobRunner
{
    public sealed class DaprJobsService : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<DaprJobsService> logger;

        public DaprJobsService(HttpClient httpClient, ILogger<DaprJobsService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }
        public async Task EnsureWorkflowIsRunning()
        {
            var result = await httpClient.GetAsync("ensure-workflow-is-running");
            logger.LogDebug($"GET job `ensure-workflow-is-running` result : {result.StatusCode.ToString()}");
            if (!result.IsSuccessStatusCode)
            {
                var createResult = await httpClient.PostAsJsonAsync("ensure-workflow-is-running", new
                {
                    data = new
                    {
                        scheduled = DateTime.UtcNow
                    },
                    schedule = "@every 10s"
                });
                logger.LogInformation($"CREATE job `ensure-workflow-is-running` result : {createResult.StatusCode.ToString()}");
                createResult.EnsureSuccessStatusCode();
            }
        }

        public async Task EnsureJobSchedule1IsRegistered()
        {
            var result = await httpClient.GetAsync("schedule-1");
            logger.LogDebug($"GET job `schedule-1` result : {result.StatusCode.ToString()}");
            if (!result.IsSuccessStatusCode)
            {
                var createResult = await httpClient.PostAsJsonAsync("schedule-1", new
                {
                    data = new
                    {
                        scheduled = DateTime.UtcNow
                    },
                    schedule = "@every 60s"
                });
                logger.LogInformation($"CREATE job `schedule-1` result : {createResult.StatusCode.ToString()}");
                createResult.EnsureSuccessStatusCode();
            }
        }

        public async Task EnsureJobSchedule2IsRegistered()
        {
            var result = await httpClient.GetAsync("schedule-2");
            logger.LogDebug($"GET job `schedule-2` result : {result.StatusCode.ToString()}");
            if (!result.IsSuccessStatusCode)
            {
                var createResult = await httpClient.PostAsJsonAsync("schedule-2", new
                {
                    data = new
                    {
                        scheduled = DateTime.UtcNow
                    },
                    schedule = "@every 180s"
                });
                logger.LogInformation($"CREATE job `schedule-2` result : {createResult.StatusCode.ToString()}");
                createResult.EnsureSuccessStatusCode();
            }
        }


        public void Dispose() => httpClient?.Dispose();
    }

}