using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nr_status
{
    class OraCtl
    {
        
        /// <summary>
        /// 获取NR标识
        /// </summary>
        /// <param name="dateString">日期yyyymmdd</param>
        /// <param name="oraId">数据库名</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string NrStatusCheck(string dateString, string oraId, string username, string password)
        {
            string oraStr = string.Format("Persist Security Info=False;Data Source={0};User ID={1};password={2}", oraId, username, password);
            using (OracleConnection oraConn = new OracleConnection(oraStr))
            {
                string result="n/a";
                try
                {
                    oraConn.Open();
                    string queryString = string.Format("select flag from nr_status where run_date=to_date('{0}','yyyymmdd')", dateString);
                    OracleCommand oraComm = new OracleCommand(queryString, oraConn);
                    var rd = oraComm.ExecuteOracleScalar();
                    result = rd.ToString();
                    //oraComm.Dispose();
                    //oraConn.Close();
                }
                catch (Exception ex)
                {
                    result = oraId + " ";
                    result += ex.Message.ToString();
                    if ((ex.ToString()).IndexOf("NullReferenceException") != -1)
                    {
                        result = "no row selected";
                    }
                    if ((ex.ToString()).IndexOf("ORA-12535") != -1)
                    {
                        result = "TNS超时";
                    }
                    //throw;
                } 
                return result;
            }
        }
    }
}
