using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbConnection.Serilog
{
    public class LoggingConnectionWrapper : System.Data.Common.DbConnection, IDisposable
    {
        private readonly System.Data.Common.DbConnection _WrappedDbConnection;
        private readonly ILogger _Logger;

        public LoggingConnectionWrapper(System.Data.Common.DbConnection dbConnection, ILogger logger)
        {
            Contract.Requires(dbConnection != null, nameof(dbConnection) + " is null.");
            Contract.Requires(logger != null, nameof(logger) + " is null.");
            _WrappedDbConnection = dbConnection;
            _Logger = logger;
        }

        public override string ConnectionString
        {
            get { return _WrappedDbConnection.ConnectionString; }
            set { _WrappedDbConnection.ConnectionString = value; }
        }

        public override int ConnectionTimeout
        {
            get { return _WrappedDbConnection.ConnectionTimeout; }
        }

        public override string Database
        {
            get { return _WrappedDbConnection.Database; }
        }

        public override ConnectionState State
        {
            get { return _WrappedDbConnection.State; }
        }

        public override string DataSource
        {
            get { return _WrappedDbConnection.DataSource; }
        }

        public override string ServerVersion
        {
            get { return _WrappedDbConnection.ServerVersion; }
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel il)
        {
            return _WrappedDbConnection.BeginTransaction(il);
        }

        public override void ChangeDatabase(string databaseName)
        {
            _WrappedDbConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            _WrappedDbConnection.Close();
        }

        protected override DbCommand CreateDbCommand()
        {
            return new LoggingCommandWrapper(_WrappedDbConnection.CreateCommand(), _Logger.ForContext<LoggingCommandWrapper>());
        }

        void IDisposable.Dispose()
        {
            _WrappedDbConnection.Dispose();
        }

        public override void Open()
        {
            _WrappedDbConnection.Open();
        }
    }
}
