using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Dapper
{
    partial class DynamicParameters
    {
        public IEnumerable<IDbDataParameter> GetAttachedParams(IDbCommand dbCommand)
        {
            foreach(ParamInfo info in this.parameters.Values)
            {
                if (info.AttachedParam != null)
                {
                    yield return info.AttachedParam;
                }
                else
                {
                    IDbDataParameter dbParameter = dbCommand.CreateParameter();
                    dbParameter.ParameterName = info.Name;
                    dbParameter.Value = info.Value;
                    dbParameter.Direction = info.ParameterDirection;

                    if (info.Size.HasValue)
                        dbParameter.Size = info.Size.Value;

                    if(info.DbType.HasValue)
                        dbParameter.DbType = info.DbType.Value;

                    yield return dbParameter;
                }
            }
        }
    }
}
