using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Serilog;
using System.Data.Common;
using System.Threading;

namespace DbConnection.Serilog
{
    public class LoggingCommandWrapper : DbCommand, IDisposable
    {
        private readonly DbCommand _WrappedDbCommand;
        private readonly ILogger _Logger;

        public LoggingCommandWrapper(DbCommand dbCommand, ILogger logger)
        {
            Contract.Requires(dbCommand != null, nameof(dbCommand) + " is null.");
            Contract.Requires(logger != null, nameof(logger) + " is null.");
            _WrappedDbCommand = dbCommand;
            _Logger = logger;
        }

        public override string CommandText
        {
            get { return _WrappedDbCommand.CommandText; }
            set { _WrappedDbCommand.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return _WrappedDbCommand.CommandTimeout; }
            set { _WrappedDbCommand.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return _WrappedDbCommand.CommandType; }
            set { _WrappedDbCommand.CommandType = value; }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get { return _WrappedDbCommand.Connection; }
            set
            {
                if (value == null)
                {
                    _WrappedDbCommand.Connection = value;
                    return;
                }
                var loggingConnectionWrapper = value as LoggingConnectionWrapper;
                if (loggingConnectionWrapper == null)
                {
                    loggingConnectionWrapper = new LoggingConnectionWrapper(value, _Logger);
                }
                _WrappedDbCommand.Connection = value;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _WrappedDbCommand.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return _WrappedDbCommand.Transaction; }
            set { _WrappedDbCommand.Transaction = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _WrappedDbCommand.UpdatedRowSource; }
            set { _WrappedDbCommand.UpdatedRowSource = value; }
        }

        public override bool DesignTimeVisible { get; set; }

        public override void Cancel()
        {
            _WrappedDbCommand.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _WrappedDbCommand.CreateParameter();
        }

        void IDisposable.Dispose()
        {
            _WrappedDbCommand.Dispose();
        }

        public override int ExecuteNonQuery()
        {
            var command = GetCommandLog();
            using (var scope = new LoggingScope(_Logger, $"{nameof(ExecuteNonQuery)} - {{Command}}", command))
            {
                var result = _WrappedDbCommand.ExecuteNonQuery();
                scope.Complete();
                return result;
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var command = GetCommandLog();
            using (var scope = new LoggingScope(_Logger, $"{nameof(ExecuteReader)} - {{Command}}", command))
            {
                var result = _WrappedDbCommand.ExecuteReader();
                scope.Complete();
                return result;
            }
        }

        protected async override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var command = GetCommandLog();
            using (var scope = new LoggingScope(_Logger, $"{nameof(ExecuteReader)} - {{Command}}", command))
            {
                var result = await _WrappedDbCommand.ExecuteReaderAsync(behavior, cancellationToken);
                scope.Complete();
                return result;
            }
        }

        public override object ExecuteScalar()
        {
            var command = GetCommandLog();
            using (var scope = new LoggingScope(_Logger, $"{nameof(ExecuteScalar)} - {{Command}}", command))
            {
                var result = _WrappedDbCommand.ExecuteScalar();
                scope.Complete();
                return result;
            }
        }

        public async override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            string command;
            try
            {
                command = GetCommandLog();
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, "Error getting command to log");
                command = "Error Getting Command";
            }
            using (var scope = new LoggingScope(_Logger, $"{nameof(ExecuteScalar)} - {{Command}}", command))
            {
                var result = await _WrappedDbCommand.ExecuteScalarAsync(cancellationToken);
                scope.Complete();
                return result;
            }
        }

        public override void Prepare()
        {
            _WrappedDbCommand.Prepare();
        }

        protected virtual string GetCommandLog()
        {
            var sbMessage = new StringBuilder(100);

            sbMessage.AppendLine(GetCommandTextLogString());

            if (Parameters != null)
            {
                foreach (var parameter in Parameters.OfType<IDbDataParameter>())
                {
                    sbMessage.AppendLine(GetParameterLogString(parameter));
                }
            }
            return sbMessage.ToString();
        }

        protected virtual string GetCommandTextLogString()
        {
            return CommandText ?? "<null>";
        }

        protected virtual string GetParameterLogString(IDbDataParameter parameter)
        {
            // -- Name: [Value] (Type = {}, Direction = {}, IsNullable = {}, Size = {}, Precision = {} Scale = {})
            var builder = new StringBuilder();
            builder.Append("-- ")
                .Append(parameter.ParameterName)
                .Append(": '")
                .Append((parameter.Value == null || parameter.Value == DBNull.Value) ? "null" : parameter.Value)
                .Append("' (Type = ");

            try
            {
                builder.Append(parameter.DbType);
            }
            catch (Exception ex)
            {
                builder.Append($"!Error getting DbType ({ex.Message})!");
            }

            if (parameter.Direction != ParameterDirection.Input)
            {
                builder.Append(", Direction = ").Append(parameter.Direction);
            }

            if (!parameter.IsNullable)
            {
                builder.Append(", IsNullable = false");
            }

            if (parameter.Size != 0)
            {
                builder.Append(", Size = ").Append(parameter.Size);
            }

            if (parameter.Precision != 0)
            {
                builder.Append(", Precision = ").Append(parameter.Precision);
            }

            if (parameter.Scale != 0)
            {
                builder.Append(", Scale = ").Append(parameter.Scale);
            }

            builder.Append(")").Append(Environment.NewLine);

            return builder.ToString();
        }
    }
}
