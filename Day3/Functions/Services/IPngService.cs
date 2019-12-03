using System.IO;
using System.Threading.Tasks;

namespace Day3.Services
{
    public interface IPngService
    {
        Task<Stream> GetPngAsync(string pngUrl);
    }
}