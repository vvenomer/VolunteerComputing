using System.Threading.Tasks;

namespace VolunteerComputing.TaskServer.Hubs
{
    public interface ITaskServerHubMessages
    {
        public Task SendTaskAsync(int programId, string data, bool useCpu);
    }
}
