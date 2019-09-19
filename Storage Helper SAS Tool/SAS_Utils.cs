using System;
using System.Windows.Controls;
using System.Windows;



namespace Storage_Helper_SAS_Tool
{
    /// <summary>
    /// ---------------------------------------------------------------------------------------------------------
    /// Constructing the Account SAS URI
    /// https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-account-sas?redirectedfrom=MSDN#constructing-the-account-sas-uri
    ///   Delegate access to:
    ///     service-level operations that are not currently available with a service-specific SAS, such as the Get/Set Service Properties and Get Service Stats operations.
    ///     more than one service in a storage account at a time. For example, you can delegate access to resources in both the Blob and File services with an account SAS.
    ///     write and delete operations for containers, queues, tables, and file shares, which are not available with an object-specific SAS.
    ///   Specify an IP address or range of IP addresses from which to accept requests.
    ///   Specify the HTTP protocol from which to accept requests (either HTTPS or HTTP/HTTPS).
    /// ---------------------------------------------------------------------------------------------------------
    /// Create a service SAS
    /// https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-service-sas
    ///   Delegates access to a resource in just one of the storage services: the Blob, Queue, Table, or File service.
    ///   A SAS may also specify the supported IP address or address range from which requests can originate
    ///   the supported protocol with which a request can be made
    ///   an optional access policy identifier associated with the request.
    /// ---------------------------------------------------------------------------------------------------------
    /// </summary>

    class SAS_Utils
    {
        //----------------------------------------------------------------------------
        /// <summary>
        /// Valid Storage Service Versions for Account SAS >= 2015-04-05 (if using srt)
        /// </summary>
        private static string[] validSV_AccountSAS_ARR = { "2019-02-02", "2018-11-09", "2018-03-28", "2017-11-09", "2017-07-29", "2017-04-17", "2016-05-31", "2015-12-11", "2015-07-08", "2015-04-05" };

        /// <summary>
        /// Valid Storage Service Versions for Service SAS  >= 2012-02-12 (if using sr, tn, queue ??)
        /// </summary>
        private static string[] validSV_ServiceSas_ARR_addon = { "2015-02-21", "2014-02-14", "2013-08-15", "2012-02-12" };

        //----------------------------------------------------------------------------


        /// <summary>
        /// Struct to save one individual value from analized SAS, and the state valid/error found
        /// </summary>
        public struct StrParameter
        {
            public string v;    // Value
            public bool s;      // State
        }

        public static byte[] fromIP = { 0, 0, 0, 0 };       // Signed IP - used on v12.0.0.0_preview
        public static byte[] toIP   = { 0, 0, 0, 0 };

        /// <summary>
        /// Struct to save all individual values from analized SAS
        /// </summary>
        public struct SasParameters
        {
            public string sharedAccessSignature; // complete SAS without the endpoints

            public string blobEndpoint;
            public string fileEndpoint;
            public string tableEndpoint;
            public string queueEndpoint;

            public StrParameter storageAccountName;   // storage Account Name, if provided on any Endpoint

            public StrParameter containerName;    // Container name, if provided on blobEndpoint
            public StrParameter blobName;         // Blob name, if provided on blobEndpoint
            public StrParameter shareName;        // Share name, if provided on fileEndpoint
            public StrParameter fileName;         // File name, if provided on fileEndpoint
            public StrParameter queueName;        // Queue name, if provided on queueEndpoint
            public StrParameter tableName;        // Table name, if provided on tableEndpoint, and if is an account SAS

            public bool onlySASprovided;        // true if the endpoints not provided

            public StrParameter apiVersion;
            public StrParameter sv;     // Service Version
            public StrParameter ss;     // Signed Services 
            public StrParameter srt;    // Signed Resource Types (account SAS)
            public StrParameter sp;     // Signed Permission 
            public StrParameter se;     // Signed Expiry DateTime
            public StrParameter st;     // Signed Start DateTime
            public StrParameter sip;    // Signed IP 
            public StrParameter spr;    // Signed Protocol 
            public string sig;          // Encripted signature
            
            public StrParameter sr;     // 
            public StrParameter tn;     // Table Name, if is Service SAS
            public StrParameter blobSnapshotName;  // Blob Snapshot Name if is Service SAS and using v12.0.0_preview
            public string spk;          //   startpk;
            public string epk;          //   endpk;
            public string srk;          //   startrk;
            public string erk;          //   endrk;
            public string si;           // Policy Name

            public DateTime stDateTime; // used to test the valid Date format
            public DateTime seDateTime; 
        };
        public static SasParameters SAS;

        /// <summary>
        /// Validate and save SAS parameter values on SAS struct
        /// Show SAS parameters results on BoxAuthResults
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SAS_Validate(string InputBoxSASvalue, TextBox BoxAuthResults)
        {
            // initialization of SAS Struct and cleas the "BoxAuthResults"
            BoxAuthResults.Text = "";
            init_SASstruct();

            // Validate and save SAS parameter values on SAS struct
            if (!SAS_GetParameters(InputBoxSASvalue)) return;

            // Some of endpoints (Blob, File, Table, Queue) can be "not found"
            Check_SASServiceEndpoints();

            // Show SAS parameters results on BoxAuthResults
            SAS_ShowResults(BoxAuthResults);
        }




        /// <summary>
        /// Test parameters on provided Account/Service SAS
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-an-Account-SAS?redirectedfrom=MSDN#constructing-the-account-sas-uri
        /// </summary>
        /// <param name="InputBoxSAS"></param>
        private bool SAS_GetParameters(string InputBoxSASvalue)
        {

            //---------------------------------------------------------------------
            // Check for some spaces or newlines on the string
            //---------------------------------------------------------------------
            string space = SAS_Utils.Get_SASSpace(InputBoxSASvalue.Trim());
            if (space != "")
            {
                MessageBox.Show("Invalid string - " + space + " found on SAS", "Invalid SAS", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;     // invalid string with spaces or new lines in the middle
            }

            // testing other parameters: (SharedAccessSignature, ;SharedAccessSignature?, BlobEndpoint=, FileEndpoint=, TableEndpoint=, QueueEndpoint= )
            if (!Test_OtherParameters(InputBoxSASvalue))
                return false;

            //---------------------------------------------------------------------
            // Retrieving values from Service endpoints
            //---------------------------------------------------------------------
            SAS.blobEndpoint = SAS_Utils.Get_SASValue(InputBoxSASvalue, "BlobEndpoint=", ";");
            SAS.fileEndpoint = SAS_Utils.Get_SASValue(InputBoxSASvalue, "FileEndpoint=", ";");
            SAS.tableEndpoint = SAS_Utils.Get_SASValue(InputBoxSASvalue, "TableEndpoint=", ";");
            SAS.queueEndpoint = SAS_Utils.Get_SASValue(InputBoxSASvalue, "QueueEndpoint=", ";");


            // If none Endpoint found, check if only URI (starting with http(s)://) was provided on InputBoxSAS
            // This is the case of the Service SAS
            //---------------------------------------------------------------------
            CheckURI_IfEndpointNotFound(InputBoxSASvalue);

            Get_StorageAccountNameFromEndpoint();


            //---------------------------------------------------------------------
            // Retrieving Shared Access Signature
            //---------------------------------------------------------------------
            // Connection string was provided
            SAS.onlySASprovided = false;
            SAS.sharedAccessSignature = SAS_Utils.Get_SASValue(InputBoxSASvalue, "SharedAccessSignature=", ";");

            // only SAS token was provided
            if (SAS.sharedAccessSignature == "not found")
            {
                SAS.sharedAccessSignature = SAS_Utils.Get_SASValue(InputBoxSASvalue, "?");

                if (Found(SAS.sharedAccessSignature, "not found="))
                    return Utils.WithMessage("Missing the '?' at the begin of SAS token, or the term 'SharedAccessSignature=' on a Connection String", "Invalid SAS");
                else
                    SAS.onlySASprovided = true;
            }




            //---------------------------------------------------------------------
            //---------------------------------------------------------------------
            // Retrieving the SAS parameters
            // Account SAS: https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-an-Account-SAS
            // Service SAS: https://docs.microsoft.com/en-us/rest/api/storageservices/constructing-a-service-sas
            //---------------------------------------------------------------------
            //---------------------------------------------------------------------
            SAS.apiVersion.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "api-version=", "&"); // Optional
            SAS.sv.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "sv=", "&");                  // Required (>=2015-04-05)
            SAS.ss.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "ss=", "&");
            SAS.srt.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "srt=", "&");
            SAS.sp.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "sp=", "&");
            SAS.se.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "se=", "&");
            SAS.st.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "st=", "&");
            SAS.sip.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "sip=", "&");
            SAS.spr.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "spr=", "&");

            SAS.sig = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "sig=", "&");



            //---------------------------------------------------------------------
            //---------------------------------------------------------------------
            // Retrieving the SAS parameters
            // Specific for Service SAS
            // Service SAS: https://docs.microsoft.com/en-us/rest/api/storageservices/constructing-a-service-sas
            //---------------------------------------------------------------------
            //---------------------------------------------------------------------
            // sr: signedresource - Required { b,c }   // blob, container - for blobs
            //                               { s,f }   // share, file     - for files
            //---------------------------------------------------------------------
            SAS.sr.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "sr=", "&");


            //---------------------------------------------------------------------
            // tn: tablename - Required. 
            // The name of the table to share.
            //---------------------------------------------------------------------
            SAS.tn.v = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "tn=", "&");


            //---------------------------------------------------------------------
            // The startpk, startrk, endpk, and endrk fields define a range of table entities associated with a shared access signature. 
            // Table queries will only return results that are within the range, and attempts to use the shared access signature to add, update, or delete entities outside this range will fail.
            // If startpk equals endpk, the shared access signature only authorizes access to entities in one partition in the table. 
            // If startpk equals endpk and startrk equals endrk, the shared access signature can only access one entity in one partition. 
            // Use the following table to understand how these fields constrain access to entities in a table.
            // startpk partitionKey >= startpk
            // endpk partitionKey <= endpk
            // startpk, startrk(partitionKey > startpk) || (partitionKey == startpk && rowKey >= startrk)
            // endpk, endrk(partitionKey < endpk) || (partitionKey == endpk && rowKey <= endrk)
            // https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-a-Service-SAS?redirectedfrom=MSDN#specifying-table-access-ranges
            //---------------------------------------------------------------------
            // spk: startpk - Table service only.
            // srk: startrk - Optional, but startpk must accompany startrk. The minimum partition and row keys accessible with this shared access signature. 
            //                Key values are inclusive. If omitted, there is no lower bound on the table entities that can be accessed.
            // epk: endpk - Table service only.
            // erk: endrk - Optional, but endpk must accompany endrk. The maximum partition and row keys accessible with this shared access signature. 
            //              Key values are inclusive. If omitted, there is no upper bound on the table entities that can be accessed.
            //---------------------------------------------------------------------
            SAS.spk = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "spk=", "&");
            SAS.epk = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "epk=", "&");
            SAS.srk = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "srk=", "&");
            SAS.erk = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "erk=", "&");

            //---------------------------------------------------------------------
            // si: signedidentifier - Optional.
            // A unique value up to 64 characters in length that correlates to an access policy specified for the container, queue, or table.
            //---------------------------------------------------------------------
            SAS.si = SAS_Utils.Get_SASValue(SAS.sharedAccessSignature, "si=", "&");

            return true;
        }



        /// <summary>
        /// testing other parameters:
        ///   SharedAccessSignature
        ///   ;SharedAccessSignature?
        ///   BlobEndpoint=
        ///   FileEndpoint=
        ///   TableEndpoint=
        ///   QueueEndpoint=
        /// </summary>
        /// <param name="InputBoxSAS"></param>
        /// <returns></returns>
        private bool Test_OtherParameters(string InputBoxSASvalue)
        {
            // test 'SharedAccessSignature' (connection string) or '?' SAS
            if (InputBoxSASvalue.IndexOf("?") != -1 && Found(InputBoxSASvalue, "SharedAccessSignature"))
                return Utils.WithMessage("Only one 'SharedAccessSignature' or '?' should be provided on SAS", "Invalid SAS");

            // no 'SharedAccessSignature=' or '?' found
            if (!Found(InputBoxSASvalue, "SharedAccessSignature=") && InputBoxSASvalue.IndexOf("?") == -1)
                return Utils.WithMessage("Missing 'SharedAccessSignature=' (for connection strings)\n or '?' (for SAS token), on the provided SAS", "Invalid SAS");

            //  SharedAccessSignature?= found
            if (Found(InputBoxSASvalue, "SharedAccessSignature?=") || Found(InputBoxSASvalue, "SharedAccessSignature=?"))
                return Utils.WithMessage("Invalid characters found after 'SharedAccessSignature',\n on the provided SAS", "Invalid SAS");


            if (Found(InputBoxSASvalue, "SharedAccessSignature=") && !EndpointsExists(InputBoxSASvalue))
                return Utils.WithMessage("Connection string found, but no Endpoints found.\nWithout endpoints, the SAS token should start with '?' instead of using 'SharedAccessSignature'", "Invalid SAS");

            if (InputBoxSASvalue.IndexOf("srt") == -1 && (InputBoxSASvalue.IndexOf("sr") != -1 || InputBoxSASvalue.IndexOf("tn") != -1)) // sr ot tn (Account SAS) found
            {
                // Endpoints found
                if (!EndpointFound(InputBoxSASvalue, "BlobEndpoint")) return false;
                if (!EndpointFound(InputBoxSASvalue, "FileEndpoint")) return false;
                if (!EndpointFound(InputBoxSASvalue, "TableEndpoint")) return false;
                if (!EndpointFound(InputBoxSASvalue, "QueueEndpoint")) return false;
            }

            if (InputBoxSASvalue.IndexOf("?") != -1 && EndpointsExists(InputBoxSASvalue))
                return Utils.WithMessage("Connection strings should be removed from the SAS token.\nOnly allowed on Connection string", "Invalid SAS");

            return true;
        }


        /// <summary>
        /// return true if any Endpoint exists in string s; otherwise return false
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool EndpointsExists(string s)
        {
            if (Found(s, "BlobEndpoint=")) return true;
            if (Found(s, "FileEndpoint=")) return true;
            if (Found(s, "QueueEndpoint=")) return true;
            if (Found(s, "TableEndpoint=")) return true;

            return false;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpointStr"></param>
        /// <returns></returns>
        private bool EndpointFound(string InputBoxSASvalue, string endpointStr)
        {
            if (InputBoxSASvalue.IndexOf(endpointStr) != -1)
            {
                MessageBox.Show("Term '" + endpointStr + "' should be removed from the Service SAS URI", "Invalid SAS", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else
                return true;
        }



        /// <summary>
        /// If none Endpoint found, check if only URI (starting with http(s)://) was provided on InputBoxSAS
        /// This is the case of the Service SAS
        /// </summary>
        private void CheckURI_IfEndpointNotFound(string inputBoxSAS)
        {
            if (SAS.blobEndpoint == "not found" && SAS.fileEndpoint == "not found" && SAS.tableEndpoint == "not found" && SAS.queueEndpoint == "not found")
            {
                string newEndpoint = "";
                string service = "";

                int i = inputBoxSAS.IndexOf("?");
                if (i != -1)
                    newEndpoint = inputBoxSAS.Substring(0, i);

                service = SAS_Utils.Get_SASValue(newEndpoint, ".", ".core");

                switch (service)
                {
                    case "blob":
                        SAS.blobEndpoint = newEndpoint;
                        break;
                    case "file":
                        SAS.fileEndpoint = newEndpoint;
                        break;
                    case "queue":
                        SAS.queueEndpoint = newEndpoint;
                        break;
                    case "table":
                        SAS.tableEndpoint = newEndpoint;
                        break;
                }
            }
        }




        /// <summary>
        /// Try to Get the Storage Account Name from any Endpoint, if provided
        /// </summary>
        private void Get_StorageAccountNameFromEndpoint()
        {
            SAS.storageAccountName.v = SAS_Utils.Get_SASValue(SAS.blobEndpoint, "://", ".");

            if (SAS.storageAccountName.v == "")
                SAS.storageAccountName.v = SAS_Utils.Get_SASValue(SAS.fileEndpoint, "://", ".");

            if (SAS.storageAccountName.v == "")
                SAS.storageAccountName.v = SAS_Utils.Get_SASValue(SAS.tableEndpoint, "://", ".");

            if (SAS.storageAccountName.v == "")
                SAS.storageAccountName.v = SAS_Utils.Get_SASValue(SAS.queueEndpoint, "://", ".");
        }






        /// <summary>
        /// Look on endpoints SAS struct strings, for values to SAS struct:
        ///    SAS.containerName 
        ///    SAS.blobName 
        ///    SAS.shareName 
        ///    SAS.fileName 
        ///    SAS.queueName 
        ///    SAS.tableName 
        /// </summary>
        private void Check_SASServiceEndpoints()
        {
            if (SAS.blobEndpoint != "not found")
            {
                // Format, if exists: https://<storage>.blob.core.windows.net/container
                string s1 = Get_SASValue(SAS.blobEndpoint, ".blob.core.windows.net/", "/");
                if (s1 != "not found")
                    SAS.containerName.v = s1.TrimStart().Replace("/", String.Empty);          // remove '/' at the end, if exists

                // Format, if exists: https://<storage>.blob.core.windows.net/container/blob
                if (SAS.containerName.v != "")
                {
                    string s2 = Get_SASValue(SAS.blobEndpoint, ".blob.core.windows.net/" + SAS.containerName + "/");
                    if (s2 != "not found")
                    {
                        // remove subfolders
                        int i = s2.LastIndexOf('/');
                        if (i == s2.Length - 1 && s2 != "")    // blob name ends with '/'
                            SAS.blobName.v = "<invalid blob on endpoint>";
                        else
                            if (i > 0)                          // sub folders exist - removing subfolders
                            SAS.blobName.v = s2.Substring(i + 1);
                        else                                // only blob name found after the container name
                            SAS.blobName.v = s2;
                    }
                }
            }

            if (SAS.fileEndpoint != "not found")
            {
                // Format, if exists: https://<storage>.file.core.windows.net/share
                string s1 = Get_SASValue(SAS.fileEndpoint, ".file.core.windows.net/", "/");
                if (s1 != "not found")
                    SAS.shareName.v = s1.TrimStart().Replace("/", String.Empty);              // remove '/' at the end, if exists

                // Format, if exists: https://<storage>.file.core.windows.net/share/file
                if (SAS.shareName.v != "")
                {
                    string s2 = Get_SASValue(SAS.fileEndpoint, ".file.core.windows.net/" + SAS.shareName + "/");
                    if (s2 != "not found")
                    {
                        // remove subfolders
                        int i = s2.LastIndexOf('/');
                        if (i == s2.Length - 1 && s2 != "")    // file name ends with '/'
                            SAS.fileName.v = "<invalid file on endpoint>";
                        else
                            if (i > 0)                          // sub folders exist - removing subfolders
                            SAS.fileName.v = s2.Substring(i + 1);
                        else                                // only file name found after the share name
                            SAS.fileName.v = s2;
                    }
                }
            }

            if (SAS.tableEndpoint != "not found")
            {
                // Format, if exists: https://<storage>.table.core.windows.net/table
                string s = Get_SASValue(SAS.tableEndpoint, ".table.core.windows.net/");
                int start = s.LastIndexOf("/") + 1; // start of the table name
                if (s != "not found" && start > 0)
                    SAS.tableName.v = s.Substring(start, s.Length - start);
            }

            if (SAS.queueEndpoint != "not found")
            {
                // Format, if exists: https://<storage>.queue.core.windows.net/queue
                string s = Get_SASValue(SAS.queueEndpoint, ".queue.core.windows.net/");
                int start = s.LastIndexOf("/") + 1; // start of the Queue name
                if (s != "not found" && start > 0)
                    SAS.queueName.v = s.Substring(start, s.Length - start);
            }
        }



        /// <summary>
        /// Return the type of the SAS detected
        /// </summary>
        private static string Show_SASType()
        {
            if (SAS.srt.v != "not found")
                return "Account SAS detected\n-----------------------------\n";
            else
            if (SAS.sr.v != "not found" || SAS.tn.v != "not found")
                return "Service SAS detected\n-----------------------------\n";
            else
                return "--> SAS type not detected (no 'srt', 'sr' or 'tn')\n";
        }



        /// <summary>
        /// Show SAS parameters results on BoxAuthResults
        /// </summary>
        /// <param name="BoxAuthResults"></param>
        private static void SAS_ShowResults(TextBox BoxAuthResults)
        {
            //---------------------------------------------------------------------
            //---------------------------------------------------------------------
            // Showing the results
            //---------------------------------------------------------------------
            //---------------------------------------------------------------------
            BoxAuthResults.Text += Show_SASType();

            BoxAuthResults.Text += "SAS Token (Unescaped): \n?" + SAS.sharedAccessSignature + "\n";
            BoxAuthResults.Text += "\n";

            // Verify if Services on URI are the same as on Service SAS permissions (Service SAS) 
            BoxAuthResults.Text += SAS_Utils.Show_Services();

            // api-version: (optional) {storage service version to use to execute the request made using the account SAS URI}
            BoxAuthResults.Text += SAS_Utils.Show_ApiVersion(SAS.apiVersion.v);

            // ss: {bqtf} - Required 
            // Service endpoints: (optional)
            BoxAuthResults.Text += SAS_Utils.Show_ss(SAS.ss.v, SAS.blobEndpoint, SAS.fileEndpoint, SAS.tableEndpoint, SAS.queueEndpoint, SAS.spr.v, SAS.onlySASprovided, SAS.srt.v);

            // spr: {https,http} - Optional (default both)
            BoxAuthResults.Text += SAS_Utils.Show_spr(SAS.spr.v, SAS.sr.v, SAS.tn.v, SAS.sv.v);

            // sv: Service Versions - Required (>=2015-04-05) - TODO more recent versions
            BoxAuthResults.Text += SAS_Utils.Show_sv(SAS.sv.v, SAS.sr.v, SAS.srt.v, SAS.tn.v);

            //---------------------------------------------------------------------
            // srt: Signed Resource Types {s,c,o} {Service, Container, Object} - Required
            // Specific for Service SAS: https://docs.microsoft.com/en-us/rest/api/storageservices/constructing-a-service-sas
            // sr: signedresource - Required { b,c }   // blob, container - for blobs
            //                               { s,f }   // share, file     - for files, version 2015-02-21 and later
            //                               {bs}      // blob snapshot,               version 2018-11-09 and later
            // tn: tablename - Required - The name of the table to share.
            //---------------------------------------------------------------------
            string s = SAS_Utils.Testing_sr_srt_tn(SAS.sr.v, SAS.srt.v, SAS.tn.v);
            switch (s)
            {
                case "":
                    BoxAuthResults.Text += "  --> No 'sr', 'srt' or 'tr' provided\n\n";
                    break;
                case "all":
                    BoxAuthResults.Text += "  --> Only one 'sr', 'srt' or 'tr' can be provided\n\n";
                    break;

                //---------------- Account SAS ----------------------------------------
                // srt: Signed Resource Types {s,c,o} {Service, Container, Object} - Required
                //---------------------------------------------------------------------
                case "srt":
                    BoxAuthResults.Text += SAS_Utils.Show_srt(SAS.srt.v);
                    break;

                //---------------- Service SAS ----------------------------------------
                // sr: signedresource - Required { b,c }   // blob, container - for blobs
                //                               { s,f }   // share, file     - for files, version 2015-02-21 and later
                //                               {bs}      // blob snapshot,               version 2018-11-09 and later
                //---------------------------------------------------------------------
                case "sr":
                    BoxAuthResults.Text += SAS_Utils.Show_sr(SAS.sr.v, SAS.sv.v);
                    break;

                //---------------- Service SAS ----------------------------------------
                // tn: tablename - Required - The name of the table to share.
                //---------------------------------------------------------------------
                case "tn":
                    BoxAuthResults.Text += SAS_Utils.Show_tn(SAS.tn.v);
                    break;
            }
            //---------------------------------------------------------------------


            // sp: SignedPermissions {r,w,d,l,a,c,u,p}  - Required
            // This field must be omitted if it has been specified in an associated stored access policy.  - TODO (Service SAS)
            BoxAuthResults.Text += SAS_Utils.Show_sp(SAS.sp.v, SAS.srt.v, SAS.sr.v, SAS.tn.v, SAS.sv.v);

            // st (SignedStart) - optional
            // se (SignedExpiry) - Required
            BoxAuthResults.Text += SAS_Utils.Show_st_se(SAS.st.v, SAS.se.v);

            // sip: Allowed IP's (optional)
            BoxAuthResults.Text += SAS_Utils.Show_sip(SAS.sip.v);

            // sig: Enchripted Signature - Required
            BoxAuthResults.Text += SAS_Utils.Show_sig(SAS.sig);



            //---------------------------------------------------------------------
            // The startpk, startrk, endpk, and endrk fields define a range of table entities associated with a shared access signature. 
            // Table queries will only return results that are within the range, and attempts to use the shared access signature to add, update, or delete entities outside this range will fail.
            // If startpk equals endpk, the shared access signature only authorizes access to entities in one partition in the table. 
            // If startpk equals endpk and startrk equals endrk, the shared access signature can only access one entity in one partition. 
            // Use the following table to understand how these fields constrain access to entities in a table.
            // startpk partitionKey >= startpk
            // endpk partitionKey <= endpk
            // startpk, startrk(partitionKey > startpk) || (partitionKey == startpk && rowKey >= startrk)
            // endpk, endrk(partitionKey < endpk) || (partitionKey == endpk && rowKey <= endrk)
            // https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-a-Service-SAS?redirectedfrom=MSDN#specifying-table-access-ranges
            //---------------------------------------------------------------------
            // spk: startpk - Table service only.
            // srk: startrk - Optional, but startpk must accompany startrk. The minimum partition and row keys accessible with this shared access signature. 
            //                Key values are inclusive. If omitted, there is no lower bound on the table entities that can be accessed.
            // epk: endpk - Table service only.
            // erk: endrk - Optional, but endpk must accompany endrk. The maximum partition and row keys accessible with this shared access signature. 
            //              Key values are inclusive. If omitted, there is no upper bound on the table entities that can be accessed.
            //---------------------------------------------------------------------
            BoxAuthResults.Text += SAS_Utils.Show_pk_rk(SAS.tn.v, SAS.spk, SAS.epk, SAS.srk, SAS.erk);

            //---------------------------------------------------------------------
            // si: signedidentifier - Optional.
            // A unique value up to 64 characters in length that correlates to an access policy specified for the container, queue, or table.
            //---------------------------------------------------------------------
            BoxAuthResults.Text += SAS_Utils.Show_si(SAS.si, SAS.srt.v);


        }





        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------



        /// <summary>
        /// Verify if Services on URI are the same as on Service SAS permissions
        /// </summary>
        public static string Show_Services()
        {
            // Table Service
            if (SAS.tn.v != "not found")
            {
                if (SAS.tableEndpoint == "not found")
                    return "  --> Table name 'tn' provided on SAS parameter, but a different or no service was provided on URI.\n\n";

                if (!(SAS.tableEndpoint.EndsWith(".net") || SAS.tableEndpoint.EndsWith(".net/")))
                    return "  --> Incorrect Table Endpoint format.\nTable Endpoint should not mention the table name on URI.\n\n";
            }

            // Blob Service
            if (SAS.sr.v == "b")
            {
                if (SAS.blobEndpoint == "not found")
                    return "  --> Blob was provided on 'sr' SAS parameter, but a different or no service was provided on URI.\n\n";
                if (SAS.blobName.v == "")
                    return "  --> Blob was provided on 'sr' SAS parameter, but no Blob file was provided on URI.\n\n";
            }
            if (SAS.sr.v == "c")
            {
                if (SAS.blobEndpoint == "not found")
                    return "  --> Container was provided on 'sr' SAS parameter, but a different or no service was provided on URI.\n\n";
                if (SAS.containerName.v == "")
                    return "  --> Container was provided on 'sr' SAS parameter, but no Container name was provided on URI.\n\n";
                if (SAS.blobName.v != "")
                    return "  --> Container was provided on 'sr' SAS parameter, but the Blob file should be removed from URI.\n\n";
            }
            if (SAS.sr.v == "bs")
            {
                if (SAS.blobEndpoint == "not found")
                    return "  --> Blob Snapshot was provided on 'sr' SAS parameter, but a different or no service was provided on URI.\n\n";
                if (SAS.blobName.v == "")
                    return "  --> Blob Snapshot was provided on 'sr' SAS parameter, but no Snapshot name was provided on URI.\n\n";
            }


            // File Service
            if (SAS.sr.v == "f")
            {
                if (SAS.fileEndpoint == "not found")
                    return "  --> File was provided on 'sr' SAS parameter, but a different or no service was provided on URI.\n\n";
                if (SAS.fileName.v == "")
                    return "  --> File was provided on 'sr' SAS parameter, but no File name was provided on URI.\n\n";
            }
            if (SAS.sr.v == "s")
            {
                if (SAS.fileEndpoint == "not found")
                    return "  --> Share was provided on 'sr' SAS parameter, but a different or no service was provided on URI.\n\n";
                if (SAS.shareName.v == "")
                    return "  --> Share was provided on 'sr' SAS parameter, but no Share name was provided on URI.\n\n";
                if (SAS.fileName.v != "")
                    return "  --> Share was provided on 'sr' SAS parameter, but the File name should be removed from URI.\n\n";
            }


            // Queue Service
            if ((SAS.sr.v == "???") && SAS.queueEndpoint == "not found")
                return "  --> Queue was provided on 'sr' SAS parameter, but a different service was provided on URI.\n\n";
            
            return "";
        }



        /// <summary>
        /// Search on source and returns the string value between SASHeader and delimiter, or the end of the string if delimiter not found
        /// debug=true : adds additional information
        /// </summary>
        public static string Get_SASValue(string source, string SASHeader, string delimiter, bool debug = false)
        {
            string s2 = "not found";
            int i = source.IndexOf(SASHeader);
            int lenght = 0;
            if (i >= 0)
            {
                i += SASHeader.Length;
                lenght = source.IndexOf(delimiter, i) - i;
                if (lenght >= 0)
                    s2 = source.Substring(i, lenght);

                if (lenght < 0) // delimeter not found - end of the string
                    s2 = source.Substring(i);
            }

            if (debug)
                return "SASHeader:" + SASHeader + "  i:" + i.ToString() + "  lenght:" + lenght.ToString() + "  s2:" + s2;
            return s2;
        }



        /// <summary>
        /// Search on source and returns the string value between SASHeader and the end on the source.
        /// debug=true : adds additional information
        /// </summary>
        public static string Get_SASValue(string source, string SASHeader, bool debug = false)
        {
            string s2 = "not found";
            int i = source.IndexOf(SASHeader);
            if (i >= 0)
                s2 = source.Substring(i + SASHeader.Length);

            if (debug)
                return "SASHeader:" + SASHeader + "  i:" + i.ToString() + "  s2:" + s2;
            return s2;
        }


        /// <summary>
        /// Search on source to seach any space or new line in the source.
        /// </summary>
        public static string Get_SASSpace(string source)
        {
            int i = -1;

            i = source.IndexOf(" ");   // space
            if (i != -1)
                return "Space";
            else
                i = source.IndexOf("\r\n");
            if (i == -1) i = source.IndexOf("\r");
            if (i == -1) i = source.IndexOf("\n");
            if (i != -1)
                return "New Line";
            return "";
        }







        /// <summary>
        /// api-version: (optional) {storage service version to use to execute the request made using the account SAS URI}
        /// 
        /// You can specify two versioning options on a shared access signature. 
        /// If specified, the optional api-version header indicates which service version to use to execute the API operation. 
        /// The SignedVersion (sv) parameter specifies the service version to use to authorize and authenticate the request made with the SAS. 
        /// If the api-version header is not specified, then the value of the SignedVersion (sv) parameter also indicates the version to 
        /// use to execute the API operation.
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/versioning-for-the-azure-storage-services#specifying-service-versions-in-requests
        /// </summary>
        public static string Show_ApiVersion(string apiVersion)
        {
            // no api-version (optional) provided - exit without any information
            if (apiVersion == "not found")
                return "";

            string s = "'api-version' parameter (Storage Service Version):\n";

            // api-version without any value
            if (String.IsNullOrEmpty(apiVersion))
                return andSetState("apiVersion", false, s += "  --> 'api-version' provided without any value (api-version=" + apiVersion + ")\n\n");

            // api-version length and format - yyyy-mm-dd
            if (apiVersion.Length != 10 || apiVersion.Substring(4, 1) != "-" || apiVersion.Substring(7, 1) != "-")
                return andSetState("apiVersion", false, s += "-- > Invalid format on 'api-version' provided (api-version=" + apiVersion + ")\n\n");

            // api-version date validation - TODO - validate the storage service versions
            try
            {
                DateTime d = Convert.ToDateTime(apiVersion);
            }
            catch (Exception ex)
            {
                return andSetState("apiVersion", false, s += "-- > Invalid date on 'api-version' provided (api-version=" + apiVersion + "): " + ex.Message + "\n\n");
            }

            return andSetState("apiVersion", true, s += "  API Version used: " + apiVersion + "\n\n");
        }




        /// <summary>
        /// Format the output string for ss parameter and Endpoints provided on connection string
        /// b,f,t,q
        /// </summary>
        public static string Show_ss(string ss, string blobEndpoint, string fileEndpoint, string tableEndpoint, string queueEndpoint, string spr, bool onlySASprovided, string srt)
        {
            string s = "'ss' parameter (Signed Services):\n";

            if (srt == "not found")     // Service SAS - ss não requerido
                if (ss == "not found")
                    return "";
                else
                    return andSetState("ss", false, s += "-- > 'ss' Not required on Service SAS, but provided)\n\n");

            // no (required) sp provided
            if (ss == "not found")
                return andSetState("ss", false, s += "  --> 'ss' Required but not provided)\n\n");

            // found chars not supported by ss paramenter
            if (!Utils.ValidateString(ss, "bfqt"))
                return andSetState("ss", false, s += "  --> Invalid 'ss' parameter provided: (ss=" + ss + ")\n\n");

            if (onlySASprovided)     // only SAS token was provided
            {
                s += "  Blob Service " + (ss.IndexOf("b") == -1 ? "NOT " : "") + "allowed\n";
                s += "  File Service " + (ss.IndexOf("f") == -1 ? "NOT " : "") + "allowed\n";
                s += "  Table Service " + (ss.IndexOf("t") == -1 ? "NOT " : "") + "allowed\n";
                s += "  Queue Service " + (ss.IndexOf("q") == -1 ? "NOT " : "") + "allowed\n";
            }
            else                    // Connection string was provided
            {
                s += "  Blob Service " + (ss.IndexOf("b") == -1 ? "NOT allowed" : "allowed " + (blobEndpoint == "not found" ? "--> but Blob Endpoint NOT provided." : Test_EndpointFormat(blobEndpoint, "Blob", spr))) + "\n";
                s += "  File Service " + (ss.IndexOf("f") == -1 ? "NOT allowed" : "allowed " + (fileEndpoint == "not found" ? "--> but File Endpoint NOT provided." : Test_EndpointFormat(fileEndpoint, "File", spr))) + "\n";
                s += "  Table Service " + (ss.IndexOf("t") == -1 ? "NOT allowed" : "allowed " + (tableEndpoint == "not found" ? "--> but Table Endpoint NOT provided." : Test_EndpointFormat(tableEndpoint, "Table", spr))) + "\n";
                s += "  Queue Service " + (ss.IndexOf("q") == -1 ? "NOT allowed" : "allowed " + (queueEndpoint == "not found" ? "--> but Queue Endpoint NOT provided." : Test_EndpointFormat(queueEndpoint, "Queue", spr))) + "\n";
            }

            return andSetState("ss", true, s + "\n");
        }



        /// <summary>
        /// Format the output string for Endpoints provided on connection string
        /// </summary>
        public static string Test_EndpointFormat(string endpoint, string service, string spr)
        {
            // checking spr and protocol endpoint
            switch (spr)
            {
                case "http":
                    if (endpoint.IndexOf("http://") == -1)
                        return "  --> http protocol provided on 'spr' parameter, but missing/wrong protocol provided on " + service + " Endpoint " + "  (" + endpoint + ")";
                    break;
                case "https":
                    if (endpoint.IndexOf("https://") == -1)
                        return "  --> https protocol provided on 'spr' parameter, but missing/wrong protocol provided on " + service + " Endpoint " + "  (" + endpoint + ")";
                    break;
                default:
                    if (endpoint.IndexOf("http://") == -1 && endpoint.IndexOf("https://") == -1)
                        return "  --> but missing/wrong protocol provided on " + service + " Endpoint " + "  (" + endpoint + ")";
                    break;
            }

            // checking the '.core.windows.net' part on the endpoint URI 
            if (endpoint.IndexOf(".core.windows.net") == -1)
                return "  --> but wrong " + service + " Endpoint URI provided " + "  (" + endpoint + ")";

            // checking for some storage account name on the endpoint URI      TODO - check the storage account name used
            if (endpoint.IndexOf("http://.") != -1 || endpoint.IndexOf("https://.") != -1)
                return "  --> but missing the Storage Account name on " + service + " Endpoint " + "  (" + endpoint + ")";

            // checking the service on the endpoint URI (blob, queue, table, file) 
            if (endpoint.IndexOf(service.ToLower()) == -1)
                return " --> but wrong service provided on " + service + " Endpoint URI " + "  (" + endpoint + ")";

            return " - " + service + " Endpoint: " + endpoint;
        }



        /// <summary>
        /// Format the output string for spr parameter and Endpoints provided on connection string
        /// Note that HTTP only is not a permitted value.
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/constructing-an-account-sas#constructing-the-account-sas-uri
        /// </summary>
        public static string Show_spr(string spr, string sr, string tn, string sv)
        {
            string s = "'spr' parameter (Allowed Protocols):\n";

            // no (optional) spr provided
            if (spr == "not found")
                return andSetState("spr", true, s += "  HTTPS, HTTP protocols allowed (spr optional not provided)\n\n");

            // spr empty
            if (spr.Length == 0)
                return andSetState("spr", false, s += "  --> Empty optional Allowed Protocols value (spr=" + spr + ")\n\n");

            // on Service SAS the 'spr' (protocol) is only supported on Service Version >= 2015-04-05
            if ((sr != "not found" || tn != "not found") && sv.CompareTo("2015-04-05") < 0)
                return andSetState("spr", false, s += "  --> 'spr' parameter only supported on Service Version ('sv') 2015-04-05 or later (sv=" + sv + ")\n\n");

            // found chars not supported by spr paramenter
            if (!Utils.ValidateString(spr, "https,"))
                return andSetState("spr", false, s += "  --> Invalid protocol provided on 'spr' parameter  (spr=" + spr + ")\n\n");

            // both protocols provided on spr 
            if (spr.IndexOf("http,https") != -1 || spr.IndexOf("https,http") != -1)
                return andSetState("spr", true, s += "  HTTPS, HTTP protocols allowed.\n\n");

            // https protocol provided on spr
            if (spr == "https")
                return andSetState("spr", true, s += "  HTTPS protocol only allowed.\n\n");

            // http protocol provided on spr (needed to prevent find http in https or httpxxx)
            if (spr == "http")
                return andSetState("spr", false, s += "  --> HTTP protocol only is not a allowed.\n\n");

            return andSetState("spr", false, s += "  --> wrong protocol provided on 'spr' parameter  (spr=" + spr + ")\n\n");
        }





        /// <summary>
        /// Format the output string for sv parameter 
        ///         
        /// You can specify two versioning options on a shared access signature. 
        /// If specified, the optional api-version header indicates which service version to use to execute the API operation. 
        /// The SignedVersion (sv) parameter specifies the service version to use to authorize and authenticate the request made with the SAS. 
        /// If the api-version header is not specified, then the value of the SignedVersion (sv) parameter also indicates the version to 
        /// use to execute the API operation.
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/versioning-for-the-azure-storage-services#specifying-service-versions-in-requests
        /// ----------------------------------------------------------------------
        /// Valid Storage Service Versions - Account SAS - Required (>=2015-04-05) - TODO more recent versions
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/constructing-an-account-sas#constructing-the-account-sas-uri
        /// 
        /// Valid Storage Service Versions - Service SAS - Required (>=2012-02-12) - TODO more recent versions
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-a-Service-SAS?redirectedfrom=MSDN#specifying-the-signed-version-field
        /// 2015-02-21
        /// 2014-02-14
        /// 2013-08-15
        /// 2012-02-12
        /// 
        /// Before version 2012-02-12, a shared access signature not associated with a stored access policy could not have an active period that exceeded one hour.
        /// Before version 2012-02-12 is supported ??? - TODO
        /// 
        /// Available Versions:
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/previous-azure-storage-service-versions
        /// --------------------------------------------------------------------------
        /// </summary>
        public static string Show_sv(string sv, string sr, string srt, string tn)
        {
            string s = "'sv' parameter (Storage Service Version):\n";

            // no (required) sv provided
            if (sv == "not found")
                return andSetState("sv", false, s += "  --> 'sv' Required but not provided)\n\n");

            // sv empty
            if (sv.Length == 0)
                return andSetState("sv", false, s += "  --> Empty Storage Version value (sv=" + sv + ")\n\n");


            // Check Account SAS
            for (int i = 0; i < validSV_AccountSAS_ARR.Length; i++)
                if (validSV_AccountSAS_ARR[i] == sv)
                    return andSetState("sv", true, s += "  Valid Storage Service Version for Account SAS  (sv=" + sv + ")\n\n");

            // Check Service SAS
            if (sr!="" || tn!="")
                for (int i = 0; i < validSV_ServiceSas_ARR_addon.Length; i++)
                    if (validSV_ServiceSas_ARR_addon[i] == sv)
                        return andSetState("sv", true, s += "  Valid Storage Service Version for Service SAS (sv=" + sv + ")\n\n");

      
            return andSetState("sv", false, s += "  --> Invalid Storage Service Version  (sv=" + sv + ")\n" +
                        "      Please visit the link below to check for new versions that may not yet be supported by this Storage Helper SAS Tool:\n" +
                        "      https://docs.microsoft.com/en-us/rest/api/storageservices/previous-azure-storage-service-versions \n\n");
        }

   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="srt"></param>
        /// <param name="tn"></param>
        /// <returns></returns>
        public static string Testing_sr_srt_tn(string sr, string srt, string tn)
        {
            // no (required) srt provided
            if (sr == "not found" && srt == "not found" && tn == "not found")
                return "";

            // Two or more srt, sr or tn provided
            if ((sr != "not found" && srt != "not found") || (sr != "not found" && tn != "not found") || (srt != "not found" && tn != "not found"))
                return "all";

            if (sr != "not found")
                return "sr";

            if(srt != "not found")
                return "srt";

            if (tn != "not found")
                return "tn";

            return "";
        }


        /// <summary>
        /// Format the output string for srt parameter 
        /// SignedResourceTypes {s,c,o} Service, Container, Object} - Required
        /// </summary>
        /// <param name="srt"></param>
        /// <returns></returns>
        public static string Show_srt(string srt)
        {
            string s = "'srt' parameter (Signed Resource Type):\n";

            // no (required) srt provided
            if (srt == "not found")
                return andSetState("srt", false, s += "  --> 'srt' Required but not provided\n\n");

            // srt empty
            if (srt.Length == 0)
                return andSetState("srt", false, s += "  --> Empty Signed Resource Type value (srt=" + srt + ")\n\n");

            // found chars not supported by srt paramenter
            if (!Utils.ValidateString(srt, "sco"))
                return andSetState("srt", false, s += "  --> Invalid Signed Resource Types  (srt=" + srt + ")\n\n");

            if (srt.Length <= 3)
            {
                if (srt.IndexOf("s") != -1)
                    s += "  Access to Service-level APIs (Get/Set Service Properties, Get Service Stats, List Containers/Queues/Tables/Shares)\n";

                if (srt.IndexOf("c") != -1)
                    s += "  Access to Container-level APIs (Create/Delete Container, Create/Delete Queue, Create/Delete Table, Create/Delete Share, List Blobs/Files and Directories)\n";

                if (srt.IndexOf("o") != -1)
                    s += "  Access to Object-level APIs (Put Blob, Query Entity, Get Messages, Create File, etc.)\n";
            }
            else
                return andSetState("srt", false, s += "  --> Invalid Signed Resource Types  (srt=" + srt + ")\n\n");

            return andSetState("srt", true, s + "\n");
        }



        /// <summary>
        /// Format the output string for sr parameter 
        /// SignedResource    sr:  Required { b,c }   // blob, container - for blobs
        ///                                 { s,f }   // share, file     - for files, version 2015-02-21 and later
        ///                                 {bs}      // blob snapshot,               version 2018-11-09 and later
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Show_sr(string sr, string sv)
        {
            string s = "'sr' parameter (Signed Resource):\n";

            // no (required) sr provided
            if (sr == "not found")
                return andSetState("sr", false, s += "  --> 'sr' Required but not provided\n\n");

            if(sr.Length >1 && sr!="bs")
                return andSetState("sr", false, s += "  --> Only one signed resource (b,c,s,f,bs) can be provided  (sr=" + sr + ")\n\n");

            // found chars not supported by sr paramenter
            if (!Utils.ValidateString(sr, "bcsf"))
                return andSetState("sr", false, s += "  --> Invalid Signed Resource  (sr=" + sr + ")\n\n");

            // Verifying the Service Version for Files
            if((sr=="s" || sr =="f") && sv.CompareTo("2015-02-21")<0)
                return andSetState("sr", false, s += "  --> The File and Share resouces (s,f) are only supported on version 2015-02-21 and later (sr=" + sr + ", sv=" + sv + ")\n\n");

            // Verifying the Service Version for Blob Snapshots
            if (sr == "bs" && sv.CompareTo("2018-11-09") < 0)
                return andSetState("sr", false, s += "  --> The Blob Snapshot resouce (bs) are only supported on version 2018-11-09 and later (sr=" + sr + ", sv=" + sv + ")\n\n");

            bool state=true;
            switch (sr)
            {
                case "b":
                    s += "  Access to Blob (grants access to the content and metadata of the blob)\n";
                    break;

                case "c":
                    s += "  Access to Container (grants access to the content and metadata of any blob in the container, and to the list of blobs in the container)\n";
                    break;

                case "s":
                    s += "  Access to File Share (grants access to the content and metadata of any file in the share, and to the list of directories and files in the share)\n";
                    break;

                case "f":
                    s += "  Access to File (grants access to the content and metadata of the file.)\n";
                    break;

                case "bs":
                    s += "  Access to Blob Snapshot (grants access to the content and metadata of the specific snapshot, but not the corresponding root blob)\n";
                    break;

                default:
                    s += "  --> Invalid Signed Resource Types  (sr=" + sr + ")\n";
                    state = false;
                    break;
            }

            return andSetState("sr", state, s + "\n");
        }



        /// <summary>
        /// Format the output string for tr parameter 
        /// TableResource {TableName} The name of the table to share - Required
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        public static string Show_tn(string tn)
        {
            string s = "'tn' parameter (Table Name):\n";

            // no (required) tn provided
            if (tn == "not found")
                return andSetState("tn", false, s += "  --> 'tn' Required but not provided\n\n");

            s += "  Access to Table '"+ tn + "'\n";

            return andSetState("tn", true, s + "\n");
        }



        /// <summary>
        /// Format the output string for startpk, startrk, endpk, and endrk parameter 
        /// TableResource {TableName} The name of the table to share - Required
        /// 
        /// Specifying table access ranges
        /// The startpk, startrk, endpk, and endrk fields define a range of table entities associated with a shared access signature.
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="startpk"></param>
        /// <param name="endpk"></param>
        /// <param name="startrk"></param>
        /// <param name="endrk"></param>
        /// <returns></returns>
        public static string Show_pk_rk(string tn, string startpk, string endpk, string startrk, string endrk)
        {
            // no table access ranges provided
            if (startpk == "not found" && endpk == "not found" && startrk == "not found" && endrk == "not found")
                return ""; // No table access ranges specified (optional)

            string s = "'startpk', 'endpk', 'startrk', 'endrk' parameters - Table access range - Partition and Row Keys:\n";

            // no tn provided to apply partition and row ranges
            if (tn == "not found")
                return s += "  --> 'tn' Required but not provided, when specifying 'startpk', 'endpk', 'startrk', 'endrk' parameters  (tn=" + tn + ")\n\n";


            // one partition key provided -> need provide both ??? - TODO
            if ((startpk == "not found" && endpk != "not found") || (startpk != "not found" && endpk == "not found"))
                return s += "  --> Missing Table Partition Key on specifying allowed Partitions (startpk=" + startpk + ", endpk=" + endpk + ")\n\n";

            // one row key provided -> need provide both ??? - TODO
            if ((startrk == "not found" && endrk != "not found") || (startrk != "not found" && endrk == "not found"))
                return s += "  --> Missing Table Row Key on specifying allowed Rows (startrk=" + startrk + ", endrk=" + endrk + ")\n\n";


            if (startpk != "not found" && endpk != "not found")
            { 
                if(startpk.Length == 0)
                    return s += "  --> Empty Start Partition Key value (startpk=" + startpk + ")\n\n";
                if (endpk.Length == 0)
                    return s += "  --> Empty End Partition Key value (endpk=" + endpk + ")\n\n";

                s += "  Allowing Table access from Partition '"+ startpk + "' to '"+ endpk + "'\n";
            }

            if (startrk != "not found" && endrk != "not found")
            {
                if (startrk.Length == 0)
                    return s += "  --> Empty Start Row Key value (startrk=" + startrk + ")\n\n";
                if (endrk.Length == 0)
                    return s += "  --> Empty End Row Key value (endrk=" + endrk + ")\n\n";

                s += "  Allowing Table access from Row '" + startrk + "' to '" + endrk + "'\n";
            }

            return s + "\n";
        }



        /// <summary>
        /// Format the output string for si parameter 
        /// si: signedidentifier - Optional.
        /// A unique value up to 64 characters in length that correlates to an access policy specified for the container, queue, or table.
        /// </summary>
        /// <param name="si"></param>
        /// <returns></returns>
        public static string Show_si(string si, string srt)
        {
            string s = "'si' parameter (Signed Identifier - Policy):\n";

            // no si provided (optional)
            if (si == "not found")
                return "";

            if (si.Length > 64)
                return s += "  --> Invalid Access Policy Name - Max 64 chars supported (si=" + si + " - current "+ si.Length.ToString() + " chars)\n\n";

            if (srt != "not found")
                return s += "  --> Policy Name ('si') not supported on Account SAS (srt=" + srt + ", si=" + si + ")\n\n";

            s += "  Access Policy Name used: '" + si + "'\n";

            s += "  Policy Permissions: TODO " ;

            return s + "\n";
        }




        /// <summary>
        /// Format the output string for sp parameter - SignedPermissions {r,w,d,l,a,c,u,p}  - Required
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="srt"></param>
        /// <param name="sr"></param>
        /// <param name="tn"></param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Show_sp(string sp, string srt, string sr, string tn, string sv)
        {
            string s = "'sp' parameter (Signed Permissions):\n";

            // no (required) sp provided
            if (sp == "not found")
                return andSetState("sp", false, s += "  --> 'sp' Required but not provided\n\n");

            if (sp.Length == 0)
                return andSetState("sp", false, s += "  --> Empty Signed Permissions (sp=" + sp + ")\n\n");

            //---------------------------------------------
            // State (true/false) defined inside Show_sp_srt()
            if (srt != "not found")
                return s += Show_sp_srt(sp, srt, sv) + "\n";

            if (sr != "not found")
                return s += Show_sp_sr(sp, sr, sv) + "\n";

            if (tn!= "not found")
                return s += Show_sp_tn(sp, tn, sv) + "\n";

            // How define Queue ???? - TODO
            //if (sr != "not found")
            //    return s += Show_sp_queue(sp, sv) + "\n";

            return s + "\n";
        }

        /// <summary>
        /// Account SAS for Service, Object, Containers
        /// https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-account-sas#specifying-account-sas-parameters
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="srt"></param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Show_sp_srt(string sp, string srt, string sv)
        {
            string s = "";
            string v = "Valid permissions for Blobs: 'rwdlacup'";
            bool state = true;

            // found chars not supported by sp paramenter
            if (!Utils.ValidateString(sp, "rwdlacup") || sp.Length > 8)
                return andSetState("sp", false, s += "  --> Invalid Signed Permissions for Account SAS (sp=" + sp + ", srt=" + srt + "). " + v + "\n");

            // Permissions by Service
            if (srt.IndexOf("o") != -1)
            { 
                s += "  Permissions for Objects (Account SAS) (o in 'srt'):\n";
                if (sp.IndexOf("r") != -1)
                    s += "    Read Objects\n";
                if (sp.IndexOf("w") != -1)
                    s += "    Write Objects\n";
                if (sp.IndexOf("d") != -1)
                    s += "    Delete Objects, except for Queue messages\n";
                // 'l' - List Not supported
                if (sp.IndexOf("a") != -1)
                    s += "    Add queue messages, table entities, and append blobs only\n";
                if (sp.IndexOf("c") != -1)
                    s += "    Create new blobs or files, but not overwrite existing blobs or files\n";
                if (sp.IndexOf("u") != -1)
                    s += "    Update queue messages and table entities only\n";
                if (sp.IndexOf("p") != -1)
                    s += "    Process queue messages only\n";
    
                if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("c") == -1 && sp.IndexOf("u") == -1 && sp.IndexOf("p") == -1)
                { 
                    s += "    --> No Permissions for Objects (sp=" + sp + ")\n";
                    state = false;
                }
            }

            if (srt.IndexOf("s") != -1)
            {
                s += "  Permissions for Services (Account SAS) (s in 'srt'):\n";
                if (sp.IndexOf("r") != -1)
                    s += "    Read Services\n";
                if (sp.IndexOf("w") != -1)
                    s += "    Write Services\n";
                // 'd' - Delete not supported
                if (sp.IndexOf("l") != -1)
                    s += "    List Services\n";
                // 'a' - Add Not supported
                // 'c' - Create Not supported
                // 'u' - Update Not supported
                // 'p' - Process Not supported

                if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("l") == -1)
                { 
                    s += "    --> No Permissions for Services (sp=" + sp + ")\n";
                    state = false;
                }
        }

            if (srt.IndexOf("c") != -1)
            {
                s += "  Permissions for Containers (Account SAS) (c in 'srt'):\n";
                if (sp.IndexOf("r") != -1)
                    s += "    Read Containers\n";
                if (sp.IndexOf("w") != -1)
                    s += "    Write Containers\n";
                if (sp.IndexOf("d") != -1)
                    s += "    Delete Containers\n";
                if (sp.IndexOf("l") != -1)
                    s += "    List Containers\n";
                // 'a' - Add Not supported
                // 'c' - Create Not supported
                // 'u' - Update Not supported
                // 'p' - Process Not supported

                if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("l") == -1)
                { 
                    s += "    --> No Permissions for Containers (sp=" + sp + ")\n";
                    state = false;
                }
            }

            SAS.sp.s = state;
            return s;
        }



        /// <summary>
        /// Service SAS for Blob, Container, File, Share
        /// Same permissions for Policy and 'sr' (Service SAS) parameter
        /// https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-service-sas?redirectedfrom=MSDN#permissions-for-a-blob
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="sr">
        /// signedresource - Required { b,c }   // blob, container - for blobs
        //                            { s,f }   // share, file     - for files, version 2015-02-21 and later
        //                            {bs}      // blob snapshot,               version 2018-11-09 and later</param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Show_sp_sr(string sp, string sr, string sv)
        {
            string s = "";
            string v = "";

            switch (sr)
            {
                case "b":
                    v = "Valid permissions for Blobs: 'rwdac'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdac") || sp.Length > 5)
                        return andSetState("sp", false, s += "  --> Invalid Signed Permissions for Blobs (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v + "\n");

                    //----------------------------------------------
                    s += "  Permissions for Blob (Service SAS) ('sr'=b):\n";
                    if (sp.IndexOf("r") != -1)
                        s += "    Read the content, properties, metadata and block list. Use the blob as the source of a copy operation.\n";

                    if (sp.IndexOf("w") != -1)
                        s += "    Create or write content, properties, metadata, or block list. Snapshot or lease the blob. Resize the blob (page blob only). Use the blob as the destination of a copy operation.\n";

                    if (sp.IndexOf("d") != -1)
                        if (sv.CompareTo("2017-07-29") < 0)
                            s += "    Delete the blob\n";
                        else
                            s += "    Delete the blob and breaking a lease on a blob\n";

                    if (sp.IndexOf("a") != -1)
                        s += "    Add a block to an append blob.\n";

                    if (sp.IndexOf("c") != -1)
                        s += "    Write a new blob, snapshot a blob, or copy a blob to a new blob\n";


                    if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("c") == -1)
                    {
                        s += "    --> No Permissions for Blobs (sp=" + sp + ")\n";
                        SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "c":
                    v = "Valid permissions for Containers: 'rwdacl'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdacl") || sp.Length > 6)
                        return andSetState("sp", false, s += "  --> Invalid Signed Permissions for Containers (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v + "\n");

                    //----------------------------------------------
                    s += "  Permissions for Containers (Service SAS) ('sr'=c):\n";
                    if (sp.IndexOf("r") != -1)
                        s += "    Read the content, properties, metadata or block list of any blob in the container. Use any blob in the container as the source of a copy operation.\n";

                    if (sp.IndexOf("w") != -1)
                        s += "    For any blob in the container, create or write content, properties, metadata, or block list. Snapshot or lease the blob. Resize the blob (page blob only). Use the blob as the destination of a copy operation.\n";

                    if (sp.IndexOf("d") != -1)
                        if (sv.CompareTo("2017-07-29") < 0)
                            s += "    Delete the blob in the container.\n";
                        else
                            s += "    Delete any blob in the container, breaking a lease on a container.\n";

                    if (sp.IndexOf("a") != -1)
                        s += "    Add a block to any append blob in the container.\n";

                    if (sp.IndexOf("c") != -1)
                        s += "    Write a new blob to the container, snapshot any blob in the container, or copy a blob to a new blob in the container.\n";

                    if (sp.IndexOf("l") != -1)
                        s += "    List blobs in the container.\n";

                    if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("c") == -1 && sp.IndexOf("l") == -1)
                    {
                        s += "    --> No Permissions for Containers (sp=" + sp + ")\n";
                        SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "s":
                    v = "Valid permissions for File Share: 'rwdlc'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdlc") || sp.Length > 5)
                        return andSetState("sp", false, s += "  --> Invalid Signed Permissions to File Share (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v + "\n");

                    if (sv.CompareTo("2015-02-21") < 0)
                        return andSetState("sp", false, s += "  --> Invalid Service Version to use File Share permissions on SAS. Needed sv=2015-02-21 or later (sr=" + sr + ", sv=" + sv + ")\n");

                    //----------------------------------------------
                    s += "  Permissions for File Share (Service SAS) ('sr'=s):\n";
                    if (sp.IndexOf("r") != -1)
                        s += "    Read the content, properties or metadata of any file in the share. Use any file in the share as the source of a copy operation.\n";

                    if (sp.IndexOf("w") != -1)
                        s += "    For any file in the share, create or write content, properties or metadata.Resize the file.Use the file as the destination of a copy operation.\n";

                    if (sp.IndexOf("d") != -1)
                        s += "    Delete any file in the share.\n";

                    if (sp.IndexOf("l") != -1)
                        s += "    List files and directories in the share.\n";

                    if (sp.IndexOf("c") != -1)
                        s += "    Create a new file in the share, or copy a file to a new file in the share.\n";

                    if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("l") == -1 && sp.IndexOf("c") == -1)
                    {
                        s += "    --> No Permissions for File Share (sp=" + sp + ")\n";
                        SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "f":
                    v = "Valid permissions for Files: 'rwdc'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdc") || sp.Length > 4)
                        return andSetState("sp", false, s += "  --> Invalid Signed Permissions for File (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v + "\n");

                    if (sv.CompareTo("2015-02-21") < 0)
                        return andSetState("sp", false, s += "  --> Invalid Service Version to File permissions on SAS. Needed sv=2015-02-21 or later (sr=" + sr + ", sv=" + sv + ")\n");

                    //----------------------------------------------
                    s += "  Permissions for File (Service SAS) ('sr'=f):\n";
                    if (sp.IndexOf("r") != -1)
                        s += "    Read the content, properties, metadata.Use the file as the source of a copy operation.\n";

                    if (sp.IndexOf("w") != -1)
                        s += "    Create or write content, properties, metadata.Resize the file.Use the file as the destination of a copy operation.\n";

                    if (sp.IndexOf("d") != -1)
                        s += "    Delete the file.\n";

                    if (sp.IndexOf("c") != -1)
                        s += "    Create a new file or copy a file to a new file.\n";

                    if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("c") == -1)
                    { 
                        s += "    --> No Permissions for Files (sp=" + sp + ")\n";
                        SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "bs":
                    // Valid permissions ??? - TODO
                    //----------------------------------------------
                    v = "Valid permissions for Blob Shapshots: 'rwdlacup' - TODO ???? ";

                    s += "  Permissions for Blob Shapshots (Service SAS) ('sr'=bs):\n";
                    
                    if (sv.CompareTo("2018-11-09") < 0)
                        return andSetState("sp", false, s += "  --> Invalid Service Version to use Blob Shapshots (Service SAS). Needed sv=2018-11-09 or later (sr=" + sr + ", sv=" + sv + ")\n");

                    if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("c") == -1 && sp.IndexOf("u") == -1 && sp.IndexOf("p") == -1)
                    { 
                        s += "    --> No Permissions for Blob Shapshots (sp=" + sp + "). " + v + "\n";
                        SAS.sp.s = false;
                    }
                    return s += "    Valid Permissions sp=" + sp + ", for Blob Shapshots\n";
            }

            return s;
        }



        /// <summary>
        /// Service SAS for Table
        /// Same permissions for Policy and 'tn' (Service SAS) parameter 
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Show_sp_tn(string sp, string tn, string sv)
        {
            string s = "";
            string v = "Valid permissions for Table Service: 'raud'";

            // found chars not supported by sp paramenter
            if (!Utils.ValidateString(sp, "raud") || sp.Length > 4)
                return andSetState("sp", false, s += "  --> Invalid Signed Permissions for Table (Service SAS) (sp=" + sp + "). " + v + "\n");

            //----------------------------------------------
            s += "  Permissions for Table (Service SAS) (Table name 'tn'=" + tn + "):\n";
            if (sp.IndexOf("r") != -1)
                s += "    Get entities and query entities.\n";

            if (sp.IndexOf("a") != -1)
                s += "    Add entities. Note: Add and Update permissions are required for upsert operations.\n";

            if (sp.IndexOf("u") != -1)
                s += "    Update entities. Note: Add and Update permissions are required for upsert operations.\n";

            if (sp.IndexOf("d") != -1)
                s += "    Delete entities.\n";

            if (sp.IndexOf("r") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("u") == -1 && sp.IndexOf("d") == -1)
            {
                s += "    --> No Permissions for Tables (sp=" + sp + ")\n";
                SAS.sp.s = false;
            }
            //----------------------------------------------

            return s;
        }




        /// <summary>
        /// TODO - How to define Queue ????
        /// 
        /// Service SAS for Queue
        /// Same permissions for Policy and 'sr' (Service SAS) parameter 
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Show_sp_queue(string sp, string sv)
        {
            string s = "";
            string v = "Valid permissions for Queue: 'raup'";

            // found chars not supported by sp paramenter
            if (!Utils.ValidateString(sp, "raup") || sp.Length > 4)
                return andSetState("sp", false, s += "  --> Invalid Signed Permissions for Queue (Service SAS) (sp=" + sp + "). " + v + "\n");

            //----------------------------------------------
            s += "  Permissions for Queue (Service SAS)  ('???'=??):\n";
            if (sp.IndexOf("r") != -1)
                s += "    Read metadata and properties, including message count. Peek at messages.\n";

            if (sp.IndexOf("a") != -1)
                s += "    Add messages to the queue.\n";

            if (sp.IndexOf("u") != -1)
                s += "    Update messages in the queue. Note: Use the Process permission with Update so you can first get the message you want to update.\n";

            if (sp.IndexOf("p") != -1)
                s += "    Get and delete messages from the queue.\n";

            if (sp.IndexOf("r") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("u") == -1 && sp.IndexOf("p") == -1)
            {
                s += "    --> No Permissions for Queues (sp=" + sp + ")\n";
                SAS.sp.s = false;
            }
            //----------------------------------------------

            return s;
        }



        /// <summary>
        /// Format the output string for st and se parameters
        /// st (SignedStart) - optional
        /// se (SignedExpiry) - Required
        /// ---------------------------------------------
        /// TODO Service SAS - Before version 2012-02-12, a shared access signature not associated with a stored access policy could not have an active period that exceeded one hour.
        /// ---------------------------------------------
        /// Supported ISO 8601 formats include the following:
        ///   YYYY-MM-DD
        ///   YYYY-MM-DDThh:mmZ              
        ///   YYYY-MM-DDThh:mm:ssZ 
        ///   https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-an-Account-SAS?redirectedfrom=MSDN#specifying-the-signature-validity-interval
        /// </summary>
        /// <param name="st"></param>
        /// <param name="se"></param>
        /// <returns></returns>
        public static string Show_st_se(string st, string se)
        {
            string s = "'st','se' parameters (Signed Start & Signed End Date/Time):\n";

            DateTime testDate;
            DateTime seDate;
            DateTime stDate;

            if (se == "not found")  // se Required
                return andSetState("se", false, s += "  --> Required Signed Expiry Date/Time 'se' value is missing\n\n");

            if (se.Length == 0)  // se Required
                return andSetState("se", false, s += "  --> Empty required Signed Expiry Date/Time value (se=" + se + ")\n\n");

            // Invalid format on Expiry Datime (by lenght)
            if (se.Length != 20 && se.Length != 17 && se.Length != 10)
                return andSetState("se", false, s += "  --> Incorrect format on Signed Expiry Date/Time (se=" + se + ")\n\n");

            // Testing valid 'se' date
            try
            {
                seDate = Convert.ToDateTime(se).ToUniversalTime();        // Convert.ToDateTime(se) -> convert to local time - > need to convert again to in UTC time
            }
            catch(Exception ex)
            {
                return andSetState("se", false, s += "  --> Invalid End date (se=" + se + "): " + ex.Message + "\n\n");
            }

            if (seDate.CompareTo(DateTime.Now.ToUniversalTime()) < 0)      // Test ending date in UTC
                return andSetState("se", false, s += "  --> Already Expired - End date (se=" + seDate.ToString().Replace("/", "-") + ", current UTC time=" + DateTime.Now.ToUniversalTime().ToString().Replace("/", "-") + ", current local time=" + DateTime.Now.ToString().Replace("/", "-") + ")\n\n");



            //-------------------------------------------------------------------------------------
            // st tests
            //-------------------------------------------------------------------------------------
            if (st.Length == 0)  // se Optional
                return andSetState("st", false, s += "  --> Empty optional Signed Start Date/Time value (st=" + st + ")\n\n");

            string s2 = "";
            if (st != "not found")
            {
                // Invalid Start Datime format (by lenght), if provided
                if (st.Length != 20 && st.Length != 17 && st.Length != 10)
                    return andSetState("st", false, s += "  --> Incorrect format on Signed Start Date/Time (st=" + st + ")\n\n");

                // Testing valid 'st' date
                try
                {
                    stDate = Convert.ToDateTime(st).ToUniversalTime();        // Convert.ToDateTime(se) -> convert to local time - > need to convert again to in UTC time
                }
                catch (Exception ex)
                {
                    return andSetState("st", false, s += "  --> Invalid Start date (st=" + st + "): " + ex.Message + "\n\n");
                }

                // Test starting date in UTC
                if (stDate.CompareTo(DateTime.Now.ToUniversalTime()) > 0)      
                    return andSetState("st", false, s += "  --> SAS not valid yet, at the current date/time (st=" + stDate.ToString().Replace("/", "-") + ", current UTC time=" + DateTime.Now.ToUniversalTime().ToString().Replace("/", "-") + ", current local time=" + DateTime.Now.ToString().Replace("/", "-") + ")\n\n");

                // Test Starting date after Ending date
                if (stDate.CompareTo(seDate) > 0)
                    return andSetState("st", false, s += "  --> Signed Start date/time after Signed Expiry Date/Time (st=" + st + ", se=" + se + ")\n\n");


                // testing Length st (SignedStart) - optional
                try
                {
                    switch (st.Length)
                    {
                        case 20:    // YYYY-MM-DDThh:mm:ssZ 
                            testDate = DateTimeUtils.FromIso8601Date(st);   // testDate used only to firing the exception if incorrect format found
                            s2 = s + "  Valid from " + st;
                            break;
                        case 17:    // YYYY-MM-DDThh:mmZ 
                            testDate = Convert.ToDateTime(st);
                            s2 = s + "  Valid from " + st;
                            break;
                        case 10:    // YYYY-MM-DD
                            testDate = Convert.ToDateTime(st);
                            s2 = s + "  Valid from " + st;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    return andSetState("st", false, s += "  --> Invalid Start date (st=" + st + "): " + ex.Message + "\n\n");
                }
            }
            //-------------------------------------------------------------------------------------



            // testing Length se (SignedExpiry) - Required
            try
            {
                switch (se.Length)
                {
                    case 20:    // YYYY-MM-DDThh:mm:ssZ 
                        testDate = DateTimeUtils.FromIso8601Date(se);   // testDate used only to firing the exception if incorrect format found
                        break;
                    case 17:    // YYYY-MM-DDThh:mmZ 
                        testDate = Convert.ToDateTime(se);
                        break;
                    case 10:    // YYYY-MM-DD
                        testDate = Convert.ToDateTime(se);
                        break;
                }

                if (st == "not found")  // SignedStart not provided
                    s += "  Valid up to " + se + "\n";
                else                    // SignedStart provided and valid
                    s = s2 + " to " + se + "\n";
            }
            catch (Exception ex)
            {
                return andSetState("se", false, s += "  --> Invalid End date (se=" + se + "): " + ex.Message + "\n\n");
            }


            return andSetState("se", true, s + "\n");
        }




        /// <summary>
        /// sip: Allowed IP's (optional)
        /// sip=168.1.5.65 or sip=168.1.5.60-168.1.5.70
        /// Multiple IPs not supported
        /// </summary>
        /// <param name="sip"></param>
        /// <returns></returns>
        public static string Show_sip(string sip)
        {
            string s = "'sip' parameter (Signed IP):\n";

            if (sip == "not found")     return andSetState("sip", true, s + "  All client IP addresses allowed (sip optional not provided)\n\n");

            // sip empty
            if (sip.Length == 0)        return andSetState("sip", false, s += "  --> Empty optional Signed IP value (sip=" + sip + ")\n\n");

            // '-' found at the end
            int i = sip.IndexOf("-");
            int p = sip.IndexOf(".");
            if (i == sip.Length - 1 || p == sip.Length - 1 )  
                                        return andSetState("sip", false, s += "  --> Invalid IP address range - IP address could not end with '-' or '.'  (sip=" + sip + ")\n\n");

            // '-' found at the begin 
            if (i == 0 || p == 0)       return andSetState("sip", false, s += "  --> Invalid IP address range - IP address could not start with '-' or '.' (sip=" + sip + ")\n\n");

            // Two or more '-' found
            i = sip.IndexOf("--");
            i = sip.IndexOf("..");
            if (i != -1 || p != -1)    return andSetState("sip", false, s += "  --> Invalid IP address range - Two or more '-' or '.' found (sip=" + sip + ")\n\n");


            // -------- IP Bytes validation --------------------
            string s2 = Validate_Sip(sip);
            if (s2 != "")  // error found on IP
                return andSetState("sip", false, s + "  --> " + s2);


            //------------- Ok, IP validated -----------------
            if (toIP[0] == 0)      // toIP empty - not used
                return andSetState("sip", true, s + "  IP address: " + fromIP[0] + "." + fromIP[1] + "." + fromIP[2] + "." + fromIP[3] + "\n\n");
            else
                return andSetState("sip", true, s + "  IP address range from: " + fromIP[0] + "." + fromIP[1] + "." + fromIP[2] + "." + fromIP[3] + " to " + toIP[0] + "." + toIP[1] + "." + toIP[2] + "." + toIP[3] + " \n\n");
        }




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string Validate_Sip()
        { 
            string s = Validate_Sip(SAS.sip.v);
            if (s != "")  // error found on IP
                return andSetState("sip", false, s);

            return ""; // empty means IPs validated - no error found
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sip"></param>
        /// <returns></returns>
        public static string Validate_Sip(string sip)
        {
            string[] IP = { "", "" };                // Array with 2 IP's - string format
            int[,] IPbyte = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };    // Array with 2 IP's - byte format

            int i = sip.IndexOf("-");
            int j2 = 0;

            // Validate different IP's on sip
            if (i != -1)   // Range specified - sip=168.1.5.60-168.1.5.70
            {
                IP[0] = sip.Substring(0, i);
                IP[1] = sip.Substring(i + 1);
            }
            else           // only one ip specified - sip=168.1.5.65 
            {
                IP[0] = sip;
            }

            // Validate bytes on each IP address
            string strByte = "";
            string sip2 = "";
            for (int myIP = 0; myIP < IP.Length; myIP++)
            {
                if (!String.IsNullOrEmpty(IP[myIP]))
                {
                    sip2 = IP[myIP];
                    for (int myByte = 0; myByte < 4; myByte++)
                    {
                        if (myByte != 3) // last byte dont have "."
                        {
                            j2 = sip2.IndexOf(".");
                            if (j2 == -1)
                                return "Invalid IP address (sip=" + sip + ")\n\n";
                            strByte = sip2.Substring(0, j2);    // b - one byte from IP
                            sip2 = sip2.Substring(j2 + 1);      // remove the readed byte
                        }
                        else // last byte
                            strByte = sip2;    // b - last byte from IP

                        try
                        {
                            IPbyte[myIP, myByte] = Int32.Parse(strByte);
                            if (((myByte == 0 && IPbyte[myIP, myByte] == 0) || (myByte > 0 && IPbyte[myIP, myByte] < 0)) || IPbyte[myIP, myByte] > 255) // p=0 - 1st byte could not be 0
                                return "Invalid IP address (sip=" + sip + ")\n\n";
                        }
                        catch (Exception ex)
                        {
                            return "Invalid IP address (sip=" + sip + "): " + ex.Message + "\n\n";
                        }
                    }
                }
            }

            // Start IP greater than End IP
            if (!String.IsNullOrEmpty(IP[1]))
                if (IPbyte[0, 0] > IPbyte[1, 0])
                    return "Invalid IP address range - Start IP greater than End IP (sip=" + sip + ")\n\n";


            // v12.0.0.0_preview
            //------------------------------------------
            // copy IP Bytes to fromIP[]
            for (int bf = 0; bf < 4; bf++)
                fromIP[bf] = (byte)IPbyte[0, bf];

            // copy IP Bytes to toIP[] 
            for (int bf = 0; bf < 4; bf++)
                toIP[bf] = (byte)IPbyte[1, bf];
            //------------------------------------------

            return "";  // empty means IPs validated - no error found
        }




        /// <summary>
        /// Format the output string for sig parameter 
        /// Encripted signature - Required 
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-a-Service-SAS?redirectedfrom=MSDN#specifying-the-signature
        /// </summary>
        /// <param name="sig"></param>
        /// <returns></returns>
        public static string Show_sig(string sig)
        {
            string s = "'sig' parameter (Signature):\n";

            // no (required) sig provided
            if (sig == "not found")
                return s += "  --> 'sig' required but not provided\n\n";

            // sig empty
            if (sig.Length == 0)
                return s += "  --> Empty required Signature value (sig=" + sig + ")\n\n";

            // TODO - some other sig checks here

            return s += "  Encripted Signature sig=" + sig + "\n\n";
        }


        /// <summary>
        /// Fill the sv ComboBox and the API_Version with the values of the array passed
        /// </summary>
        /// <param name="comboBox1"></param>
        public static void PopulateComboBox_sv(ComboBox comboBox1, string comboBox_sr_txt, string TextBox_tn_txt)
        {
            comboBox1.Items.Clear();

            // Check Account SAS
            PopulateComboBox(comboBox1, comboBox_sr_txt, validSV_AccountSAS_ARR);

            // Check Service SAS 
            if (comboBox_sr_txt != "" || TextBox_tn_txt != "")
                PopulateComboBox(comboBox1, comboBox_sr_txt, validSV_ServiceSas_ARR_addon);
        }


        /// <summary>
        /// Fill an genetic ComboBox with the values of the array passed
        /// Control Service Versions supported on each type of SAS
        /// </summary>
        /// <param name="comboBox1"></param>
        public static void PopulateComboBox(ComboBox comboBox1, string sr, string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (sr == "bs")
                {
                    if (arr[i].CompareTo("2018-11-09") >= 0)        // Service SAS bs suported on Service version 2018-11-09 and later
                        comboBox1.Items.Add(arr[i]);
                }
                else
                    if(sr == "f" || sr == "s")
                    {
                        if (arr[i].CompareTo("2015-02-21") > 0)    // Service SAS File and Share suported on Service version 2015-02-21 and later
                            comboBox1.Items.Add(arr[i]);            // (from Storage Explorer, seems 2015-02-21 not suported - removing the '=')
                }
                    else
                        comboBox1.Items.Add(arr[i]);
            }
        }





        /// <summary>
        /// Set the state on StrParameter param, and return the string
        /// States: true - no error on parameter; false - Error found on paramenter
        /// Used on Show_XXX functions
        /// </summary>
        private static string andSetState(string param, bool state, string s)
        {
            Set_StateParameter(param, state);
            return s;
        }




        /// <summary>
        /// true / false ig the 'findStr' found on 'source'
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        private static bool Found(string source, string findStr)
        {
            if (source.IndexOf(findStr) != -1)
                return true;
            return false;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="state"></param>
        private static void Set_StateParameter(string param, bool state)
        {
            switch (param)
            {
                case "apiVersion":
                    SAS.apiVersion.s = state;
                    break;
                case "sv":
                    SAS.sv.s = state;
                    break;
                case "ss":
                    SAS.ss.s = state;
                    break;
                case "srt":
                    SAS.srt.s = state;
                    break;
                case "sp":
                    SAS.sp.s = state;
                    break;
                case "st":
                    SAS.st.s = state;
                    break;
                case "se":
                    SAS.se.s = state;
                    break;
                case "sip":
                    SAS.sip.s = state;
                    break;
                case "spr":
                    SAS.spr.s = state;
                    break;
                case "sr":
                    SAS.sr.s = state;
                    break;
                case "tn":
                    SAS.tn.s = state;
                    break;
            }
        }
        //--------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------


        public static void init_SASstruct() 
        {
            SAS.sharedAccessSignature = "";

            SAS.blobEndpoint = "";
            SAS.fileEndpoint = "";
            SAS.tableEndpoint = "";
            SAS.queueEndpoint = "";

            SAS.storageAccountName.v = "";  SAS.storageAccountName.s = true;

            SAS.containerName.v = "";       SAS.containerName.s = true;
            SAS.blobName.v = "";            SAS.blobName.s = true;
            SAS.shareName.v = "";           SAS.shareName.s = true;
            SAS.fileName.v = "";            SAS.fileName.s = true;
            SAS.queueName.v = "";           SAS.queueName.s = true;
            SAS.tableName.v = "";           SAS.tableName.s = true;

            //SAS.onlySASprovided = true;

            SAS.apiVersion.v = "";  SAS.apiVersion.s = true;
            SAS.sv.v = "";          SAS.sv.s = true;
            SAS.ss.v = "";          SAS.ss.s = true;
            SAS.srt.v = "";         SAS.srt.s = true;
            SAS.sp.v = "";          SAS.sp.s = true;
            SAS.se.v = "";          SAS.se.s = true;
            SAS.st.v = "";          SAS.st.s = true;
            SAS.sip.v = "";         SAS.sip.s = true;
            SAS.spr.v = "";         SAS.spr.s = true;
            SAS.sig = "";

            SAS.sr.v = "";          SAS.sr.s = true;
            SAS.tn.v = "";          SAS.tn.s = true;
            SAS.blobSnapshotName.v = ""; SAS.blobSnapshotName.s = true; // v12.0.0.0_preview
            SAS.spk = "";           // The bellow are disable by default on UI
            SAS.epk = "";
            SAS.srk = "";
            SAS.erk = "";
            SAS.si = "";
        }

    }
}
