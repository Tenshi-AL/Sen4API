using System.Net.Http.Json;
using Infrastructure.DTO;
using Infrastructure.Models;

namespace Sen4.IntegrationTests.Helpers;

public class ProjectHelper(HttpClient httpClient)
{
    private const string ProjectPostUrl = "project";
    private const string ProjectTaskPostUrl = "ProjectTask";
    private const string ProjectSetRuleUrl = "Rule";
    public async Task<ProjectReadDTO?> CreateProjectAsync(string projectName)
    {
        var body = new ProjectWriteDTO()
        {
            Name = projectName,
            CreatedDateTime = DateTime.UtcNow,
            Description = null,
        };
        httpClient.DefaultRequestHeaders.Add("requestId", Guid.NewGuid().ToString());
        var response = await httpClient.PostAsJsonAsync(ProjectPostUrl, body);
        var result = await response.Content.ReadFromJsonAsync<ProjectReadDTO>();
        return result;
    }

    public async Task<HttpResponseMessage?> CreateTaskAsync(Guid projectId, string name, Guid userId)
    {
        return await httpClient.PostAsJsonAsync(ProjectTaskPostUrl, new ProjectTaskWriteDTO()
        {
            Name = name,
            Description = null,
            TaskStatusId = Guid.Parse("8f2da16a-0d31-4585-bacf-118135fe4dcd"),
            UserExecutorId = userId,
            UserCreatedId = userId,
            PriorityId = Guid.Parse("8f2da16a-0d31-4585-bacf-118135fe4dcd"),
            ProjectId = projectId
        });
    }

    public async Task<HttpResponseMessage?> SetRuleAsync(Guid userId, Guid projectId, Guid operationId)
    {
        return await httpClient.PostAsJsonAsync(ProjectSetRuleUrl, new SetRuleModel()
        {
            UserId = userId,
            ProjectId = projectId,
            Rules = new List<RuleDTO>()
            {
                new RuleDTO()
                {
                    OperationId = operationId,
                    Access = true
                }
            }
        });
    }
}