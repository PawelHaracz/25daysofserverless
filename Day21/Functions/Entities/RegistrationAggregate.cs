using System;
using System.Threading.Tasks;
using Day16.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Day16.Entities
{
    public class RegistrationAggregate 
    {
        public string Id;
        public Status Status { get; set; }
        public DateTime? StartedTime { get; set; } 
        public DateTime? FinishedTime { get; set; }
        
        [FunctionName(nameof(RegistrationAggregate))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            if (ctx.HasState is false)
            {
                ctx.SetState(new RegistrationAggregate()
                {
                    Id = ctx.EntityKey
                });
            }
            return ctx.DispatchAsync<RegistrationAggregate>();
        }

        public Task Create(DateTime time)
        {
            Status = Status.Started;
            StartedTime = time;
            Entity.Current.SignalEntity(new EntityId(nameof(AllRegistrationAggregate), AllRegistrationAggregate.EntityId), nameof(AllRegistrationAggregate.Register),
                Entity.Current.EntityKey);
            return Task.CompletedTask;;
        }

        public Task SetState(UpdateStatusCommand command)
        {
            if (Status is Status.Started)
            {
                Status = command.Status;
                FinishedTime = command.Time;
            }

            return Task.CompletedTask;
        }
    }
}