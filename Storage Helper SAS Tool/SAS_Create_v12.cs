using System;
using System.Windows;
using System.Windows.Controls;

// Storage SDK v12.x.x
using Azure.Storage;
using Azure.Storage.Sas;
using Azure.Storage.Blobs;
using Azure.Storage.Files;
using Azure.Storage.Queues;


namespace Storage_Helper_SAS_Tool
{
    class SAS_Create_v12
    {
        /// <summary>
        /// SAS builder using the new version 12.0.0 (preview) 
        /// Azure.Storage
        /// Azure.Storage.Sas
        /// ------------------------------------------------------------------------------------------
        /// Account SAS builder
        /// https://docs.microsoft.com/cs-cz/dotnet/api/azure.storage.sas.accountsasbuilder?view=azure-dotnet-preview
        /// 
        /// Blob SAS Builder
        /// https://docs.microsoft.com/cs-cz/dotnet/api/azure.storage.sas.blobsasbuilder?view=azure-dotnet-preview
        /// 
        /// File SAS Builder
        /// https://docs.microsoft.com/en-us/dotnet/api/azure.storage.sas.filesasbuilder?view=azure-dotnet-preview
        /// 
        /// Queue SAS Builder
        /// https://docs.microsoft.com/en-us/dotnet/api/azure.storage.sas.queuesasbuilder?view=azure-dotnet-preview
        /// 
        /// Table not supported
        /// 
        /// Create SAS token
        /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-user-delegation-sas-create-dotnet#create-the-sas-token
        /// ------------------------------------------------------------------------------------------
        /// </summary>
        /// 


        /// <summary>
        /// Return info about SDK v11 limitations, on generating SAS
        /// </summary>
        /// <returns></returns>
        public static string Limitations_v12_Info(string StorageSDK_12_Version, ComboBox ComboBox_sr)
        {
            string s1 = "-------------------------------------------------\n";

            string s3 = "\n";
            s3 += "Notes:\n";

            // Regenerated Account SAS (srt) 
            if (SAS_Utils.SAS.srt.v != "not found" && SAS_Utils.SAS.srt.v != "")
            {
                s3 += "Regenerated using Storage SDK " + StorageSDK_12_Version + "\n";
                s3 += " - Parameter 'Api Version' not defined on this SDK - uses the same as 'Service Version'\n";
                s3 += "\n\n";
                s3 += "Tips:";
                s3 += " - On Azure Storage Explorer, Account SAS need all resources sco, and at least rwl permissions.";
            }

            // Regenerated Service SAS (sr) or (tn)
            if (SAS_Utils.SAS.sr.v != "not found" && SAS_Utils.SAS.sr.v != "")
            {
                s3 += "Regenerated using Storage SDK " + StorageSDK_12_Version + "\n";
                s3 += " - Parameter 'Api Version' not defined on this SDK - uses the same as 'Service Version'\n";
                s3 += "\n\n";
                s3 += "Tips:";
                switch (ComboBox_sr.Text)
                {
                    case "c":
                        s3 += " - On Azure Storage Explorer, Container Service SAS need at least l permissions.";
                        break;
                    case "b":
                        s3 += " - On Azure Storage Explorer, Blob Service SAS is not supported.";
                        s3 += " - On Browser, Blob Service SAS need at least rd permissions.";
                        break;
                    case "s":
                        s3 += " - On Azure Storage Explorer, Share Service SAS need at least rwl permissions.";
                        break;
                    case "f":
                        s3 += " - On Azure Storage Explorer, File Service SAS is not supported.";
                        s3 += " - On Azure Storage Explorer, File Service SAS need all the rwdc permissions.";
                        break;
                    case "q":
                        s3 += " - On Azure Storage Explorer, Queue Service SAS failed with all the permissions.";
                        s3 += " - Queue Service SAS is not well documented.";
                        break;
                    case "bs":

                        break;
                }
            }

            return s1 + s3;
        }


        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Account SAS methods
        //---------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns SasProtocol value based on the string on paramenter.
        /// if not https / http,https / https,http, returns None
        /// </summary>
        /// <returns></returns>
        private static SasProtocol Get_SasProtocol(string protocol)
        {
            // both protocols provided on spr 
            if (protocol.IndexOf("http,https") != -1 || protocol.IndexOf("https,http") != -1)
                return SasProtocol.HttpsAndHttp;

            // https protocol provided on spr
            if (protocol == "https")
                return SasProtocol.Https;

            return SasProtocol.None;
        }



        public static bool Regenerate_AccountSAS(TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox BoxAuthResults, string ss)
        {
            string sas = "?" + AccountSasBuilder(textBoxAccountName.Text, textBoxAccountKey1.Text);
            if (sas == "?") return false;

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

            return true;
        }



        /// <summary>
        /// AccountSasBuilder Struct
        /// https://docs.microsoft.com/cs-cz/dotnet/api/azure.storage.sas.accountsasbuilder?view=azure-dotnet-preview
        /// 
        /// </summary>
        /// <returns></returns>
        public static string AccountSasBuilder(string accountName, string accountKey)
        {
            // AccountSasBuilder is used to generate an account level Shared Access Signature(SAS) for Azure Storage services.
            // mandatory fields
            AccountSasBuilder accountSasBuilder;
            try
            {
                accountSasBuilder = new AccountSasBuilder()
                {
                    //IPRange = 
                    Permissions = SAS_Utils.SAS.sp.v,        // string - Add, Create, Delete, List, Process, Read, Update, Write (account)
                    //Protocol = 
                    ResourceTypes = SAS_Utils.SAS.srt.v,     // string - Container, Object, Service 
                    Services = SAS_Utils.SAS.ss.v,           // String - Blobs, Files, Queues
                    //StartTime =                                               - DateTimeOffset.UtcNow                 // OK
                    ExpiryTime = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    accountSasBuilder.StartTime = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    accountSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    accountSasBuilder.IPRange = new IPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0]==0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error on creating Account SAS\n\n" + ex.ToString(), "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }
            //-----------------------------------------------------------
            
            StorageSharedKeyCredential Credential = new StorageSharedKeyCredential(accountName, accountKey);
          
            // Use the key to get the SAS token.
            string sasToken = accountSasBuilder.ToSasQueryParameters(Credential).ToString();
            return sasToken;
        }





        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Blob Service SAS methods
        //---------------------------------------------------------------------------------------------------------------------

        public static bool Regenerate_ServiceSAS_Container(Label labelContainerName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxContainerName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelContainerName, textBoxContainerName.Text, "Missing Container Name", "Error")) { SAS_Utils.SAS.storageAccountName.s = false; return false; }

            string sas = "?" + BlobSasBuilder(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxContainerName.Text, "", "", textBoxPolicyName.Text);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Container:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Container (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";

            BoxAuthResults.Text += "Container URI:\n" + "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }



        public static bool Regenerate_ServiceSAS_Blob(Label labelContainerName, Label labelBlobName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxContainerName, TextBox textBoxBlobName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelContainerName, textBoxContainerName.Text, "Missing Container Name", "Error")) { SAS_Utils.SAS.containerName.s = false; return false; }
            if (Utils.StringEmpty(labelBlobName, textBoxBlobName.Text, "Missing Blob Name", "Error")) { SAS_Utils.SAS.blobName.s = false; return false; }

            string sas = "?" + BlobSasBuilder(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxContainerName.Text, textBoxBlobName.Text, "", textBoxPolicyName.Text);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Blob:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Blob (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";

            BoxAuthResults.Text += "Blob URI:\n" + "https://" + textBoxAccountName.Text + ".blob.core.windows.net/"+ textBoxContainerName.Text + "/"+ textBoxBlobName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }



        public static bool Regenerate_ServiceSAS_BlobSnapshot(Label labelContainerName, Label labelBlobName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxContainerName, TextBox textBoxBlobSnapsotName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelContainerName, textBoxContainerName.Text, "Missing Container Name", "Error")) { SAS_Utils.SAS.containerName.s = false; return false; }
            if (Utils.StringEmpty(labelBlobName, textBoxBlobSnapsotName.Text, "Missing Blob Snapshot Name", "Error")) { SAS_Utils.SAS.blobSnapshotName.s = false; return false; }

            string sas = "?" + BlobSasBuilder(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxContainerName.Text, textBoxBlobSnapsotName.Text, "", textBoxPolicyName.Text);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Blob Snapshot:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Blob Snapshot (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";

            BoxAuthResults.Text += "Blob Snapshot URI:\n" + "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobSnapsotName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }


        /// <summary>
        /// BlobSasBuilder is used to generate a Shared Access Signature (SAS) for an Azure Storage container or blob
        /// https://docs.microsoft.com/cs-cz/dotnet/api/azure.storage.sas.blobsasbuilder?view=azure-dotnet-preview
        /// 
        /// </summary>
        /// <returns></returns>
        public static string BlobSasBuilder(string accountName, string accountKey, string containerName, string blobName = "", string SnapshotName = "", string PolicyName = "")
        {
            // TODO - ExpiryTime must be omitted if Identifier is specified.
            // TODO - Header fields
            // BlobSasBuilder is used to generate a Shared Access Signature (SAS) for an Azure Storage container or blob
            BlobSasBuilder blobSasBuilder;
            try
            {
                blobSasBuilder = new BlobSasBuilder()
                {
                    // CacheControl
                    // ContentDisposition
                    // ContentEncoding
                    // ContentLanguage
                    // ContentType
                    ContainerName = containerName,  // string
                    // BlobName = 
                    // Snapshot =                 // String - Name of Snapshot (if Resource = "bs")
                    //Identifier =                // string - access policy specified for the container
                    //IPRange =                   // IPRange - StartIP, optional EndIP
                    //Permissions = "drw",        // string - Delete, Read, Write (Blob Snapshot)       
                    //Permissions = "acdrw",      // string - Delete, Read, Write, Add, Create (Blob)
                    //Permissions = "acdlrw",     // string - Delete, Read, List, Write, Add, Create (container)
                    Permissions = SAS_Utils.SAS.sp.v,        // Permissions validated on ComboBox_sr_DropDownClosed()
                    // Protocol =                            // SasProtocol - Https, HttpsAndHttp, None
                    Resource = SAS_Utils.SAS.sr.v,           // string - Blob, Container, BlobSnapshot (sv 2018-11-09 and later)
                    //StartTime =                                               - DateTimeOffset.UtcNow                 // OK
                    ExpiryTime = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if(SAS_Utils.SAS.sr.v == "b")
                    blobSasBuilder.BlobName = blobName;

                if (SAS_Utils.SAS.sr.v == "bs")     // BlobSnapshot (sv 2018-11-09 and later)
                    if (String.Compare(SAS_Utils.SAS.sv.v, "2018-11-09") >= 0)
                        blobSasBuilder.Snapshot = SnapshotName;
                    else
                    {
                        MessageBox.Show("BlobSnapshot is only supported on Service Version 2018-11-09 and later.", "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return "";
                    }

                // ExpiryTime must be omitted if it has been specified in an associated stored access policy.
                if (PolicyName != "")
                    blobSasBuilder.Identifier = PolicyName;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    blobSasBuilder.StartTime = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    blobSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    blobSasBuilder.IPRange = new IPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0] == 0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error on creating " + (blobName != "" ? "Blob" : (SnapshotName != "" ? "BlobSnapshot" : "Container")) + " Service SAS\n\n" + ex.ToString(), "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }
            //-----------------------------------------------------------

            StorageSharedKeyCredential Credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Use the key to get the SAS token.
            string sasToken = blobSasBuilder.ToSasQueryParameters(Credential).ToString();
            return sasToken;
        }




        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- File Service SAS methods
        //---------------------------------------------------------------------------------------------------------------------

        public static bool Regenerate_ServiceSAS_Share(Label labelShareName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxShareName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelShareName, textBoxShareName.Text, "Missing Share Name", "Error")) { SAS_Utils.SAS.shareName.s = false; return false; }

            string sas = "?" + FileSasBuilder(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxShareName.Text, "", textBoxPolicyName.Text);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Share:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Share (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";

            BoxAuthResults.Text += "Share URI:\n" + "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }



        public static bool Regenerate_ServiceSAS_File(Label labelShareName, Label labelFileName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxShareName, TextBox textBoxFileName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelShareName, textBoxShareName.Text, "Missing Share Name", "Error")) { SAS_Utils.SAS.shareName.s = false; return false; }
            if (Utils.StringEmpty(labelFileName, textBoxFileName.Text, "Missing File Name", "Error")) { SAS_Utils.SAS.fileName.s = false; return false; }

            string sas = "?" + FileSasBuilder(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxShareName.Text, textBoxFileName.Text, textBoxPolicyName.Text);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Service SAS - File:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - File (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";

            BoxAuthResults.Text += "File URI:\n" + "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "/" + textBoxFileName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }



        /// <summary>
        /// File SAS Builder
        /// https://docs.microsoft.com/en-us/dotnet/api/azure.storage.sas.filesasbuilder?view=azure-dotnet-preview
        /// 
        /// </summary>
        /// <returns></returns>
        public static string FileSasBuilder(string accountName, string accountKey, string shareName, string filePath = "", string PolicyName = "")
        {
            // TODO - ExpiryTime must be omitted if Identifier is specified.
            // TODO - Header fields
            // Create a SAS token that's valid a short interval.
            FileSasBuilder fileSasBuilder;
            try
            {
                fileSasBuilder = new FileSasBuilder()
                {
                    // CacheControl
                    // ContentDisposition
                    // ContentEncoding
                    // ContentLanguage
                    // ContentType
                    // FilePath =                 // string - The path of the file or directory being made accessible, or Empty for a share SAS.
                    ShareName = shareName,        // string - The name of the share being made accessible.
                    // Identifier =               // string - access policy specified for the container
                    // IPRange = 
                    //Permissions = "cdrw",       // string - Create, Delete, Read, Write (File SAS)
                    //Permissions = "cdrw",       // string - Create, Delete, List, Read, Write (Share SAS)
                    Permissions = SAS_Utils.SAS.sp.v,        // Permissions validated on ComboBox_sr_DropDownClosed()
                    // Protocol =                            // SasProtocol - Https, HttpsAndHttp, None
                    // StartTime =                                              - DateTimeOffset.UtcNow                 // OK
                    ExpiryTime = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (filePath != "")
                    fileSasBuilder.FilePath = filePath;

                // ExpiryTime must be omitted if it has been specified in an associated stored access policy.
                if (PolicyName != "")
                    fileSasBuilder.Identifier = PolicyName;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    fileSasBuilder.StartTime = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    fileSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    fileSasBuilder.IPRange = new IPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0] == 0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on creating "+(filePath == ""?"Share":"File") +" Service SAS\n\n" + ex.ToString(), "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }
            //-----------------------------------------------------------



            StorageSharedKeyCredential Credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Use the key to get the SAS token.
            string sasToken = fileSasBuilder.ToSasQueryParameters(Credential).ToString();
            return sasToken;
        }




        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Queue Service SAS methods
        //---------------------------------------------------------------------------------------------------------------------

        public static bool Regenerate_ServiceSAS_Queue(Label labelQueueName, TextBox textBoxAccountName, TextBox textBoxAccountKey1, TextBox textBoxQueueName, TextBox textBoxPolicyName, TextBox BoxAuthResults)
        {
            if (Utils.StringEmpty(labelQueueName, textBoxQueueName.Text, "Missing Queue Name", "Error")) { SAS_Utils.SAS.queueName.s = false; return false; }

            string sas = "?" + QueueSasBuilder(textBoxAccountName.Text, textBoxAccountKey1.Text, textBoxQueueName.Text, textBoxPolicyName.Text);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Queue:\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "Regenerated Service SAS URI - Queue (Unescaped):\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n\n\n";

            BoxAuthResults.Text += "Queue URI:\n" + "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + textBoxQueueName.Text + Uri.UnescapeDataString(sas) + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }
           


        /// <summary>
        /// Queue SAS Builder
        /// https://docs.microsoft.com/en-us/dotnet/api/azure.storage.sas.queuesasbuilder?view=azure-dotnet-preview
        /// 
        /// </summary>
        /// <returns></returns>
        public static string QueueSasBuilder(string accountName, string accountKey, string queueName, string PolicyName = "")
        {
            // TODO - ExpiryTime must be omitted if Identifier is specified.
            // Create a SAS token that's valid a short interval.
            QueueSasBuilder queueSasBuilder;
            try{
                queueSasBuilder = new QueueSasBuilder()
                {
                    QueueName = queueName,              // string - The name of the queue
                    // Identifier =             // string - access policy specified for the container
                    // IPRange = 
                    //Permissions = "adlpruw",          // string - Add, Delete, List, Process, Read, Update, Write (Queue Account SAS)
                    //Permissions = "apru",             // string - Add, Process, Read, Update (Queue SAS)
                    Permissions = SAS_Utils.SAS.sp.v,
                    // Protocol =                        // SasProtocol - Https, HttpsAndHttp, None, Value
                    // StartTime =                                              - DateTimeOffset.UtcNow                 // OK
                    // QueueName = 
                    ExpiryTime = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                // ExpiryTime must be omitted if it has been specified in an associated stored access policy.
                if (PolicyName != "")
                    queueSasBuilder.Identifier = PolicyName;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    queueSasBuilder.StartTime = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    queueSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    queueSasBuilder.IPRange = new IPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0] == 0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.queueName.v))
                    queueSasBuilder.QueueName = SAS_Utils.SAS.queueName.v;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on creating Queue Service SAS\n\n" + ex.ToString(), "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }
            //-----------------------------------------------------------


            StorageSharedKeyCredential Credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Use the key to get the SAS token.
            string sasToken = queueSasBuilder.ToSasQueryParameters(Credential).ToString();
            return sasToken;
        }





        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Table Service SAS methods - TODO
        //---------------------------------------------------------------------------------------------------------------------

        // TableSasBuilder - not supported by Storage SDK v12.0.0.0_preview

    }
}
