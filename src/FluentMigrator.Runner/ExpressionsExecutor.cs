#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner
{
    public interface IExpressionsExecutor
    {
        IList<Exception> CaughtExceptions { get; }
        void Execute(ICollection<IMigrationExpression> expressions, IMigrationConventions conventions, bool silentlyFail);
    }

    public class ExpressionsExecutor : IExpressionsExecutor
    {
        private readonly IAnnouncer _announcer;
        private readonly IMigrationProcessor _processor;
        private readonly IStopWatch _stopWatch;
        public IList<Exception> CaughtExceptions { get; private set; }

        public ExpressionsExecutor(IAnnouncer announcer, IMigrationProcessor processor, IStopWatch stopWatch)
        {
            _announcer = announcer;
            _processor = processor;
            _stopWatch = stopWatch;
        }

        public void Execute(ICollection<IMigrationExpression> expressions, IMigrationConventions conventions, bool silentlyFail)
        {
            long insertTicks = 0;
            var insertCount = 0;
            CaughtExceptions = new List<Exception>();

            foreach (var expression in expressions)
            {
                try
                {
                    expression.ApplyConventions(conventions);
                    if (expression is InsertDataExpression)
                    {
                        insertTicks += Time(() => expression.ExecuteWith(_processor));
                        insertCount++;
                    }
                    else
                    {
                        AnnounceTime(expression.ToString(), () => expression.ExecuteWith(_processor));
                    }
                }
                catch (Exception er)
                {
                    _announcer.Error(er.Message);

                    //catch the error and move onto the next expression
                    if (silentlyFail)
                    {
                        CaughtExceptions.Add(er);
                        continue;
                    }
                    throw;
                }
            }

            if (insertCount > 0)
            {
                var avg = new TimeSpan(insertTicks / insertCount);
                var msg = string.Format("-> {0} Insert operations completed in {1} taking an average of {2}", insertCount, new TimeSpan(insertTicks), avg);
                _announcer.Say(msg);
            }
        }

        private void AnnounceTime(string message, Action action)
        {
            _announcer.Say(message);

            _stopWatch.Start();
            action();
            _stopWatch.Stop();

            _announcer.ElapsedTime(_stopWatch.ElapsedTime());
        }

        private long Time(Action action)
        {
            _stopWatch.Start();

            action();

            _stopWatch.Stop();

            return _stopWatch.ElapsedTime().Ticks;
        }
    }
}