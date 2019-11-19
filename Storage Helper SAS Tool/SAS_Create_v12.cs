using System;
using System.Windows;
using System.Windows.Controls;

// Storage SDK v12.0.0
// Azure.Storge.Files was renamed Azure.Storage.Files.Shares, and is still on v12.0.0 Preview 5
using Azure.Storage;
using Azure.Storage.Sas;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
//using Azure.Storage.Files;
using Azure.Storage.Files.Shares;


namespace Storage_Helper_SAS_Tool
{
    class SAS_Create_v12
    {
        /// <summary>
        /// Azure Blob storage client library v12 for .NET is now GA available.
        /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet

        /// ChangeLog:
        /// https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/storage/Azure.Storage.Blobs/Changelog.txt

        /// GitHub source - v12 for .NET GA:
        /// https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/storage/Azure.Storage.Blobs
        /// ------------------------------------------------------------------------------------------
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
        public static string Limitations_v12_Info(string StorageSDK_12_Version)
        {
            string s = "-------------------------------------------------\n";
            s += "Notes:\n";
            s += " - Regenerated using Storage SDK " + StorageSDK_12_Version + "\n";
            s += " - Parameter 'Api Version' not defined on this SDK - uses the same as 'Service Version'\n";
            s += "\n";
            s += "Tips:\n";

            // Regenerated Account SAS (srt) 
            if (SAS_Utils.SAS.srt.v != "not found" && SAS_Utils.SAS.srt.v != "")
            {
                //s += " - Different order on 'sp' parameters can generate different singatures 'sig' for Account SAS, but any regenerated SAS should be valid.\n"; // Generated with Powershell only
                s += " - On Azure Storage Explorer, Account SAS need all resources sco, and at least rl permissions.\n";
                s += " - On Browser, replace <container> and <blob> on the URL, by some existing container and blob on the storage account.\n";
                s += " - On Browser, Account SAS need at least r permissions to read blob content.\n";
                s += "\n";
            }

            // Regenerated Service SAS (sr) or (tn)
            if (SAS_Utils.SAS.sr.v != "not found" && SAS_Utils.SAS.sr.v != "")
            {
                switch (SAS_Utils.SAS.sr.v)
                {
                    case "c":
                        s += " - On Azure Storage Explorer, only URLencoded using Service Version 'sv' 2018-11-09 or above are valid regenerated Container Service SAS.\n";
                        s += " - On Azure Storage Explorer, Container Service SAS need at least l permissions.\n";
                        s += " - On Browser, only URLencoded using Service Version 'sv' 2019-02-02 or above are valid regenerated Container Service SAS.\n";
                        break;
                    case "b":
                        s += " - On Azure Storage Explorer, Blob Service SAS is not supported (single blob).\n";
                        s += " - On Browser, Blob Service SAS need at least r permissions.\n";
                        break;
                    case "s":
                        s += " - On Azure Storage Explorer, Share Service SAS need at least l permissions.\n";
                        s += " - On Browser, Share Service SAS need at least l permissions.\n";
                        break;
                    case "f":
                        s += " - On Azure Storage Explorer, File Service SAS is not supported (single file).\n";
                        s += " - On Browser, File Service SAS need at least r permissions.\n";
                        break;
                    case "bs":
                        s += " - On Azure Storage Explorer, Blob Snapshot Service SAS is not supported (single blob).\n";
                        s += " - On Browser, Blob Snapshot Service SAS need at least rd permissions.\n";
                        s += " - On Browser, replace <DateTime> on URL by date time of the Blob Snapshot (From 'URL' property in the 'View SnapShots' blade, on Azure Portal.\n";
                        break;
                }
            }

            // All empty means Queue Service SAS
            if ((SAS_Utils.SAS.srt.v == "not found" || SAS_Utils.SAS.srt.v == "") &&
                (SAS_Utils.SAS.sr.v == "not found" || SAS_Utils.SAS.sr.v == "") &&
                SAS_Utils.SAS.tn.v == "")
                // s += " - On Azure Storage Explorer, Queue Service SAS failed with all the permissions.\n";
                s += "\n";

            return s;
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
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Account SAS key is not supporting Table Service anymore on this version 12.0.0\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid sig\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Account SAS token:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            if (ss.IndexOf("b") != -1)
            {
                BoxAuthResults.Text += "Blob URI:\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + sas + "\n\n";
            }

            if (ss.IndexOf("f") != -1)
            {
                BoxAuthResults.Text += "File URI:\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + sas + "\n\n";
            }

            if (ss.IndexOf("q") != -1)
            {
                BoxAuthResults.Text += "Queue URI:\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + sas + "\n\n";
            }

            if (ss.IndexOf("t") != -1)
            {
                BoxAuthResults.Text += "Table URI:\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".table.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".table.core.windows.net/" + sas + "\n\n";
            }

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get blob) (replace <container> and <blob>):\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/<container>/<blob>" + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/<container>/<blob>" + sas + "\n\n";


            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }



        /// <summary>
        /// Return AccountSasServices from srt string
        /// Changed on v12 to acomodate AccountSasResourceTypes - All, Container, Object, Service
        /// https://docs.microsoft.com/en-us/dotnet/api/azure.storage.sas.accountsasresourcetypes?view=azure-dotnet
        /// </summary>
        /// <param name="srt"></param>
        /// <returns></returns>
        private static AccountSasResourceTypes Get_AccountSasResourceTypes_FromStr(string srt)
        {
            if ((srt.IndexOf("s") != -1) && (srt.IndexOf("o") != -1) && (srt.IndexOf("c") != -1)) // All
                return AccountSasResourceTypes.All;

            AccountSasResourceTypes result = 0;

            if (srt.IndexOf("s") != -1)
                result = AccountSasResourceTypes.Service;

            if (srt.IndexOf("o") != -1)
                result |= AccountSasResourceTypes.Object;

            if (srt.IndexOf("c") != -1)
                result |= AccountSasResourceTypes.Container;

            return result;
        }




        /// <summary>
        /// Return AccountSasServices from ss string
        /// Changed on v12 to acomodate AccountSasServices - All, Blobs, Files, Queues
        /// https://docs.microsoft.com/en-us/dotnet/api/azure.storage.sas.accountsasservices?view=azure-dotnet
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        private static AccountSasServices Get_AccountSasServices_FromStr(string ss)
        {
            if ((ss.IndexOf("b") != -1) && (ss.IndexOf("f") != -1) && (ss.IndexOf("q") != -1)) // All
                return AccountSasServices.All;

            AccountSasServices result = 0;

            if (ss.IndexOf("b") != -1)
                result = AccountSasServices.Blobs;

            if (ss.IndexOf("f") != -1)
                result |= AccountSasServices.Files;

            if (ss.IndexOf("q") != -1)
                result |= AccountSasServices.Queues;

            return result;
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
                    //Permissions = SAS_Utils.SAS.sp.v,        // Changed to .SetPermissions(string) method, on v12
                    //Protocol = 
                    ResourceTypes = Get_AccountSasResourceTypes_FromStr(SAS_Utils.SAS.srt.v), // Changed on v12 to acomodate AccountSasResourceTypes - All, Container, Object, Service 
                    Services = Get_AccountSasServices_FromStr(SAS_Utils.SAS.ss.v),            // Changed on v12 to acomodate AccountSasResourceTypes - All, Blobs, Files, Queues
                    //StartTime =                                               - DateTimeOffset.UtcNow                 // OK
                    ExpiresOn = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                accountSasBuilder.SetPermissions(SAS_Utils.SAS.sp.v);   // string - Add, Create, Delete, List, Process, Read, Update, Write (account)

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    accountSasBuilder.StartsOn = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    accountSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    accountSasBuilder.IPRange = new SasIPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0]==0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP
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
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid sig\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Container:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Container URI:\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (list blobs):\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "?restype=container&comp=list" + Uri.UnescapeDataString(sas).Replace("?", "&") + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "?restype=container&comp=list" + sas.Replace("?", "&") + "\n\n";

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
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid sig\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Blob:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Blob URI:\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/"+ textBoxContainerName.Text + "/"+ textBoxBlobName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobName.Text + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get blob):\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobName.Text + sas + "\n\n";

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
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid sig\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Blob Snapshot:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Blob Snapshot URI:\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobSnapsotName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobSnapsotName.Text + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get snapshot) (repalce <DateTime> by snapshot datetime):\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobSnapsotName.Text + "?snapshot=<DateTime>" + Uri.UnescapeDataString(sas).Replace("?","&") + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".blob.core.windows.net/" + textBoxContainerName.Text + "/" + textBoxBlobSnapsotName.Text + "?snapshot=<DateTime>" + sas.Replace("?", "&") + "\n\n";

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
                    BlobContainerName = containerName,  // string
                    // BlobName = 
                    // Snapshot =                 // String - Name of Snapshot (if Resource = "bs")
                    //Identifier =                // string - access policy specified for the container
                    //IPRange =                   // IPRange - StartIP, optional EndIP
                    //Permissions = "drw",        // string - Delete, Read, Write (Blob Snapshot)       
                    //Permissions = "acdrw",      // string - Delete, Read, Write, Add, Create (Blob)
                    //Permissions = "acdlrw",     // string - Delete, Read, List, Write, Add, Create (container)
                    //Permissions = SAS_Utils.SAS.sp.v,      // Changed to .SetPermissions(string) method, on v12 
                    // Protocol =                            // SasProtocol - Https, HttpsAndHttp, None
                    Resource = SAS_Utils.SAS.sr.v,           // string - Blob, Container, BlobSnapshot (sv 2018-11-09 and later)
                    //StartTime =                                               - DateTimeOffset.UtcNow                 // OK
                    //ExpiryTime = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    ExpiresOn = SAS_Utils.SAS.seDateTime,
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                blobSasBuilder.SetPermissions(SAS_Utils.SAS.sp.v);   // Permissions validated on ComboBox_sr_DropDownClosed()

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (SAS_Utils.SAS.sr.v == "b")
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
                { 
                    //blobSasBuilder.StartTime = SAS_Utils.SAS.stDateTime;
                    blobSasBuilder.StartsOn = SAS_Utils.SAS.stDateTime;
                }

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    blobSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    blobSasBuilder.IPRange = new SasIPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0] == 0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP
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
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid sig\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Share:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Share URI:\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (list files):\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "?restype=directory&comp=list" + Uri.UnescapeDataString(sas).Replace("?", "&") + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "?restype=directory&comp=list" + sas.Replace("?", "&") + "\n\n";

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
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid sig\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - File:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "File URI:\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "/" + textBoxFileName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "/" + textBoxFileName.Text + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get file):\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "/" + textBoxFileName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".file.core.windows.net/" + textBoxShareName.Text + "/" + textBoxFileName.Text + sas + "\n\n";

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sas, "sig=", "&"));

            return true;
        }



        /// <summary>
        /// File SAS Builder
        /// https://docs.microsoft.com/en-us/dotnet/api/azure.storage.sas.filesasbuilder?view=azure-dotnet-preview
        /// Azure.Storge.Files was renamed Azure.Storage.Files.Shares, and is still on v12.0.0 Preview 5
        ///             FileSasBuilder replaced by ShareSasBuilder
        ///             https://github.com/Azure/azure-sdk-for-net/issues/8477
        /// </summary>
        /// <returns></returns>
        public static string FileSasBuilder(string accountName, string accountKey, string shareName, string filePath = "", string PolicyName = "")
        {
            // TODO - ExpiryTime must be omitted if Identifier is specified.
            // TODO - Header fields
            // Create a SAS token that's valid a short interval.
            ShareSasBuilder shareSasBuilder;     // FileSasBuilder
            try
            {
                shareSasBuilder = new ShareSasBuilder()
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
                    //Permissions = SAS_Utils.SAS.sp.v,        // Changed to .SetPermissions(string) method, on v12.0.0 Preview 5 (Azure.Storage.Files.Shares)
                    // Protocol =                            // SasProtocol - Https, HttpsAndHttp, None
                    // StartTime =                                              - DateTimeOffset.UtcNow                 // OK
                    ExpiresOn = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                shareSasBuilder.SetPermissions(SAS_Utils.SAS.sp.v);   // Permissions validated on ComboBox_sr_DropDownClosed()

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                if (filePath != "")
                    shareSasBuilder.FilePath = filePath;

                // ExpiryTime must be omitted if it has been specified in an associated stored access policy.
                if (PolicyName != "")
                    shareSasBuilder.Identifier = PolicyName;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    shareSasBuilder.StartsOn = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    shareSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    shareSasBuilder.IPRange = new SasIPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0] == 0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on creating "+(filePath == ""?"Share":"File") +" Service SAS\n\n" + ex.ToString(), "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }
            //-----------------------------------------------------------


            StorageSharedKeyCredential Credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Use the key to get the SAS token.
            string sasToken = shareSasBuilder.ToSasQueryParameters(Credential).ToString();
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
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid sig\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Queue:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Queue URI:\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + textBoxQueueName.Text + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + textBoxQueueName.Text + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (list messages):\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + textBoxQueueName.Text + "/messages" + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + textBoxAccountName.Text + ".queue.core.windows.net/" + textBoxQueueName.Text + "/messages" + sas + "\n\n";

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
                    //Permissions = SAS_Utils.SAS.sp.v,
                    // Protocol =                        // SasProtocol - Https, HttpsAndHttp, None, Value
                    // StartTime =                                              - DateTimeOffset.UtcNow                 // OK
                    // QueueName = 
                    ExpiresOn = SAS_Utils.SAS.seDateTime,   // DateTimeOffset  - DateTimeOffset.UtcNow.AddMinutes(60)  // OK
                    Version = SAS_Utils.SAS.sv.v             // String - sv
                };

                // Adding the optional fields commented out above
                //-----------------------------------------------------------
                // ExpiryTime must be omitted if it has been specified in an associated stored access policy.
                if (PolicyName != "")
                    queueSasBuilder.Identifier = PolicyName;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    queueSasBuilder.StartsOn = SAS_Utils.SAS.stDateTime;

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.spr.v))
                    queueSasBuilder.Protocol = Get_SasProtocol(SAS_Utils.SAS.spr.v);     // SasProtocol - Https, HttpsAndHttp, None

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.sip.v))
                    queueSasBuilder.IPRange = new SasIPRange(new System.Net.IPAddress(SAS_Utils.fromIP), (SAS_Utils.toIP[0] == 0 ? null : new System.Net.IPAddress(SAS_Utils.toIP))); // IPRange - StartIP, optional EndIP

                if (!String.IsNullOrEmpty(SAS_Utils.SAS.queueName.v))
                    queueSasBuilder.QueueName = SAS_Utils.SAS.queueName.v;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on creating Queue Service SAS\n\n" + ex.ToString(), "Invalid SAS parameters", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return "";
            }
            //-----------------------------------------------------------

            queueSasBuilder.SetPermissions(SAS_Utils.SAS.sp.v);   // Permissions validated on ComboBox_sr_DropDownClosed()

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
