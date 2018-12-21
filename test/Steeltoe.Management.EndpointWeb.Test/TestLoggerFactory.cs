using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Steeltoe.Management.EndpointWeb.Test
{
    public class TestLoggerFactory : ILoggerFactory
    {
        private readonly ITestOutputHelper outputHelper;
        private ILogger logger;

        public TestLoggerFactory(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitLogger(this.outputHelper);
        }

        public void Dispose()
        {
        }

        private class XUnitLogger : ILogger, IDisposable
        {
            private readonly Action<string> _writeline;

            public XUnitLogger(ITestOutputHelper outputHelper = null)
            {
                _writeline = outputHelper == null ? (Action<string>)outputHelper.WriteLine : Console.WriteLine;
            }

            public void Dispose()
            {
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => _writeline(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => this;
        }
    }

}