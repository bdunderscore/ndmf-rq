﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace nadena.dev.ndmf.ReactiveQuery
{
    public static class TaskThrottle
    {
        public static readonly ThreadLocal<Func<bool>> ShouldThrottle = new(() => () => false);

        public static async ValueTask MaybeThrottle()
        {
            if (ShouldThrottle.Value())
            {
                await Task.CompletedTask.ContinueWith(
                    _ => Task.CompletedTask,
                    CancellationToken.None,
                    TaskContinuationOptions.RunContinuationsAsynchronously,
                    TaskScheduler.Current
                );
            }
        }
        
        public static IDisposable WithThrottleCondition(Func<bool> condition)
        {
            return new ThrottleConditionScope(condition);
        }
        
        private class ThrottleConditionScope : IDisposable
        {
            private readonly Func<bool> _previousCondition;
            
            public ThrottleConditionScope(Func<bool> condition)
            {
                _previousCondition = TaskThrottle.ShouldThrottle.Value;
                TaskThrottle.ShouldThrottle.Value = condition;
            }

            public void Dispose()
            {
                TaskThrottle.ShouldThrottle.Value = _previousCondition;
            }
        }
    }
}