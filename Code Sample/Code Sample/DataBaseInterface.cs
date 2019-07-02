using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Configuration;
using System.Data;
using Dapper;
using System.Linq;
using System.Threading;


namespace Code_Sample
{
    class DataBaseInterface
    {
        public static List<KeyValuePair<string, int>> GetMostCommonEmailAddress()
        {

            Dictionary<string, int> domainCounter = new Dictionary<string, int>();
            List<string> emails = GetEmailAddresses();
            foreach(string email in emails)
            {
                string domain = email.Split('@')[1];
                if (domainCounter.ContainsKey(domain))
                    domainCounter[domain]++;
                else
                    domainCounter.Add(domain, 1);
            }
            List<KeyValuePair<string, int>> domainCounterList = domainCounter.ToList();

       
            domainCounterList = domainCounterList.AsParallel().OrderByDescending(x => x.Value).ToList();
            //domainCounterList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
    
            return domainCounterList;
        }
        public static List<string> GetCountys()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("SELECT county FROM person");
                return output.ToList();
            }
        }
        public static List<string> GetPostalsByInCounty(string county)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("SELECT postal FROM person where county = '"+county+"'");
                return output.ToList();
            }
        }
        public static List<KeyValuePair<string,string>> GetCountysWithPostcodes()
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                string statement = "SELECT postal , county FROM person";
                List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();
                using (SQLiteCommand cmd = new SQLiteCommand(statement, cnn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            data.Add(new KeyValuePair<string, string>(rdr.GetString(0), rdr.GetString(1)));
                        }
                    }
                }
                return data;
            }
        }
        public static List<string> GetAllPostCode()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("SELECT postal FROM person");
                return output.ToList();

            }
        }
        public static List<string> GetEmailAddresses()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("select email from person");
                return output.ToList();

            }
        }


        private static string LoadConnectionString(String id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
