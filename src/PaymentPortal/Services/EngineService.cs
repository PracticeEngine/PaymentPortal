using PaymentPortal.Models;
using PEPaymentProvider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PaymentPortal.Services
{
    /// <summary>
    /// Access to PE Database Calls
    /// </summary>
    public class EngineService
    {

        private readonly SqlConnection connection;

        public EngineService()
        {
            var constr = ConfigurationManager.ConnectionStrings["EngineDb"].ConnectionString;
            this.connection = new SqlConnection(constr);
        }

        /// <summary>
        /// Returns the ContIndex if known, otherwise returns null
        /// </summary>
        /// <param name="link"></param>
        /// <param name="clientcode"></param>
        /// <returns></returns>
        public int? ValidateCredential(Guid link, string clientcode)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PP_Validate_Login @Guid, @ClientCode";
                cmd.Parameters.AddWithValue("@Guid", link);
                cmd.Parameters.AddWithValue("@ClientCode", clientcode);
                cmd.Connection.Open();
                try
                {
                    var result = cmd.ExecuteScalar();

                    return (int?)(!Convert.IsDBNull(result) ? result : null);
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Returns the Client Details
        /// </summary>
        /// <param name="ContIndex"></param>
        /// <returns></returns>
        public ClientDetails GetClientDetails(int ContIndex)
        {
            ClientDetails result = null;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PP_Client_Details @ContIndex";
                cmd.Parameters.AddWithValue("@ContIndex", ContIndex);
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    result = DataReaderFirstObject<ClientDetails>(reader);
                }
            }
            return result;
        }
        
        /// <summary>
        /// Sets Payment Message on the Collection Details
        /// </summary>
        /// <param name="ContIndex">The Client's ContIndex</param>
        /// <param name="invoiceCSV">CSV of the Selected Invoices</param>
        /// <param name="provider">The Name of the Provider</param>
        public void SetPaymentMessage(int ContIndex, string invoiceCSV, string provider)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PP_Collection_Message @ContIndex, @Invoices, @Provider";
                cmd.Parameters.AddWithValue("@ContIndex", ContIndex);
                cmd.Parameters.AddWithValue("@Invoices", invoiceCSV);
                cmd.Parameters.AddWithValue("@Provider", provider);
                cmd.Connection.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Returns the Invoice Details
        /// </summary>
        /// <param name="ContIndex"></param>
        /// <returns></returns>
        public IEnumerable<InvoiceDetails> GetInvoiceDetails(int ContIndex)
        {
            IEnumerable<InvoiceDetails> result = new InvoiceDetails[0];
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PP_Invoice_Details @ContIndex";
                cmd.Parameters.AddWithValue("@ContIndex", ContIndex);
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    result = DataReaderToList<InvoiceDetails>(reader);
                }
            }
            return result;
        }

        /// <summary>
        /// Generic Helper Method to Materialize Poco Objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        private IList<T> DataReaderToList<T>(IDataReader dr)
        {
            IList<T> list = new List<T>();
            T obj = default(T);

            bool primed = false;
            PropertyInfo[] props = null;
            IList<string> names = null;

            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                if (!primed)
                {
                    props = obj.GetType().GetProperties();
                    names = new List<string>();

                    for (var f = 0; f < dr.FieldCount; f++)
                    {
                        names.Add(dr.GetName(f));
                    }
                }
                foreach (PropertyInfo prop in props)
                {
                    if (names.Contains(prop.Name))
                    {
                        if (!object.Equals(dr[prop.Name], DBNull.Value))
                        {
                            prop.SetValue(obj, dr[prop.Name], null);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        /// Generic Helper Method to Materialize a single Poco Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        private T DataReaderFirstObject<T>(IDataReader dr)
        {
            T obj = default(T);

            bool primed = false;
            PropertyInfo[] props = null;
            IList<string> names = null;

            if (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                if (!primed)
                {
                    props = obj.GetType().GetProperties();
                    names = new List<string>();

                    for (var f = 0; f < dr.FieldCount; f++)
                    {
                        names.Add(dr.GetName(f));
                    }
                }
                foreach (PropertyInfo prop in props)
                {

                    if (names.Contains(prop.Name))
                    {
                        if (!object.Equals(dr[prop.Name], DBNull.Value))
                        {
                            prop.SetValue(obj, dr[prop.Name], null);
                        }
                    }
                }
            }
            return obj;
        }
    }
}