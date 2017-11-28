using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;

namespace DbConnection.Serilog
{
    internal sealed class LoggingScope : IDisposable
    {
        private readonly ILogger _Logger;
        private readonly string _MessageTemplate;
        private readonly object[] _PropertyValues;
        private readonly Stopwatch _StopWatch;
        private LoggingScopeStatus _Status;

#pragma warning disable Serilog004 // Constant MessageTemplate verifier
        internal LoggingScope(ILogger logger, string messageTemplate, params object[] propertyValues)
        {
            _Logger = logger;
            _MessageTemplate = messageTemplate;
            _PropertyValues = propertyValues;
            _StopWatch = new Stopwatch();
            _StopWatch.Start();
            _Logger.Information($">> Started: \"{_MessageTemplate}\"", _PropertyValues);
        }

        public void Dispose()
        {
            _StopWatch.Stop();
            switch (_Status)
            {
                case LoggingScopeStatus.Incompleted:
                    if (_Logger.IsEnabled(LogEventLevel.Information))
                    {
                        var values = _PropertyValues.ToList();
                        values.Add(_StopWatch.ElapsedMilliseconds);

                        _Logger.Information($"<< Finished: \"{_MessageTemplate}\" Unsuccessfully, ({{duration}}ms)", values.ToArray());
                    }
                    break;
                case LoggingScopeStatus.Completed:
                    if (_Logger.IsEnabled(LogEventLevel.Debug))
                    {
                        var values = _PropertyValues.ToList();
                        values.Add(_StopWatch.ElapsedMilliseconds);

                        _Logger.Debug($"<< Finished: \"{_MessageTemplate}\" Successfully, ({{duration}}ms)", values.ToArray());
                    }
                    break;
            }
        }
#pragma warning restore Serilog004 // Constant MessageTemplate verifier

        internal void Complete()
        {
            _Status = LoggingScopeStatus.Completed;
        }

        enum LoggingScopeStatus
        {
            Incompleted, Completed
        }
    }
}
