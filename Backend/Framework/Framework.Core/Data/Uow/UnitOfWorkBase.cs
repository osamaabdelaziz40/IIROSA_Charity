using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Threading.Tasks;
using ZXing;

namespace Framework.Core.Data.Uow
{
    public class UnitOfWorkBase<TContext> : IUnitOfWorkBase<TContext>
        where TContext : IBaseDbContext
    {
        public TContext Context { get; }
        private readonly IMediator _mediator;

        public UnitOfWorkBase(TContext context, 
                              IMediator mediator)
        {
            Context = context;
            _mediator = mediator;
        }

        public virtual int SaveChanges()
        {
            return Context.SaveChanges();
        }

        
        public virtual async Task<int> SaveChangesAsync()
        {
            int result = await Context.SaveChangesAsync();
            await Dispatcher();
            return result;
        }

        private async Task Dispatcher()
        {
            if (_mediator == null) return;

            var entitiesWithEvents = Context.ChangeTracker.Entries()
                .Select(e => e.Entity as EntityBase)
                .Where(e => e?.Events != null && e.Events.Any())
                .ToArray();

            foreach (var entity in entitiesWithEvents)
            {
                var events = entity!.Events.ToArray();
                entity.Events.Clear();
                foreach (var domainEvent in events)
                {
                    await _mediator.Publish(domainEvent).ConfigureAwait(false);
                }
            }

        }

        ~UnitOfWorkBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Context?.Dispose();
                var dbContext = this.Context;
                dbContext?.Dispose();
            }
        }
    }
}