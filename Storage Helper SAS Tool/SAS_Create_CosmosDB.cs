using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Azure.Cosmos.Table;


namespace Storage_Helper_SAS_Tool
{
    /// <summary>
    /// Class to deal with Table SAS - CosmoDB SDK
    /// 
    /// SharedAccessTablePolicy  // Microsoft.Azure.Cosmos.Table
    /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-service-sas-create-dotnet#create-a-service-sas-for-a-blob-container
    /// </summary>
    class SAS_Create_CosmosDB
    {


        public static bool Regenerate_ServiceSAS_Table(Label labelTableName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxTableName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelTableName, textBoxTableName.Text, "Missing Table Name", "Error")) return false;

            string sas = Get_ServiceSAS_Table(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxTableName.Text, textBoxPolicyName.Text);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text = "Regenerated Service SAS - Table:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Table (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            BoxAuthResults.Text += "Table URI:\n" + "https://" + textBoxAccountName.Text + ".table.core.windows.net/" + textBoxTableName.Text + Uri.UnescapeDataString(sas) + "\n\n";


            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }




        /// <summary>
        /// Create a service SAS for a Table - SharedAccessTablePolicy - Microsoft.Azure.Cosmos.Table
        /// ---> Using the 'Microsoft.Azure.Cosmos.Table' library <---
        /// SharedAccessTablePolicy: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.table.sharedaccesstablepolicy?view=azure-dotnet
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <param name="tableName"></param>
        /// <param name="policyName"></param>
        /// <returns> queue.Uri + SAS Token</returns>
        public static string Get_ServiceSAS_Table(string accountName, string accountKey, string tableName, string policyName = null)
        {
            //--------------------------------------------------
            // Creating StorageCredentials and table references using the 'Microsoft.Azure.Cosmos.Table' library
            string tableUri = string.Format("https://{0}.table.core.windows.net", accountName);
            StorageCredentials storageCredentials = new StorageCredentials(accountName, accountKey);
            StorageUri uri = new StorageUri(new Uri(tableUri));
            CloudTableClient tableClient = new CloudTableClient(uri, storageCredentials);
            CloudTable table = tableClient.GetTableReference(tableName);
            //--------------------------------------------------

            // Create a new access policy and define its constraints, using the 'Microsoft.Azure.Cosmos.Table' library.
            // Note that the SharedAccessTablePolicy class is used both to define the parameters of an ad hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            Microsoft.Azure.Cosmos.Table.SharedAccessTablePolicy tablePolicy;
            try
            {
                tablePolicy = new SharedAccessTablePolicy()
                {
                    Permissions = Set_PermissionsFromStr_ServiceSAS_Tables(),
                    // SharedAccessStartTime = 
                    SharedAccessExpiryTime = SAS_Utils.SAS.seDateTime
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    tablePolicy.SharedAccessStartTime = SAS_Utils.SAS.stDateTime;
                //-----------------------------------------------------------
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on generating the Table Service SAS\n" + ex, "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }

            if (String.IsNullOrEmpty(policyName))
            {
                // Generate the shared access signature on the table, setting the constraints directly on the signature.
                //return tableUri + table.GetSharedAccessSignature(blobPolicy);
                return table.GetSharedAccessSignature(tablePolicy);
            }
            else
            {
                // Generate the shared access signature on the table. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                return table.GetSharedAccessSignature(tablePolicy, policyName);
            }
        }




        /// <summary>
        /// Set Permissions From String, for Service SAS - Tables
        /// ---> Using the 'Microsoft.Azure.Cosmos.Table' library <---
        ///      "raud"
        /// </summary>
        /// <returns></returns>
        private static SharedAccessTablePermissions Set_PermissionsFromStr_ServiceSAS_Tables()
        {
            string sp = SAS_Utils.SAS.sp.v;
            SharedAccessTablePermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? SharedAccessTablePermissions.Query : 0;
            Permissions |= (sp.IndexOf("a") != -1) ? SharedAccessTablePermissions.Add : 0;
            Permissions |= (sp.IndexOf("u") != -1) ? SharedAccessTablePermissions.Update : 0;
            Permissions |= (sp.IndexOf("d") != -1) ? SharedAccessTablePermissions.Delete : 0;

            return Permissions;
        }
    }
}
