using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.File;
using Microsoft.Azure.Storage.Queue;
//using Microsoft.Azure.Cosmos.Table;    // Used with full path library - many Storage definitions ambiguos with Cosmos - 

namespace Storage_Helper_SAS_Tool
{
    /// <summary>
    /// You can sign a SAS in one of two ways:
    ///   - With a key created using Azure Active Directory(Azure AD) credentials. A SAS signed with Azure AD credentials is a user delegation SAS.
    ///   - With the storage account key. Both a service SAS and an account SAS are signed with the storage account key.
    /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-user-delegation-sas-create-dotnet#example-get-a-user-delegation-sas
    /// 
    /// Create an account SAS
    ///     SharedAccessAccountPolicy
    /// https://docs.microsoft.com/en-us/azure/storage/common/storage-account-sas-create-dotnet#create-an-account-sas 
    /// 
    /// Create a service SAS for a blob or container
    ///    SharedAccessBlobPolicy
    ///    SharedAccessFilePolicy 
    ///    SharedAccessQueuePolicy
    ///    SharedAccessTablePolicy  // Microsoft.Azure.CosmosDB.Table
    /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-service-sas-create-dotnet#create-a-service-sas-for-a-blob-container
    /// 
    /// Create a user delegation SAS 
    /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-user-delegation-sas-create-dotnet#example-get-a-user-delegation-sas
    /// 
    /// Storage Accounts - List Account SAS
    /// https://docs.microsoft.com/en-us/rest/api/storagerp/storageaccounts/listaccountsas
    /// 
    /// </summary>

    class SAS_Create_v11
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textBoxAccountName"></param>
        /// <param name="textBoxAccountKey1"></param>
        /// <param name="BoxAuthResults"></param>
        public static void Regenerate_AccountSAS(TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox BoxAuthResults, string ss)
        {
            string sas = Get_AccountSAS(textBoxAccountName.Text, textBoxAccountKey1.Text);
            if (sas == "") return;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Account SAS token:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Account SAS token (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            if (ss.IndexOf("b") != -1)
                BoxAuthResults.Text += "Blob URI:\n" + "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n\n";

            if (ss.IndexOf("f") != -1)
                BoxAuthResults.Text += "File URI:\n" + "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n\n";

            if (ss.IndexOf("q") != -1)
                BoxAuthResults.Text += "Queue URI:\n" + "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n\n";

            if (ss.IndexOf("t") != -1)
                BoxAuthResults.Text += "Table URI:\n" + "https://" + textBoxAccountName.Text + ".table.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));
        }



        public static void Regenerate_ServiceSAS_Container(Label labelContainerName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxContainerName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelContainerName, textBoxContainerName.Text, "Missing Container Name", "Error")) { SAS_Utils.SAS.storageAccountName.s = false; return; }

            string sas = Get_ServiceSAS_Container(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxContainerName.Text, textBoxPolicyName.Text);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text = "Regenerated Service SAS - Container:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Container (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            BoxAuthResults.Text += "Container URI:\n" + "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));
        }



        public static void Regenerate_ServiceSAS_Blob(Label labelContainerName, Label labelBlobName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxContainerName, TextBox textBoxBlobName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelContainerName, textBoxContainerName.Text, "Missing Container Name", "Error")) { SAS_Utils.SAS.containerName.s = false; return; }
            if (Utils.StringEmpty(labelBlobName, textBoxBlobName.Text, "Missing Blob Name", "Error")) { SAS_Utils.SAS.blobName.s = false; return; }

            string sas = Get_ServiceSAS_Blob(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxContainerName.Text, textBoxBlobName.Text, textBoxPolicyName.Text);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text = "Regenerated Service SAS - Blob:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Blob (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            BoxAuthResults.Text += "Blob URI:\n" + "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));
        }


        public static void Regenerate_ServiceSAS_Queue(Label labelQueueName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxQueueName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelQueueName, textBoxQueueName.Text, "Missing Queue Name", "Error")) { SAS_Utils.SAS.queueName.s = false; return; }

            string sas = Get_ServiceSAS_Queue(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxQueueName.Text, textBoxPolicyName.Text);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text = "Regenerated Service SAS - Queue:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Queue (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            BoxAuthResults.Text += "Queue URI:\n" + "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + textBoxQueueName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));
        }


        public static void Regenerate_ServiceSAS_File(Label labelShareName, Label labelFileName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxShareName, TextBox textBoxFileName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelShareName, textBoxShareName.Text, "Missing Share Name", "Error")) { SAS_Utils.SAS.shareName.s = false; return; }
            if (Utils.StringEmpty(labelFileName, textBoxFileName.Text, "Missing File Name", "Error")) { SAS_Utils.SAS.fileName.s = false; return; }

            string sas = Get_ServiceSAS_File(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxShareName.Text, textBoxFileName.Text, textBoxPolicyName.Text);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text = "Regenerated Service SAS - File:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - File (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            BoxAuthResults.Text += "File URI:\n" + "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "/" + textBoxFileName.Text + Uri.UnescapeDataString(sas) + "\n\n";


            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));
        }



        public static void Regenerate_ServiceSAS_Share(Label labelShareName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxShareName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelShareName, textBoxShareName.Text, "Missing Share Name", "Error")) { SAS_Utils.SAS.shareName.s = false; return; }

            string sas = Get_ServiceSAS_Share(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxShareName.Text, textBoxPolicyName.Text);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text = "Regenerated Service SAS - Share:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Share (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            BoxAuthResults.Text += "Share URI:\n" + "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));
        }



        public static void Regenerate_ServiceSAS_Table(Label labelTableName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxTableName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelTableName, textBoxTableName.Text, "Missing Table Name", "Error")) return;

            string sas = Get_ServiceSAS_Table(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxTableName.Text, textBoxPolicyName.Text);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text = "Regenerated Service SAS - Table:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Table (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";


            BoxAuthResults.Text += "Table URI:\n" + "https://" + textBoxAccountName.Text + ".table.core.windows.net/" + textBoxTableName.Text + Uri.UnescapeDataString(sas) + "\n\n";


            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));
        }





        public static void Regenerate_ServiceSAS_BlobSnapshot(TextBox BoxAuthResults)
        {
            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Service SAS URI - Blob Snapshot\n";
            BoxAuthResults.Text += "  --> TODO - Not implemeted for Blob Snapshots <--\n";
        }




        //---------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Set Permissions From String, for Account SAS
        ///      r,w,d,l,a,c,u,p
        /// </summary>
        /// <returns></returns>
        private static SharedAccessAccountPermissions Set_PermissionsFromStr_AccountSAS()
        { 
            string sp = SAS_Utils.SAS.sp.v;
            SharedAccessAccountPermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? SharedAccessAccountPermissions.Read : 0;
            Permissions |= (sp.IndexOf("w") != -1) ? SharedAccessAccountPermissions.Write : 0;
            Permissions |= (sp.IndexOf("d") != -1) ? SharedAccessAccountPermissions.Delete : 0;
            Permissions |= (sp.IndexOf("l") != -1) ? SharedAccessAccountPermissions.List : 0;
            Permissions |= (sp.IndexOf("a") != -1) ? SharedAccessAccountPermissions.Add : 0;
            Permissions |= (sp.IndexOf("c") != -1) ? SharedAccessAccountPermissions.Create : 0;
            Permissions |= (sp.IndexOf("u") != -1) ? SharedAccessAccountPermissions.Update : 0;
            Permissions |= (sp.IndexOf("p") != -1) ? SharedAccessAccountPermissions.ProcessMessages : 0;

            return Permissions;
        }


        /// <summary>
        /// Set Services From String, for Account SAS
        ///      b,f,t,q
        /// </summary>
        /// <returns></returns>
        private static SharedAccessAccountServices Set_ServicesFromStr_AccountSAS()
        {
            string ss = SAS_Utils.SAS.ss.v;
            SharedAccessAccountServices Services = 0;
            Services |= (ss.IndexOf("b") != -1) ? SharedAccessAccountServices.Blob : 0;
            Services |= (ss.IndexOf("f") != -1) ? SharedAccessAccountServices.File : 0;
            Services |= (ss.IndexOf("t") != -1) ? SharedAccessAccountServices.Table : 0;
            Services |= (ss.IndexOf("q") != -1) ? SharedAccessAccountServices.Queue : 0;

            return Services;
        }


        /// <summary>
        /// Set Resource Type From String, for Account SAS
        ///      s,c,o
        /// </summary>
        /// <returns></returns>
        private static SharedAccessAccountResourceTypes Set_ResourceTypesFromStr_AccountSAS()
        {
            string srt = SAS_Utils.SAS.srt.v;
            SharedAccessAccountResourceTypes ResourceTypes = 0;
            ResourceTypes |= (srt.IndexOf("s") != -1) ? SharedAccessAccountResourceTypes.Service : 0;
            ResourceTypes |= (srt.IndexOf("c") != -1) ? SharedAccessAccountResourceTypes.Container : 0;
            ResourceTypes |= (srt.IndexOf("o") != -1) ? SharedAccessAccountResourceTypes.Object : 0;

            return ResourceTypes;
        }


        /// <summary>
        /// Set Protocols From String, for Account SAS
        ///      https, http,https
        /// </summary>
        /// <returns></returns>
        private static SharedAccessProtocol Set_ProtocolsFromStr_AccountSAS()
        {
            string spr = SAS_Utils.SAS.spr.v;
            SharedAccessProtocol Protocols = 0;

            // both protocols provided on spr 
            if (spr.IndexOf("http,https") != -1 || spr.IndexOf("https,http") != -1)
                Protocols = SharedAccessProtocol.HttpsOrHttp;
            else
                // only https provided on spr (http only is not supported)
                Protocols = SharedAccessProtocol.HttpsOnly; 

            return Protocols;
        }


        
        /// <summary>
        /// Set IP Address From String, for Account SAS
        ///      IPAddress,Range
        /// </summary>
        /// <returns></returns>
        private static IPAddressOrRange Set_IPAddressFromStr_AccountSAS()
        {
            string sip = SAS_Utils.SAS.sip.v;
            IPAddressOrRange IPAddress = null;

            if(sip == "not found" || String.IsNullOrEmpty(sip))
                return IPAddress;

            int i = sip.IndexOf("-");
            if (i != -1)  // '-' encontrado - Range
                IPAddress = new IPAddressOrRange(sip.Substring(0, i), sip.Substring(i + 1));
            else
                IPAddress = new IPAddressOrRange(sip);

            return IPAddress;
        }


        /// <summary>
        /// Create an account SAS - SharedAccessAccountPolicy
        /// https://docs.microsoft.com/en-us/azure/storage/common/storage-account-sas-create-dotnet#create-an-account-sas 
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <returns> SAS Token</returns>
        public static string Get_AccountSAS(string accountName, string accountKey)
        {
            //--------------------------------------------------
            // To create the account SAS, you need to use Shared Key credentials. Modify for your account.
            string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            //--------------------------------------------------

            // Create a new access policy for the account.
            SharedAccessAccountPolicy accountPolicy;
            try
            {
                accountPolicy = new SharedAccessAccountPolicy()
                {
                    Permissions =   Set_PermissionsFromStr_AccountSAS(),
                    Services =      Set_ServicesFromStr_AccountSAS(),
                    ResourceTypes = Set_ResourceTypesFromStr_AccountSAS(),
                    // SharedAccessStartTime = 
                    SharedAccessExpiryTime = SAS_Utils.SAS.seDateTime,
                    // Protocols = 
                    // IPAddressOrRange = 
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    accountPolicy.SharedAccessStartTime = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    accountPolicy.Protocols = Set_ProtocolsFromStr_AccountSAS();

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    accountPolicy.IPAddressOrRange = Set_IPAddressFromStr_AccountSAS();
                //-----------------------------------------------------------
            }
            catch (Exception ex)
            { 
                MessageBox.Show("Error on generating the Account SAS\n" + ex, "Account SAS error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }
            // Return the SAS token.
            return storageAccount.GetSharedAccessSignature(accountPolicy);
        }


        /// <summary>
        /// Set Permissions From String, for Service SAS - Containers
        ///      'rwdacl'
        /// </summary>
        /// <returns></returns>
        private static SharedAccessBlobPermissions Set_PermissionsFromStr_ServiceSAS_Container()
        {
            string sp = SAS_Utils.SAS.sp.v;
            SharedAccessBlobPermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? SharedAccessBlobPermissions.Read : 0;
            Permissions |= (sp.IndexOf("w") != -1) ? SharedAccessBlobPermissions.Write : 0;
            Permissions |= (sp.IndexOf("d") != -1) ? SharedAccessBlobPermissions.Delete : 0;
            Permissions |= (sp.IndexOf("a") != -1) ? SharedAccessBlobPermissions.Add : 0;
            Permissions |= (sp.IndexOf("c") != -1) ? SharedAccessBlobPermissions.Create : 0;
            Permissions |= (sp.IndexOf("l") != -1) ? SharedAccessBlobPermissions.List : 0;

            return Permissions;
        }


        /// <summary>
        /// Create a service SAS for a container - SharedAccessBlobPolicy
        /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-service-sas-create-dotnet#create-a-service-sas-for-a-blob-container
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <param name="containerName"></param>
        /// <param name="policyName"></param>
        /// <returns> blob.Uri + SAS Token</returns>
        public static string Get_ServiceSAS_Container(string accountName, string accountKey, string containerName, string policyName = null)
        {
            //--------------------------------------------------
            // To create the account SAS, you need to use Shared Key credentials. Modify for your account.
            string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            //container.CreateIfNotExists();
            //--------------------------------------------------

            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            SharedAccessBlobPolicy containerPolicy;
            try
            {
                containerPolicy = new SharedAccessBlobPolicy()
                {
                    Permissions = Set_PermissionsFromStr_ServiceSAS_Container(),
                    // SharedAccessStartTime = 
                    SharedAccessExpiryTime = SAS_Utils.SAS.seDateTime
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    containerPolicy.SharedAccessStartTime = SAS_Utils.SAS.stDateTime;
                //-----------------------------------------------------------
            }
            catch (Exception ex)
            { 
                MessageBox.Show("Error on generating the Container Sevice SAS\n" + ex, "Container Sevice SAS error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (String.IsNullOrEmpty(policyName))
            {
                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                return container.GetSharedAccessSignature(containerPolicy, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad hoc SAS and others on the stored access policy.
                return container.GetSharedAccessSignature(containerPolicy, policyName);
            }
        }




        /// <summary>
        /// Set Permissions From String, for Service SAS - Blobs
        ///      'rwdac'
        /// </summary>
        /// <returns></returns>
        private static SharedAccessBlobPermissions Set_PermissionsFromStr_ServiceSAS_Blob()
        {
            string sp = SAS_Utils.SAS.sp.v;
            SharedAccessBlobPermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? SharedAccessBlobPermissions.Read : 0;
            Permissions |= (sp.IndexOf("w") != -1) ? SharedAccessBlobPermissions.Write : 0;
            Permissions |= (sp.IndexOf("d") != -1) ? SharedAccessBlobPermissions.Delete : 0;
            Permissions |= (sp.IndexOf("a") != -1) ? SharedAccessBlobPermissions.Add : 0;
            Permissions |= (sp.IndexOf("c") != -1) ? SharedAccessBlobPermissions.Create : 0;

            return Permissions;
        }



        /// <summary>
        /// Create a service SAS for a blob - SharedAccessBlobPolicy
        /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-service-sas-create-dotnet#create-a-service-sas-for-a-blob
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="policyName"></param>
        /// <returns> blob.Uri + SAS Token</returns>
        public static string Get_ServiceSAS_Blob(string accountName, string accountKey, string containerName, string blobName, string policyName = null)
        {
            //--------------------------------------------------
            // To create the account SAS, you need to use Shared Key credentials. Modify for your account.
            string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            //container.CreateIfNotExists();

            //Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            //--------------------------------------------------

            // Create a new access policy and define its constraints.
            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            SharedAccessBlobPolicy blobPolicy;
            try{
                blobPolicy = new SharedAccessBlobPolicy()
                {
                    Permissions = Set_PermissionsFromStr_ServiceSAS_Blob(),
                    // SharedAccessStartTime =
                    SharedAccessExpiryTime = SAS_Utils.SAS.seDateTime,
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    blobPolicy.SharedAccessStartTime = SAS_Utils.SAS.stDateTime;
                //-----------------------------------------------------------
            }
            catch (Exception ex)
            { 
                MessageBox.Show("Error on generating the Blob Service SAS\n" + ex, "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }

            if (String.IsNullOrEmpty(policyName))
            {
                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                return blob.GetSharedAccessSignature(blobPolicy);
            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                return blob.GetSharedAccessSignature(blobPolicy, policyName);
            }
        }




        /// <summary>
        /// Set Permissions From String, for Service SAS - Files
        ///      'rwdc'
        /// </summary>
        /// <returns></returns>
        private static SharedAccessFilePermissions Set_PermissionsFromStr_ServiceSAS_Files()
        {
            string sp = SAS_Utils.SAS.sp.v;
            SharedAccessFilePermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? SharedAccessFilePermissions.Read : 0;
            Permissions |= (sp.IndexOf("w") != -1) ? SharedAccessFilePermissions.Write : 0;
            Permissions |= (sp.IndexOf("d") != -1) ? SharedAccessFilePermissions.Delete : 0;
            Permissions |= (sp.IndexOf("c") != -1) ? SharedAccessFilePermissions.Create : 0;

            return Permissions;
        }



        /// <summary>
        /// Create a service SAS for a File - SharedAccessFilePolicy
        /// Similar: https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-service-sas-create-dotnet#create-a-service-sas-for-a-blob
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <param name="shareName"></param>
        /// <param name="fileName"></param>
        /// <param name="policyName"></param>
        /// <returns> share.Uri + SAS Token</returns>
        public static string Get_ServiceSAS_File(string accountName, string accountKey, string shareName, string fileName, string policyName = null)
        {
            //--------------------------------------------------
            // To create the account SAS, you need to use Shared Key credentials. Modify for your account.
            string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            //Create the File client object.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            //Get a reference to a share to use for the sample code, and create it if it does not exist.
            CloudFileShare share = fileClient.GetShareReference(shareName);
            //share.CreateIfNotExists();

            //Get a reference to a root directory within the share.
            CloudFileDirectory rootDirectory = share.GetRootDirectoryReference();

            //Get a reference to a file within the directory.
            CloudFile file = rootDirectory.GetFileReference(fileName);
            //--------------------------------------------------

            // Create a new access policy and define its constraints.
            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            SharedAccessFilePolicy filePolicy;
            try{
                filePolicy = new SharedAccessFilePolicy()
                {
                    Permissions = Set_PermissionsFromStr_ServiceSAS_Files(),
                    // SharedAccessStartTime = 
                    SharedAccessExpiryTime = SAS_Utils.SAS.seDateTime
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    filePolicy.SharedAccessStartTime = SAS_Utils.SAS.stDateTime;
                //-----------------------------------------------------------
            }
            catch (Exception ex)
            { 
                MessageBox.Show("Error on generating the File Service SAS\n" + ex, "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }


            if (String.IsNullOrEmpty(policyName))
            {
                // Generate the shared access signature on the file, setting the constraints directly on the signature.
                return file.GetSharedAccessSignature(filePolicy);
            }
            else
            {
                // Generate the shared access signature on the file. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                return file.GetSharedAccessSignature(filePolicy, policyName);
            }
        }



        /// <summary>
        /// Set Permissions From String, for Service SAS - Shares
        ///      'rwdcl'
        /// </summary>
        /// <returns></returns>
        private static SharedAccessFilePermissions Set_PermissionsFromStr_ServiceSAS_Shares()
        {
            string sp = SAS_Utils.SAS.sp.v;
            SharedAccessFilePermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? SharedAccessFilePermissions.Read : 0;
            Permissions |= (sp.IndexOf("w") != -1) ? SharedAccessFilePermissions.Write : 0;
            Permissions |= (sp.IndexOf("d") != -1) ? SharedAccessFilePermissions.Delete : 0;
            Permissions |= (sp.IndexOf("c") != -1) ? SharedAccessFilePermissions.Create : 0;
            Permissions |= (sp.IndexOf("l") != -1) ? SharedAccessFilePermissions.List : 0;

            return Permissions;
        }


        /// <summary>
        /// Create a service SAS for a Shares - SharedAccessFilePolicy
        /// Similar: https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-service-sas-create-dotnet#create-a-service-sas-for-a-blob
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <param name="shareName"></param>
        /// <param name="policyName"></param>
        /// <returns> share.Uri + SAS Token</returns>
        public static string Get_ServiceSAS_Share(string accountName, string accountKey, string shareName, string policyName = null)
        {
            //--------------------------------------------------
            // To create the account SAS, you need to use Shared Key credentials. Modify for your account.
            string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            //Create the File client object.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            //Get a reference to a share to use for the sample code, and create it if it does not exist.
            CloudFileShare share = fileClient.GetShareReference(shareName);
            //share.CreateIfNotExists();

            //--------------------------------------------------

            // Create a new access policy and define its constraints.
            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            SharedAccessFilePolicy sharePolicy;
            try
            {
                sharePolicy = new SharedAccessFilePolicy()
                {
                    Permissions = Set_PermissionsFromStr_ServiceSAS_Shares(),
                    // SharedAccessStartTime = 
                    SharedAccessExpiryTime = SAS_Utils.SAS.seDateTime
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    sharePolicy.SharedAccessStartTime = SAS_Utils.SAS.stDateTime;
                //-----------------------------------------------------------
            }
            catch (Exception ex)
            { 
                MessageBox.Show("Error on generating the Share Service SAS\n" + ex, "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }

            if (String.IsNullOrEmpty(policyName))
            {
                // Generate the shared access signature on the Share, setting the constraints directly on the signature.
                return share.GetSharedAccessSignature(sharePolicy);
            }
            else
            {
                // Generate the shared access signature on the Share. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                return share.GetSharedAccessSignature(sharePolicy, policyName);
            }
        }




        /// <summary>
        /// Set Permissions From String, for Service SAS - Queues
        ///      'arup'
        /// </summary>
        /// <returns></returns>
        private static SharedAccessQueuePermissions Set_PermissionsFromStr_ServiceSAS_Queues()
        {
            string sp = SAS_Utils.SAS.sp.v;
            SharedAccessQueuePermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? SharedAccessQueuePermissions.Read : 0;
            Permissions |= (sp.IndexOf("a") != -1) ? SharedAccessQueuePermissions.Add : 0;
            Permissions |= (sp.IndexOf("u") != -1) ? SharedAccessQueuePermissions.Update : 0;
            Permissions |= (sp.IndexOf("p") != -1) ? SharedAccessQueuePermissions.ProcessMessages : 0;

            return Permissions;
        }


        /// <summary>
        /// Create a service SAS for a Queue - SharedAccessQueuePolicy
        /// Similar: https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-service-sas-create-dotnet#create-a-service-sas-for-a-blob
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <param name="queueName"></param>
        /// <param name="policyName"></param>
        /// <returns> queue.Uri + SAS Token</returns>
        public static string Get_ServiceSAS_Queue(string accountName, string accountKey, string queueName, string policyName = null)
        {
            //--------------------------------------------------
            // To create the account SAS, you need to use Shared Key credentials. Modify for your account.
            string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            //Create the queue client object.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            //Get a reference to a queue to use for the sample code, and create it if it does not exist.
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            //queue.CreateIfNotExists();
            //--------------------------------------------------

            // Create a new access policy and define its constraints.
            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            SharedAccessQueuePolicy queuePolicy;
            try{
                queuePolicy = new SharedAccessQueuePolicy()
                {
                    Permissions = Set_PermissionsFromStr_ServiceSAS_Queues(),
                    //SharedAccessStartTime = 
                    SharedAccessExpiryTime = SAS_Utils.SAS.seDateTime
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    queuePolicy.SharedAccessStartTime = SAS_Utils.SAS.stDateTime;
                //-----------------------------------------------------------
            }
            catch (Exception ex)
            { 
                MessageBox.Show("Error on generating the Queue Service SAS\n" + ex, "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }

            if (String.IsNullOrEmpty(policyName))
            {
                // Generate the shared access signature on the Queue, setting the constraints directly on the signature.
                return queue.GetSharedAccessSignature(queuePolicy);
            }
            else
            {
                // Generate the shared access signature on the Queue. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                return queue.GetSharedAccessSignature(queuePolicy, policyName);
            }
        }





        /// <summary>
        /// Set Permissions From String, for Service SAS - Tables
        /// ---> Using the 'Microsoft.Azure.Cosmos.Table' library <---
        ///      "raud"
        /// </summary>
        /// <returns></returns>
        private static Microsoft.Azure.Cosmos.Table.SharedAccessTablePermissions Set_PermissionsFromStr_ServiceSAS_Tables()
        {
            string sp = SAS_Utils.SAS.sp.v;
            Microsoft.Azure.Cosmos.Table.SharedAccessTablePermissions Permissions = 0;
            Permissions |= (sp.IndexOf("r") != -1) ? Microsoft.Azure.Cosmos.Table.SharedAccessTablePermissions.Query : 0;
            Permissions |= (sp.IndexOf("a") != -1) ? Microsoft.Azure.Cosmos.Table.SharedAccessTablePermissions.Add : 0;
            Permissions |= (sp.IndexOf("u") != -1) ? Microsoft.Azure.Cosmos.Table.SharedAccessTablePermissions.Update : 0;
            Permissions |= (sp.IndexOf("d") != -1) ? Microsoft.Azure.Cosmos.Table.SharedAccessTablePermissions.Delete : 0;

            return Permissions;
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
            Microsoft.Azure.Cosmos.Table.StorageCredentials storageCredentials = new Microsoft.Azure.Cosmos.Table.StorageCredentials(accountName, accountKey);
            Microsoft.Azure.Cosmos.Table.StorageUri uri = new Microsoft.Azure.Cosmos.Table.StorageUri(new Uri(tableUri));
            Microsoft.Azure.Cosmos.Table.CloudTableClient tableClient = new Microsoft.Azure.Cosmos.Table.CloudTableClient(uri, storageCredentials);
            Microsoft.Azure.Cosmos.Table.CloudTable table = tableClient.GetTableReference(tableName);
            //--------------------------------------------------

            // Create a new access policy and define its constraints, using the 'Microsoft.Azure.Cosmos.Table' library.
            // Note that the SharedAccessTablePolicy class is used both to define the parameters of an ad hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            Microsoft.Azure.Cosmos.Table.SharedAccessTablePolicy tablePolicy;
            try
            {
                tablePolicy = new Microsoft.Azure.Cosmos.Table.SharedAccessTablePolicy()
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



    }
}


//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------