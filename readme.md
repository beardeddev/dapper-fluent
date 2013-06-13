## Dapper Fluent API

Dapper.Fluent is a small and easy library that supports fluent API for query construction and execution over database connection using [Dapper.Net](https://github.com/SamSaffron/dapper-dot-net). 

### Core Features

- Fluent interface, an object oriented API that aims to provide readable code.
- Usage the full power of [Dapper.Net](https://github.com/SamSaffron/dapper-dot-net) in elegant way.
- Implementation of API close to [BLToolkit](http://bltoolkit.net) ORM API.

### Overview
**Fluent API**
```csharp
IDbManager SetCommand(string commandText);
IDbManager SetCommand(string commandText, object parameters);
IDbManager SetSpCommand(string commandText);
IDbManager SetSpCommand(string commandText, object parameters);
IDbManager SetParameter(string name, object value);
IDbManager SetParameter(string name, DbType dbType, ParameterDirection direction);
IDbManager SetParameter(string name, object value, DbType dbType, ParameterDirection direction);
IDbManager SetParameter(string name, object value, DbType dbType, ParameterDirection direction, int? size);
IDbManager SetParameters(object value);

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
int Execute();
T ExecuteObject<T>() where T : class;
IEnumerable<T> ExecuteList<T>() where T : class;
Tuple<IEnumerable<T1>, IEnumerable<T2>> ExecuteMultiple<T1, T2>();
Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> ExecuteMultiple<T1, T2, T3>();
IEnumerable<TResult> ExecuteMapping<T1, T2, TResult>(Func<T1, T2, TResult> map, string splitOn);
IEnumerable<TResult> ExecuteMapping<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> map, string splitOn);
IEnumerable<TResult> ExecuteMapping<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> map, string splitOn);
IEnumerable<TResult> ExecuteMapping<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> map, string splitOn);
IEnumerable<TResult> ExecuteMapping<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> map, string splitOn);
```

### Examples usage

**Setting up fluent API context:**

```csharp
DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);
IDbConnection connection = factory.CreateConnection();

```
**Querying for post by id using stored procedure:**
```csharp
using(IDbManager dbManager = new DbManager(connection))
{
	Post post = dbManager.SetSpCommand("sp_Posts_FindById", new { @id = id })
    				.ExecuteObject<Post>();
}    
```

**Querying for posts published for the last 7 days:**

```csharp
using(IDbManager dbManager = new DbManager(connection))
{
	IEnumerable<Post> posts = dbManager.SetCommand("SELECT * FROM [dbo].[Posts] WHERE [PublishedOn] BETWEEN @StartDate AND @EndDate AND [Status] = @Status")
    							.SetParameter("StartDate", DateTime.UtcNow.AddDays(-7))
    							.SetParameter("EndDate", DateTime.UtcNow)
    							.SetParameter("Status", (byte)Status.Active)
    							.ExecuteList<Post>();
}
```

