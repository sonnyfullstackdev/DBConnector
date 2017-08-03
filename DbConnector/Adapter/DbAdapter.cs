using DbConnector.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbConnector.Adapter
{
    public class DbAdapter : IDbAdapter
    {
        public DbAdapter(IDbCommand dbCommand, IDbConnection dbConnection)
        {
            DbCommand = dbCommand;
            DbConnection = dbConnection;
        }

        public IDbCommand DbCommand { get; private set; }
        public IDbConnection DbConnection { get; private set; }

        int _commandTimeout = 5000;
        public int CommandTimeout
        {
            get { return _commandTimeout; }
            set { _commandTimeout = value; }
        }

        public IEnumerable<T> LoadObject<T>(IDbCommandDef commandDef) where T : class
        {
            try
            {
                if (commandDef == null)
                    throw new ArgumentException("Missing Db Command Def");

                List<T> itms = new List<T>();
                using (IDbConnection conn = DbConnection)
                using (IDbCommand cmd = DbCommand)
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.CommandType = commandDef.DbCommandType;
                    cmd.CommandText = commandDef.DbCommandText;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Connection = conn;
                    if (commandDef.DbParameters != null)
                    {
                        foreach (IDbDataParameter param in commandDef.DbParameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                    }
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itms.Add(FillItem<T>(reader));
                    }

                }

                return itms;
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<T> LoadObject<T>(IDbCommandDef commandDef, Func<IDataReader, T> mapper) where T : class
        {
            try
            {
                if (commandDef == null)
                    throw new ArgumentException("Missing Db Command Def");

                List<T> itms = new List<T>();
                using (IDbConnection conn = DbConnection)
                using (IDbCommand cmd = DbCommand)
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.CommandType = commandDef.DbCommandType;
                    cmd.CommandText = commandDef.DbCommandText;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Connection = conn;
                    if (commandDef.DbParameters != null)
                    {
                        foreach (IDbDataParameter param in commandDef.DbParameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                    }
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itms.Add(mapper(reader));
                    }

                }

                return itms;
            }
            catch
            {
                throw;
            }
        }

        public int ExecuteQuery(IDbCommandDef commandDef)
        {
            try
            {
                using (IDbConnection conn = DbConnection)
                using (IDbCommand cmd = DbCommand)
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.CommandType = commandDef.DbCommandType;
                    cmd.CommandText = commandDef.DbCommandText;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Connection = conn;
                    if (commandDef.DbParameters != null)
                    {
                        foreach (IDbDataParameter param in commandDef.DbParameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
        }


        public object ExecuteDbScalar(IDbCommandDef commandDef)
        {
            try
            {
                using (IDbConnection conn = DbConnection)
                using (IDbCommand cmd = DbCommand)
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }
                    cmd.CommandType = commandDef.DbCommandType;
                    cmd.CommandText = commandDef.DbCommandText;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Connection = conn;
                    if (commandDef.DbParameters != null)
                    {
                        foreach (IDbDataParameter param in commandDef.DbParameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                    }
                    return cmd.ExecuteScalar();
                }
            }
            catch { throw; }
        }
        //TUPLE
        private Tuple<List<string>, System.Reflection.PropertyInfo[]> GetDefs2<T>(IDataReader reader)
        {
            string className = typeof(T).Name;
            if (ObjCache.Instance.HasCache(className))
                return ObjCache.Instance.Get<Tuple<List<string>, System.Reflection.PropertyInfo[]>>(className);
            Tuple<List<string>, System.Reflection.PropertyInfo[]> vals = new Tuple<List<string>, System.Reflection.PropertyInfo[]>(reader.GetSchemaTable()
                .Rows.Cast<DataRow>().Select(c => c["ColumnName"]
                .ToString().ToLower()).ToList(), typeof(T).GetProperties());
            ObjCache.Instance.Set(className, vals);
            return vals;
        }


        protected T FillItem<T>(IDataReader reader) where T : class
        {
            ReturnObject rcd = CachingData<T>(reader);
            List<string> colnames = rcd.ColumnNames; /*reader.GetSchemaTable().Rows.Cast<DataRow>().Select(c => c["ColumnName"].ToString().ToLower()).ToList();*/

            System.Reflection.PropertyInfo[] props = rcd.Properties;/*typeof(T).GetProperties();*/
            T obj = Activator.CreateInstance<T>();

            foreach (System.Reflection.PropertyInfo prop in props)
            {
                if (colnames.Contains(prop.Name.ToLower()))
                {
                    if (reader[prop.Name] != DBNull.Value)
                    {
                        if (reader[prop.Name].GetType() == typeof(decimal))
                        {
                            prop.SetValue(obj, (reader.GetDouble(prop.Name)), null);
                        }
                        else
                        {
                            prop.SetValue(obj, (reader.GetValue(reader.GetOrdinal(prop.Name)) ?? null), null);
                        }
                    }
                }
            }
            return obj;
        }

        private ReturnObject CachingData<T>(IDataReader reader)
        {
            string name = typeof(T).Name;

            if (ObjCache.Instance.HasCache(name))
                return ObjCache.Instance.Get<ReturnObject>(name);

            ReturnObject returnObj = new ReturnObject()
            {
                ColumnNames = reader.GetSchemaTable()
                .Rows.Cast<DataRow>().Select(c => c["ColumnName"].ToString().ToLower()).ToList(),
                Properties = typeof(T).GetProperties()
            };
            ObjCache.Instance.Set(name, returnObj);
            return returnObj;

            //returnObj.KeyValue = name;
            //    returnObj.ColNames = colnames;
            //    returnObj.Props = props;
            //return ObjCache.Instance.Get<ReturnObject>(name);

        }
    }
}
