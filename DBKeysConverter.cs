using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace KeysConversion
{
    internal class DBKeysConverter
    {
        private string connectionString;
        private KeysConversion.Converter converter;
        private bool test = true;
        public DBKeysConverter(string localKey, string salt, string zmk, string zmkSBlock, string connString, string HSMIP, int? HSMPort)
        {
            connectionString = connString;
            converter = new KeysConversion.Converter
            (
            localKey,
            salt,
            zmk,
            zmkSBlock,
            //"127.0.0.1",
            HSMIP,
            //7777
            HSMPort
            );
        }
        public void generateCommands()
        {
            int counter = 0;
            SqlConnection updConnection = new SqlConnection(connectionString);
            updConnection.Open();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string convertedKey;
                string cmd = "";
                byte[] kbe;
                //string queryString = "SELECT pk, str_bdk_pin, str_ksn_pin, str_reader_sn FROM [cardport].[dbo].[reader] with(nolock) where str_reader_type = 'DSPREAD_QPOSMINI' and str_pin_mode = 'SOFT' and str_bdk_pin is not null and str_ksn_pin is not null and str_reader_sn ='27000409118062001083'";
                string queryString = "SELECT pk, str_bdk_pin, str_ksn_pin, str_reader_sn FROM [cardport].[dbo].[reader] with(nolock) where str_reader_type = 'DSPREAD_QPOSMINI' and str_pin_mode = 'SOFT' and str_bdk_pin is not null and str_ksn_pin is not null and str_reader_sn is not null";// and str_reader_sn ='27000209117122701331'";

                connection.Open();
                Console.WriteLine("Connection to DB is established");
                SqlCommand selectCommand = new SqlCommand(queryString, connection);
                SqlDataReader reader = selectCommand.ExecuteReader();

                if (reader.HasRows) 
                {
                    Console.WriteLine("Start reading data from DB");
                    //Console.WriteLine("{0}\t{1}\t{2}", reader.GetName(0), reader.GetName(1), reader.GetName(2));
                    while (reader.Read()) 
                    {
                        object row1 = reader.GetValue(0);
                        string row2 = reader.GetValue(1).ToString();
                        string row3 = reader.GetValue(2).ToString();
                        string row4 = reader.GetValue(3).ToString();

                        //Console.WriteLine("Row processing...");
                        convertedKey = converter.convertKey(row2, test);

                        kbe = ASCIIEncoding.ASCII.GetBytes(convertedKey);
                        Console.WriteLine("Executing command");
                        cmd = "UPDATE [cardport].[dbo].[reader] SET str_bdk_pin='" + converter.encryptKey(KeysConversion.Tools.ByteArrayToHexString(KeysConversion.Tools.applyMultiplicity(kbe, 16, 0x0E))) + "', str_pin_mode = 'HSM_RECRYPT' WHERE pk=" + row1.ToString() + " AND str_reader_type = 'DSPREAD_QPOSMINI' and str_reader_sn='" + row4.ToString() + "'";
                        
                        SqlCommand updCommand = new SqlCommand();
                        updCommand.Connection = updConnection;

                        Console.WriteLine(cmd);
                        updCommand.CommandText = cmd;
                        updCommand.ExecuteNonQuery();

                        Console.WriteLine("--ROLLBACK-- UPDATE [cardport].[dbo].[reader] SET str_bdk_pin='" + row2 + "', str_pin_mode = 'SOFT' WHERE pk=" + row1.ToString() + " AND str_reader_type = 'DSPREAD_QPOSMINI' and str_reader_sn='" + row4.ToString() + "'");
                        counter = counter + 1;
                    }
                }
                else
                {
                    Console.WriteLine("No data in DB");
                }
                reader.Close();
                updConnection.Close();
                Console.WriteLine("Rows processed: " + counter.ToString());
            }
        }
    }
}
