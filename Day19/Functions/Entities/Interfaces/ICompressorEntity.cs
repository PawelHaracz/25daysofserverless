using System.Threading.Tasks;

namespace Day19.Entities.Interfaces
{
    public interface ICompressorEntity
    {
        Task Calculate(float balloonPressure);
        Task UpdatePressure(float compressorPressure);
    }
}