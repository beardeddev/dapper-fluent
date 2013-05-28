## Dapper Fluent API

Dapper.Fluent is a small and easy library that supports fluent API for query construction and execution over database connection using [Dapper.Net](https://github.com/SamSaffron/dapper-dot-net). 

### Core Features

- Fluent interface, an object oriented API that aims to provide readable code.
- Full control over database connection, commands, parameters, transaction with one context.
- IDataReader, IDictionary, NonQuery, POCO, POCO collections and multiple result sets mapping.
- Implementation of API close to [BLToolkit](http://bltoolkit.net) ORM API.

### Overview
**Fluent API**
```csharp
IDbManager SetCommand(string commandText);
IDbManager SetCommand(string commandText, object parameters);
IDbManager SetSpCommand(string commandText);
IDbManager SetSpCommand(string commandText, object parameters);
IDbManager AddParameter(string name, object value);
IDbManager AddParameter(string name, DbType dbType, ParameterDirection direction);
IDbManager AddParameter(string name, object value, DbType dbType, ParameterDirection direction);
IDbManager AddParameter(string name, object value, DbType dbType, ParameterDirection direction, int? size);
```
**Transactions support**
```csharp
void BeginTransaction();
void BeginTransaction(IsolationLevel isolationLevel);
void CommitTransaction();
void RollbackTransaction();
```
**Querying**
```csharp
T ExecuteObject<T>() where T : class;
IEnumerable<T> ExecuteList<T>() where T : class;
Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> ExecuteMultiple<T1, T2, T3>();
Tuple<IEnumerable<T1>, IEnumerable<T2>> ExecuteMultiple<T1, T2>();
object Execute();
int ExecuteNonQuery();
IDataReader ExecuteReader();
IDataReader ExecuteReader(CommandBehavior behavior);
IDictionary ExecuteDictionary();
```

### Examples usage

**Setting up fluent API context:**
```csharp
DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);
IDbConnection connection = factory.CreateConnection();
IDbManager dbManager = new DbManager(connection);
```
**Querying for posts by id using stored procedure:**
```csharp
dbManager.SetSpCommand("sp_Posts_FindById", new { @id = id })
    .ExecuteObject<Post>();
```

**Querying for posts:**

```csharp
dbManager.SetCommand("SELECT * FROM [dbo].[Posts] WHERE [PublishedOn] BETWEEN @StartDate AND @EndDate AND [Status] = @Status")
    .AddParameter("StartDate", DateTime.UtcNow.AddDays(-7))
    .AddParameter("EndDate", DateTime.UtcNow)
    .AddParameter("Status", (byte)Status.Active)
    .ExecuteList<Post>();
```

### TODO:
- Implement unit tests
- Add example of using IOC, best practices
