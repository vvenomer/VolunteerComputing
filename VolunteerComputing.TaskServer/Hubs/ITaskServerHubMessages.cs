using System.Threading.Tasks;

namespace VolunteerComputing.TaskServer.Hubs
{
    public interface ITaskServerHubMessages
    {
        public Task SendTaskAsync(int programId, byte[] data, bool useCpu);
        public Task InformFinished();
    }
}
