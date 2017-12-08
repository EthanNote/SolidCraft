using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HyperDB
{
    public class QueryResult
    {
        public DBNode Result { get; private set; }
        public bool Succeed { get; private set; }
        public string Message { get; private set; }
        public QueryResult(DBNode result, bool match, string message=null)
        {
            Result = result;
            Succeed = match;
            Message = message;
        }
    }

}
