using System.Threading.Tasks;
using Day19.Entities.Interfaces;
using Day19.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Day19.Entities
{
    public class CompressorEntity : ICompressorEntity
    {
        public float Pressure {get;set;}
        public int Balloons { get; set; }

        public CompressorEntity()
        {
            if (Entity.Current.HasState is false)
            {
                Balloons = 333;
            }
        }
        
        public Task Calculate(float balloonPressure)
        {
            if (balloonPressure < 0.1)
            {
                return Task.CompletedTask;
            }

            if (balloonPressure > 0.6)
            {
                return Task.CompletedTask;
            }
            
            var @result = 0.6f - balloonPressure;
            if (Pressure - @result < 0)
            {
                result = 0.6f - Pressure;
            }
            else
            {
                Balloons--;
            }

            return Task.CompletedTask;
        }

        public Task UpdatePressure(float compressorPressure)
        {
            Pressure = compressorPressure;
            return  Task.CompletedTask;
        }

        [FunctionName(nameof(CompressorEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<CompressorEntity>();
        }

    }
}