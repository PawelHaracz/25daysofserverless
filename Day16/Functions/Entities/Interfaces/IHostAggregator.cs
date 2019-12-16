using System.Threading.Tasks;
using Day16.Models;

namespace Day16.Entities.Interfaces
{
    public interface IHostAggregator
    {
        Task Create(CreateHostCommand createHostCommand);
    }
}