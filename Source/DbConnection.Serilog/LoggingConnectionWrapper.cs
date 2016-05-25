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
        private readonly ILogger _Logger;
        private System.Data.Common.DbConnection WrappedDbConnection { get; }

        public LoggingConnectionWrapper(System.Data.Common.DbConnection dbConnection, ILogger logger)
        {
            Contract.Requires(dbConnection != null, nameof(dbConnection) + " is null.");
            Contract.Requires(logger != null, nameof(logger) + " is null.");
            WrappedDbConnection = dbConnection;
            _Logger = logger;
        }

        public override string ConnectionString
        {
            get { return WrappedDbConnection.ConnectionString; }
            set { WrappedDbConnection.ConnectionString = value; }
        }

        public override int ConnectionTimeout => WrappedDbConnection.ConnectionTimeout;

        public override string Database => WrappedDbConnection.Database;

        public override ConnectionState State => WrappedDbConnection.State;

        public override string DataSource => WrappedDbConnection.DataSource;

        public override string ServerVersion => WrappedDbConnection.ServerVersion;

        protected override DbTransaction BeginDbTransaction(IsolationLevel il) => WrappedDbConnection.BeginTransaction(il);

        public override void ChangeDatabase(string databaseName) => WrappedDbConnection.ChangeDatabase(databaseName);        

        public override void Close() => WrappedDbConnection.Close();

        protected override DbCommand CreateDbCommand() => new LoggingCommandWrapper(WrappedDbConnection.CreateCommand(), _Logger.ForContext<LoggingCommandWrapper>());

        void IDisposable.Dispose() => WrappedDbConnection.Dispose();

        public override void Open() => WrappedDbConnection.Open();
    }
}
