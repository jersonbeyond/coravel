using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Coravel.Scheduling.Schedule;
using Coravel.Scheduling.Schedule.Interfaces;
using Coravel.Scheduling.Schedule.Mutex;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.Scheduling.Stubs;
using Xunit;

namespace UnitTests.Scheduling.Invocable
{
    public class InvocableTests
    {
        [Fact]
        public async Task TestScheduledInvocableRuns()
        {
            bool invocableRan = false;
            var services = new ServiceCollection();
            services.AddScoped<Action>(p => () => invocableRan = true);
            services.AddScoped<TestInvocable>();
            var provider = services.BuildServiceProvider();

            var scheduler = new Scheduler(new InMemoryMutex(), provider.GetRequiredService<IServiceScopeFactory>(), new DispatcherStub());
            scheduler.Schedule<TestInvocable>().EveryMinute();

            await (scheduler as Scheduler).RunAtAsync(new DateTime(2019, 1, 1));

            Assert.True(invocableRan);
        }

        [Fact]
        public async Task TestScheduledInvocableWithParamsRuns()
        {
            bool invocableRan = false;
            var services = new ServiceCollection();
            services.AddScoped<Action>(p => () => invocableRan = true);
            services.AddScoped<TestInvocableWithParams>();
            var provider = services.BuildServiceProvider();

            var scheduler = new Scheduler(new InMemoryMutex(), provider.GetRequiredService<IServiceScopeFactory>(), new DispatcherStub());
            scheduler.ScheduleWithParams<TestInvocableWithParams>("stringParam", 1).EveryMinute();

            await (scheduler as Scheduler).RunAtAsync(new DateTime(2019, 1, 1));

            Assert.True(invocableRan);
        }

        [Fact]
        public async Task TestScheduledInvocableWithMissingParamsDoesNotRun()
        {
            bool invocableRan = false;
            var services = new ServiceCollection();
            services.AddScoped<Action>(p => () => invocableRan = true);
            services.AddScoped<TestInvocableWithParams>();
            var provider = services.BuildServiceProvider();

            var scheduler = new Scheduler(new InMemoryMutex(), provider.GetRequiredService<IServiceScopeFactory>(), new DispatcherStub());
            scheduler.ScheduleWithParams<TestInvocableWithParams>("stringParam").EveryMinute();

            await (scheduler as Scheduler).RunAtAsync(new DateTime(2019, 1, 1));

            Assert.False(invocableRan);
        }

        [Fact]
        public async Task TestScheduledInvocableFromTypeRuns()
        {
            bool invocableRan = false;
            var services = new ServiceCollection();
            services.AddScoped<Action>(p => () => invocableRan = true);
            services.AddScoped<TestInvocable>();
            var provider = services.BuildServiceProvider();

            var scheduler = new Scheduler(new InMemoryMutex(), provider.GetRequiredService<IServiceScopeFactory>(), new DispatcherStub());
            scheduler.ScheduleInvocableType(typeof(TestInvocable)).EveryMinute();

            await (scheduler as Scheduler).RunAtAsync(new DateTime(2019, 1, 1));

            Assert.True(invocableRan);
        }

        [Fact]
        public async Task TestScheduledInvocableFromTypeRuns_FromInterface()
        {
            bool invocableRan = false;
            var services = new ServiceCollection();
            services.AddScoped<Action>(p => () => invocableRan = true);
            services.AddScoped<TestInvocable>();
            var provider = services.BuildServiceProvider();

            IScheduler scheduler = new Scheduler(new InMemoryMutex(), provider.GetRequiredService<IServiceScopeFactory>(), new DispatcherStub());
            scheduler.ScheduleInvocableType(typeof(TestInvocable)).EveryMinute();

            await (scheduler as Scheduler).RunAtAsync(new DateTime(2019, 1, 1));

            Assert.True(invocableRan);
        }

        [Fact]  
        public async Task TestScheduledInvocableFromType_Throws()
        {
            await Assert.ThrowsAnyAsync<Exception>(async () => {
                var services = new ServiceCollection();
                services.AddScoped<Action>(p => () => { });
                services.AddScoped<TestInvocable>();
                var provider = services.BuildServiceProvider();

                var scheduler = new Scheduler(new InMemoryMutex(), provider.GetRequiredService<IServiceScopeFactory>(), new DispatcherStub());
                scheduler.ScheduleInvocableType(typeof(string)).EveryMinute();

                await (scheduler as Scheduler).RunAtAsync(new DateTime(2019, 1, 1));
            });
        }

        private class TestInvocable : IInvocable
        {
            private Action _func;

            public TestInvocable(Action func) => this._func = func;
            public Task Invoke()
            {
                this._func();
                return Task.CompletedTask;
            }
        }

        private class TestInvocableWithParams : IInvocable
        {
            private Action _func;

            public TestInvocableWithParams(Action func, string stringParam, int intParam)
            {
                this._func = func;
            }

            public Task Invoke()
            {
                this._func();
                return Task.CompletedTask;
            }
        }
    }
}