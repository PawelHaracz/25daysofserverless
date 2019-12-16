using System;
using System.Threading.Tasks;

namespace Day16.Entities.Interfaces
{
    public interface IPasadasAllAggregator
    {
        Task Add(string id);
    }
}