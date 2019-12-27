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
        public static string[] validSV_AccountSAS_ARR = { "2019-02-02", "2018-11-09", "2018-03-28", "2017-11-09", "2017-07-29", "2017-04-17", "2016-05-31", "2015-12-11", "2015-07-08", "2015-04-05" };

        /// <summary>
        /// Valid Storage Service Versions for Service SAS  >= 2012-02-12 
        /// 
        /// In Service Versions before 2012-02-12, the duration between signedstart and signedexpiry cannot exceed one hour
        /// </summary>
        public static string[] validSV_ServiceSas_ARR_addon = { "2015-02-21", "2014-02-14", "2013-08-15", "2012-02-12" };

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
        public static byte[] toIP = { 0, 0, 0, 0 };

        /// <summary>
        /// Struct to save all individual values from analized SAS, used to regenerate new SAS
        /// The struct easly allow access all the SAS parameters from any class
        /// </summary>
        public struct SasParameters
        {
            public StrParameter sharedAccessSignature; // complete SAS without the endpoints

            public string blobEndpoint;
            public string fileEndpoint;
            public string tableEndpoint;
            public string queueEndpoint;

            public StrParameter storageAccountName;   // storage Account Name, if provided on any Endpoint
            public StrParameter storageAccountKey;    // storage Account Key

            public StrParameter containerName;    // Container name, if provided on blobEndpoint
            public StrParameter blobName;         // Blob name, if provided on blobEndpoint
            public StrParameter shareName;        // Share name, if provided on fileEndpoint
            public StrParameter fileName;         // File name, if provided on fileEndpoint
            public StrParameter queueName;        // Queue name, if provided on queueEndpoint
            //public StrParameter tableName;        // Table name, if provided on tableEndpoint, and if is an account SAS

            public bool onlySASprovided;        // true if the endpoints not provided

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
            public StrParameter blobSnapshotName;  // Blob Snapshot Name if is Service SAS and using 
            public StrParameter blobSnapshotTime;  // Blob Snapshot Time - Service SAS only

            public string spk;          //   startpk;
            public string epk;          //   endpk;
            public string srk;          //   startrk;
            public string erk;          //   endrk;
            public StrParameter si;           // Policy Name

            // query parameters to override response headers (Blob and File services only)
            public string rscc;    // Cache-Control
            public string rscd;    // Content-Disposition
            public string rsce;    // Content-Encoding
            public string rscl;    // Content-Language
            public string rsct;    // Content-Type

            public DateTime stDateTime; // used to test the valid Date format
            public DateTime seDateTime;

            public string DebugInfo;
        };
        public static SasParameters SAS;




        /// <summary>
        /// Contruct generic parameter on URI Escaped format, if exists
        /// ex: &si=sco
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Get_Parameter(string param, string value)
        {
            if (String.IsNullOrEmpty(value))
                return "";

            return "&" + param + "=" + Uri.EscapeDataString(value);
        }



        /// <summary>
        /// Ordering permissions - racwudpl
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static string Order_permissions(string permissions)
        {
            string aux = "";

            if (permissions.IndexOf("r") != -1) aux += "r";
            if (permissions.IndexOf("a") != -1) aux += "a";
            if (permissions.IndexOf("c") != -1) aux += "c";
            if (permissions.IndexOf("w") != -1) aux += "w";
            if (permissions.IndexOf("u") != -1) aux += "u";
            if (permissions.IndexOf("d") != -1) aux += "d";
            if (permissions.IndexOf("p") != -1) aux += "p";
            if (permissions.IndexOf("l") != -1) aux += "l";

            return aux;
        }




        /// <summary>
        /// Validate and save SAS parameter values on SAS struct
        /// Show SAS parameters results on BoxAuthResults
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SAS_Validate(string InputBoxSASvalue, TextBox BoxAuthResults)
        {
            // clear the left "BoxAuthResults"
            BoxAuthResults.Text = "";

            // initialization of SAS Struct 
            init_SASstruct();

            // Validate and save SAS parameter values on SAS struct
            // return false if SAS String is invalid, and not show any results on left "BoxAuthResults"
            if (!SAS_GetParameters(InputBoxSASvalue)) return;

            // Show SAS parameters results on "BoxAuthResults"
            SAS_ShowResults(BoxAuthResults);
        }




        /// <summary>
        /// Test parameters on provided Account/Service SAS
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-an-Account-SAS?redirectedfrom=MSDN#constructing-the-account-sas-uri
        /// 
        /// return false if SAS String is invalid, and not show any results on left TextBox
        /// </summary>
        /// <param name="InputBoxSAS"></param>
        private bool SAS_GetParameters(string InputBoxSASvalue)
        {
            // testing other parameters: (SharedAccessSignature, ;SharedAccessSignature?, BlobEndpoint=, FileEndpoint=, TableEndpoint=, QueueEndpoint= )
            // return if SAS String is invalid
            if (!Check_SASString(InputBoxSASvalue))
                return false;

            //---------------------------------------------------------------------
            // Retrieving values from Service endpoints
            //---------------------------------------------------------------------
            SAS.blobEndpoint = Get_SASValue(InputBoxSASvalue, "BlobEndpoint=", ";");
            SAS.fileEndpoint = Get_SASValue(InputBoxSASvalue, "FileEndpoint=", ";");
            SAS.tableEndpoint = Get_SASValue(InputBoxSASvalue, "TableEndpoint=", ";");
            SAS.queueEndpoint = Get_SASValue(InputBoxSASvalue, "QueueEndpoint=", ";");

            // If Endpoints not found, check if only URI (starting with http(s)://) was provided on InputBoxSAS
            // This is the case of the Service SAS
            Get_Endpoint_FromServiceSASURI(InputBoxSASvalue);

            // If endpoint Found
            // Search on endpoints strings, for values to SAS struct: 
            // containerName, blobName, shareName, fileName, queueName, tableName
            Get_ResourceNames_FromEndpoints();

            // Try to Get the Storage Account Name from any Endpoint, if provided
            Get_StorageAccountName_FromEndpoint();


            //---------------------------------------------------------------------
            // Retrieving Shared Access Signature
            //---------------------------------------------------------------------
            // Connection string was provided
            SAS.onlySASprovided = false;
            SAS.sharedAccessSignature.v = Get_SASValue(InputBoxSASvalue, "SharedAccessSignature=", ";");

            // only SAS token was provided
            if (SAS.sharedAccessSignature.v == "not found")
            {
                SAS.sharedAccessSignature.v = Get_SASValue(InputBoxSASvalue, "?");

                if (Found(SAS.sharedAccessSignature.v, "not found="))
                {
                    SAS.sharedAccessSignature.s = false;
                    return Utils.WithMessage("Missing the '?' at the begin of SAS token, or the term 'SharedAccessSignature=' on a Connection String", "Invalid SAS");
                }
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
            SAS.sv.v = Get_SASValue(SAS.sharedAccessSignature.v, "sv=", "&");                  // Required (>=2015-04-05)
            SAS.ss.v = Get_SASValue(SAS.sharedAccessSignature.v, "ss=", "&");
            SAS.srt.v = Get_SASValue(SAS.sharedAccessSignature.v, "srt=", "&");
            SAS.sp.v = Get_SASValue(SAS.sharedAccessSignature.v, "sp=", "&");
            SAS.se.v = Get_SASValue(SAS.sharedAccessSignature.v, "se=", "&");
            SAS.st.v = Get_SASValue(SAS.sharedAccessSignature.v, "st=", "&");
            SAS.sip.v = Get_SASValue(SAS.sharedAccessSignature.v, "sip=", "&");
            SAS.spr.v = Get_SASValue(SAS.sharedAccessSignature.v, "spr=", "&");

            SAS.sig = Get_SASValue(SAS.sharedAccessSignature.v, "sig=", "&");



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
            SAS.sr.v = Get_SASValue(SAS.sharedAccessSignature.v, "sr=", "&");


            //---------------------------------------------------------------------
            // tn: tablename - Required. 
            // The name of the table to share.
            //---------------------------------------------------------------------
            SAS.tn.v = Get_SASValue(SAS.sharedAccessSignature.v, "tn=", "&");


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
            SAS.spk = Get_SASValue(SAS.sharedAccessSignature.v, "spk=", "&");
            SAS.epk = Get_SASValue(SAS.sharedAccessSignature.v, "epk=", "&");
            SAS.srk = Get_SASValue(SAS.sharedAccessSignature.v, "srk=", "&");
            SAS.erk = Get_SASValue(SAS.sharedAccessSignature.v, "erk=", "&");


            //---------------------------------------------------------------------
            // override response headers
            // Service SAS only - (Blob and File services only)
            // https://docs.microsoft.com/en-us/rest/api/storageservices/create-service-sas#specifying-query-parameters-to-override-response-headers-blob-and-file-services-only
            //---------------------------------------------------------------------
            SAS.rscc = Get_SASValue(SAS.sharedAccessSignature.v, "rscc=", "&");
            SAS.rscd = Get_SASValue(SAS.sharedAccessSignature.v, "rscd=", "&");
            SAS.rsce = Get_SASValue(SAS.sharedAccessSignature.v, "rsce=", "&");
            SAS.rscl = Get_SASValue(SAS.sharedAccessSignature.v, "rscl=", "&");
            SAS.rsct = Get_SASValue(SAS.sharedAccessSignature.v, "rsct=", "&");

            //---------------------------------------------------------------------
            // si: signedidentifier - Optional.
            // A unique value up to 64 characters in length that correlates to an access policy specified for the container, queue, or table.
            //---------------------------------------------------------------------
            SAS.si.v = Get_SASValue(SAS.sharedAccessSignature.v, "si=", "&");

            return true;
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

            BoxAuthResults.Text += "SAS Token (Unescaped): \n?" + SAS.sharedAccessSignature.v + "\n";
            BoxAuthResults.Text += "\n";

            // Verify if Services on URI are the same as on Service SAS permissions (Service SAS) 
            BoxAuthResults.Text += Show_Services();

            // ss: {bqtf} - Required 
            // Service endpoints: (optional)
            BoxAuthResults.Text += Show_ss(SAS.ss.v, SAS.blobEndpoint, SAS.fileEndpoint, SAS.tableEndpoint, SAS.queueEndpoint, SAS.spr.v, SAS.onlySASprovided, SAS.srt.v);

            // spr: {https,http} - Optional (default both)
            BoxAuthResults.Text += Show_spr(SAS.spr.v, SAS.sr.v, SAS.tn.v, SAS.sv.v);

            // sv: Service Versions - Required (>=2015-04-05) - TODO more recent versions
            BoxAuthResults.Text += Show_sv(SAS.sv.v, SAS.sr.v, SAS.srt.v, SAS.tn.v);

            //---------------------------------------------------------------------
            // srt: Signed Resource Types {s,c,o} {Service, Container, Object} - Required
            // Specific for Service SAS: https://docs.microsoft.com/en-us/rest/api/storageservices/constructing-a-service-sas
            // sr: signedresource - Required { b,c }   // blob, container - for blobs
            //                               { s,f }   // share, file     - for files, version 2015-02-21 and later
            //                               {bs}      // blob snapshot,               version 2018-11-09 and later
            // tn: tablename - Required - The name of the table to share.
            // No sr, srt or tn provided means queue service
            //---------------------------------------------------------------------
            string s = SAS_ValidateParam.Sr_srt_tn(SAS.sr.v, SAS.srt.v, SAS.tn.v);
            switch (s)
            {
                // No srt, sr or tn provided - Service Queue SAS
                //---------------------------------------------------------------------
                case "":
                    BoxAuthResults.Text += Show_Queue(SAS.queueName.v);
                    break;

                // More than one srt, sr or tn provided - Only one Required
                //---------------------------------------------------------------------
                case "all":
                    BoxAuthResults.Text += Show_All_srt_sr_tn("all");
                    break;

                //---------------- Account SAS ----------------------------------------
                // srt: Signed Resource Types {s,c,o} {Service, Container, Object} - Required
                //---------------------------------------------------------------------
                case "srt":
                    BoxAuthResults.Text += Show_srt(SAS.srt.v);
                    break;

                //---------------- Service SAS ----------------------------------------
                // sr: signedresource - Required { b,c }   // blob, container - for blobs
                //                               { s,f }   // share, file     - for files, version 2015-02-21 and later
                //                               {bs}      // blob snapshot,               version 2018-11-09 and later
                //---------------------------------------------------------------------
                case "sr":
                    BoxAuthResults.Text += Show_sr(SAS.sr.v, SAS.sv.v);
                    break;

                //---------------- Service SAS ----------------------------------------
                // tn: tablename - Required - The name of the table to share.
                //---------------------------------------------------------------------
                case "tn":
                    BoxAuthResults.Text += Show_tn(SAS.tn.v);
                    break;
            }
            //---------------------------------------------------------------------

            // 'si' Policy may define Permissions and Start and Expiry datetime
            // If Policy provided, no validate Permissions and Start and Expiry datetime
            if (String.IsNullOrEmpty(SAS.si.v))
            {
                // sp: SignedPermissions {r,w,d,l,a,c,u,p}  - Required
                // This field must be omitted if it has been specified in an associated stored access policy.  - TODO (Service SAS)
                BoxAuthResults.Text += Show_sp(SAS.sp.v, SAS.srt.v, SAS.sr.v, SAS.tn.v, SAS.sv.v);

                // st (SignedStart) - optional
                // se (SignedExpiry) - Required
                BoxAuthResults.Text += Show_st_se(SAS.st.v, SAS.se.v);
            }

            // sip: Allowed IP's (optional)
            BoxAuthResults.Text += Show_sip(SAS.sip.v);

            // sig: Enchripted Signature - Required
            BoxAuthResults.Text += Show_sig(SAS.sig);



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
            BoxAuthResults.Text += Show_pk_rk(SAS.tn.v, SAS.spk, SAS.epk, SAS.srk, SAS.erk);



            //---------------------------------------------------------------------
            // override response headers (Blob and File services only)
            // Service SAS only - (Blob and File services only)
            // https://docs.microsoft.com/en-us/rest/api/storageservices/create-service-sas#specifying-query-parameters-to-override-response-headers-blob-and-file-services-only
            //---------------------------------------------------------------------
            BoxAuthResults.Text += Show_rscX(SAS.rscc, SAS.rscd, SAS.rsce, SAS.rscl, SAS.rsct);



            //---------------------------------------------------------------------
            // si: signedidentifier - Optional.
            // A unique value up to 64 characters in length that correlates to an access policy specified for the container, queue, or table.
            //---------------------------------------------------------------------
            BoxAuthResults.Text += Show_si(SAS.si.v, SAS.srt.v);
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
        private bool Check_SASString(string InputBoxSASvalue)
        {
            // Assuming some error will be found
            SAS.sharedAccessSignature.s = false;

            // Check for empty string
            if (InputBoxSASvalue == "")
                return Utils.WithMessage("Empty SAS or Connection String", "Invalid SAS");

            // Check for some spaces or newlines on the string
            string space = Get_SpaceNL(InputBoxSASvalue.Trim());
            if (space != "")
                return Utils.WithMessage("Invalid string - " + space + " found on SAS", "Invalid SAS");

            // test 'SharedAccessSignature' (connection string) or '?' SAS
            if (InputBoxSASvalue.IndexOf("?") != -1 && Found(InputBoxSASvalue, "SharedAccessSignature"))
                return Utils.WithMessage("Only one 'SharedAccessSignature' or '?' should be provided on SAS", "Invalid SAS");

            // test '??' found on SAS
            if (InputBoxSASvalue.IndexOf("??") != -1)
                return Utils.WithMessage("Only one '?' should be provided on SAS", "Invalid SAS");

            // no 'SharedAccessSignature=' or '?' found
            if (!Found(InputBoxSASvalue, "SharedAccessSignature=") && InputBoxSASvalue.IndexOf("?") == -1)
                return Utils.WithMessage("Missing 'SharedAccessSignature=' (for connection strings)\n or '?' (for SAS token), on the provided SAS", "Invalid SAS");

            //  SharedAccessSignature?= found
            if (Found(InputBoxSASvalue, "SharedAccessSignature?=") || Found(InputBoxSASvalue, "SharedAccessSignature=?"))
                return Utils.WithMessage("Invalid characters found after 'SharedAccessSignature',\n on the provided SAS", "Invalid SAS");

            // Connection string found, but no Endpoints found
            if (Found(InputBoxSASvalue, "SharedAccessSignature=") && !EndpointsExists(InputBoxSASvalue))
                return Utils.WithMessage("Connection string found, but no Endpoints found.\nWithout endpoints, the SAS token should start with '?' instead of using 'SharedAccessSignature'", "Invalid SAS");

            //
            if (InputBoxSASvalue.IndexOf("srt") == -1 && (InputBoxSASvalue.IndexOf("sr") != -1 || InputBoxSASvalue.IndexOf("tn") != -1)) // sr or tn (Service SAS) found
            {
                // Endpoints found
                if (!EndpointFound(InputBoxSASvalue, "BlobEndpoint")) return false;
                if (!EndpointFound(InputBoxSASvalue, "FileEndpoint")) return false;
                if (!EndpointFound(InputBoxSASvalue, "TableEndpoint")) return false;
                if (!EndpointFound(InputBoxSASvalue, "QueueEndpoint")) return false;
            }

            //
            if (InputBoxSASvalue.IndexOf("?") != -1 && EndpointsExists(InputBoxSASvalue))
                return Utils.WithMessage("Endpoints should be removed from the SAS token.\nOnly allowed on Connection string", "Invalid SAS");

            // No error found - restoring the .s value to true, before return
            SAS.sharedAccessSignature.s = true;
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
        private void Get_Endpoint_FromServiceSASURI(string inputBoxSAS)
        {
            if (SAS.blobEndpoint == "not found" && SAS.fileEndpoint == "not found" && SAS.tableEndpoint == "not found" && SAS.queueEndpoint == "not found")
            {
                string newEndpoint = "";
                string service = "";

                int i = inputBoxSAS.IndexOf("?");
                if (i != -1)
                    newEndpoint = inputBoxSAS.Substring(0, i);

                service = Get_SASValue(newEndpoint, ".", ".core");

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
        /// Search on endpoints strings, for values to:
        ///    SAS.containerName 
        ///    SAS.blobName 
        ///    SAS.shareName 
        ///    SAS.fileName 
        ///    SAS.queueName 
        ///    SAS.tableName 
        ///    
        ///    Some of endpoints (Blob, File, Table, Queue) can be "not found"
        /// </summary>
        private void Get_ResourceNames_FromEndpoints()
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
                    string s2 = Get_SASValue(SAS.blobEndpoint, ".blob.core.windows.net/" + SAS.containerName.v + "/");
                    if (s2 != "not found")
                    {
                        // remove subfolders
                        int i = s2.LastIndexOf('/');
                        if (i == s2.Length - 1 && s2 != "")    // blob name ends with '/'
                        {
                            SAS.blobName.v = "<invalid blob on endpoint>";
                            SAS.blobName.s = false;
                        }
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
                    string s2 = Get_SASValue(SAS.fileEndpoint, ".file.core.windows.net/" + SAS.shareName.v + "/");
                    if (s2 != "not found")
                    {
                        // remove subfolders
                        int i = s2.LastIndexOf('/');
                        if (i == s2.Length - 1 && s2 != "")    // file name ends with '/'
                        {
                            SAS.fileName.v = "<invalid file on endpoint>";
                            SAS.fileName.s = false;
                        }
                        else
                            if (i > 0)                          // sub folders exist - removing subfolders
                            SAS.fileName.v = s2.Substring(i + 1);
                        else                                // only file name found after the share name
                            SAS.fileName.v = s2;
                    }
                }
            }

            // Table name if provided on tn paramenter and not on the URL
            /*
            if (SAS.tableEndpoint != "not found")
            {
                // Format, if exists: https://<storage>.table.core.windows.net/
                string s = Get_SASValue(SAS.tableEndpoint, ".table.core.windows.net/");
                int start = s.LastIndexOf("/") + 1; // start of the table name
                //if (s != "not found" && start > 0)
                //    SAS.tn.v = s.Substring(start, s.Length - start);
                SAS.tn.v = s.TrimStart().Replace("/", String.Empty);          // remove '/' at the end, if exists
            }
            */

            if (SAS.queueEndpoint != "not found")
            {
                // Format, if exists: https://<storage>.queue.core.windows.net/queue
                string s = Get_SASValue(SAS.queueEndpoint, ".queue.core.windows.net/");
                int start = s.LastIndexOf("/") + 1; // start of the Queue name
                SAS.queueName.v = s.TrimStart().Replace("/", String.Empty);          // remove '/' at the end, if exists
                //if(SAS.queueName.v == "")
                //    SAS.queueName.s = false;
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
            if (SAS.sr.v != "not found" || SAS.tn.v != "not found") // Blob, Container, Share, File, Table
            {
                string service = "";
                if (SAS.tn.v != "not found")
                    service = "Table";
                else
                    switch (SAS.sr.v)
                    {
                        case "b":
                            service = "Blob";
                            break;
                        case "c":
                            service = "Container";
                            break;
                        case "s":
                            service = "Share";
                            break;
                        case "f":
                            service = "File";
                            break;
                        case "bs":
                            service = "Blob Snapshot";
                            break;
                    }
                return service + " Service SAS detected\n---------------------------- -\n";
            }
            else
                return "Queue Service SAS detected\n-----------------------------\n";         // Queue
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
                    return "  --> Table name 'tn' provided on SAS parameter, but a different or no URI was provided to the service.\n\n";

                //if (!(SAS.tableEndpoint.EndsWith(".net") || SAS.tableEndpoint.EndsWith(".net/")))
                //    return "  --> Incorrect Table Endpoint format.\nTable Endpoint should not mention the table name on URI.\n\n";
            }

            // Blob Service
            if (SAS.sr.v == "b")
            {
                if (SAS.blobEndpoint == "not found")
                    return "  --> Blob was provided on 'sr' SAS parameter, but a different or no URI was provided to the service.\n\n";
                if (SAS.blobName.v == "")
                    return "  --> Blob was provided on 'sr' SAS parameter, but no Blob name was provided on URI.\n\n";
            }
            if (SAS.sr.v == "c")
            {
                if (SAS.blobEndpoint == "not found")
                    return "  --> Container was provided on 'sr' SAS parameter, but a different or no URI was provided to the service.\n\n";
                if (SAS.containerName.v == "")
                    return "  --> Container was provided on 'sr' SAS parameter, but no Container name was provided on URI.\n\n";
                if (SAS.blobName.v != "")
                    return "  --> Container was provided on 'sr' SAS parameter, but the Blob name should be removed from URI.\n\n";
            }
            if (SAS.sr.v == "bs")
            {
                if (SAS.blobEndpoint == "not found")
                    return "  --> Blob Snapshot was provided on 'sr' SAS parameter, but a different or no URI was provided to the service.\n\n";
                if (SAS.blobName.v == "")
                    return "  --> Blob Snapshot was provided on 'sr' SAS parameter, but no Snapshot name was provided on URI.\n\n";
            }


            // File Service
            if (SAS.sr.v == "f")
            {
                if (SAS.fileEndpoint == "not found")
                    return "  --> File was provided on 'sr' SAS parameter, but a different or no URI was provided to the service.\n\n";
                if (SAS.fileName.v == "")
                    return "  --> File was provided on 'sr' SAS parameter, but no File name was provided on URI.\n\n";
            }
            if (SAS.sr.v == "s")
            {
                if (SAS.fileEndpoint == "not found")
                    return "  --> Share was provided on 'sr' SAS parameter, but a different or no URI was provided to the service.\n\n";
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
        /// Format the output string for ss parameter and Endpoints provided on connection string
        /// b,f,t,q
        /// </summary>
        public static string Show_ss(string ss, string blobEndpoint, string fileEndpoint, string tableEndpoint, string queueEndpoint, string spr, bool onlySASprovided, string srt)
        {
            string s = "'ss' parameter (Signed Services):\n";

            string res = SAS_ValidateParam.Ss(ss, spr, srt);

            if (SAS.ss.s == false)           // error on value                    
                return s + "  --> " + res + "\n\n";

            if (res == "noSrt")
                return ""; // silent return ("not found")


            // value validated - add the endpoint
            //------------------------------------------------------------------
            if (onlySASprovided)     // only SAS token was provided
            {
                s += "  Blob Service " + (ss.IndexOf("b") == -1 ? "NOT " : "") + "allowed " + (blobEndpoint != "not found" ? SAS_ValidateParam.EndpointFormat(blobEndpoint, "Blob", spr) : "") + "\n";
                s += "  File Service " + (ss.IndexOf("f") == -1 ? "NOT " : "") + "allowed " + (fileEndpoint != "not found" ? SAS_ValidateParam.EndpointFormat(fileEndpoint, "File", spr) : "") + "\n";
                s += "  Table Service " + (ss.IndexOf("t") == -1 ? "NOT " : "") + "allowed " + (tableEndpoint != "not found" ? SAS_ValidateParam.EndpointFormat(tableEndpoint, "Table", spr) : "") + "\n";
                s += "  Queue Service " + (ss.IndexOf("q") == -1 ? "NOT " : "") + "allowed " + (queueEndpoint != "not found" ? SAS_ValidateParam.EndpointFormat(queueEndpoint, "Queue", spr) : "") + "\n";
            }
            else                    // Connection string was provided
            {
                s += "  Blob Service " + (ss.IndexOf("b") == -1 ? "NOT allowed" : "allowed " + (blobEndpoint == "not found" ? "--> but Blob Endpoint NOT provided." : SAS_ValidateParam.EndpointFormat(blobEndpoint, "Blob", spr))) + "\n";
                s += "  File Service " + (ss.IndexOf("f") == -1 ? "NOT allowed" : "allowed " + (fileEndpoint == "not found" ? "--> but File Endpoint NOT provided." : SAS_ValidateParam.EndpointFormat(fileEndpoint, "File", spr))) + "\n";
                s += "  Table Service " + (ss.IndexOf("t") == -1 ? "NOT allowed" : "allowed " + (tableEndpoint == "not found" ? "--> but Table Endpoint NOT provided." : SAS_ValidateParam.EndpointFormat(tableEndpoint, "Table", spr))) + "\n";
                s += "  Queue Service " + (ss.IndexOf("q") == -1 ? "NOT allowed" : "allowed " + (queueEndpoint == "not found" ? "--> but Queue Endpoint NOT provided." : SAS_ValidateParam.EndpointFormat(queueEndpoint, "Queue", spr))) + "\n";
            }

            return andSetState("ss", true, s + "\n");
        }






        /// <summary>
        /// Format the output string for spr parameter and Endpoints provided on connection string
        /// Note that HTTP only is not a permitted value.
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/constructing-an-account-sas#constructing-the-account-sas-uri
        /// </summary>
        public static string Show_spr(string spr, string sr, string tn, string sv)
        {
            string s = "'spr' parameter (Allowed Protocols):\n";

            string res = SAS_ValidateParam.Spr(spr, sr, tn, sv);

            if (SAS.spr.s == true)           // value validated     
                return s + "  " + res + "\n\n";
            else                            // error on value 
                return s + "  --> " + res + "\n\n";
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

            string res = SAS_ValidateParam.Sv(sv, sr, srt, tn);

            if (SAS.sv.s == true)           // value validated    
                return s + "  " + res + "\n\n";
            else                            // error on value 
                return s + "  --> " + res + "\n\n";
        }



        /// <summary>
        /// Format the output string in case of more than one provided
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Show_All_srt_sr_tn(string str)
        {
            string s = "'srt', 'sr', 'tn' parameters (Account / Service SAS):\n";

            string res = SAS_ValidateParam.Srt_sr_tn(str);

            if (SAS.srt.s == true && SAS.sr.s == true && SAS.tn.s == true)            // value validated
                return s + "  " + res + "\n\n";
            else                            // error on value 
                return s + "  --> " + res + "\n\n";
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
                return andSetState("srt", false, s + "  --> 'srt' Required but not provided\n\n");

            // srt empty
            if (srt.Length == 0)
                return andSetState("srt", false, s + "  --> Empty Signed Resource Type value (srt=" + srt + ")\n\n");

            // found chars not supported by srt paramenter
            if (!Utils.ValidateString(srt, "sco"))
                return andSetState("srt", false, s + "  --> Invalid Signed Resource Types  (srt=" + srt + ")\n\n");

            // srt OK
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
                return andSetState("srt", false, s + "  --> Invalid Signed Resource Types  (srt=" + srt + ")\n\n");

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

            string res = SAS_ValidateParam.Sr(sr, sv);

            if (SAS.sr.s == true)            // value validated
                return s + "  " + res + "\n\n";
            else                            // error on value 
                return s + "  --> " + res + "\n\n";
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

            string res = SAS_ValidateParam.Tn(tn);

            if (SAS.tn.s == true)           // value validated
                return s + "  " + res + "\n\n";
            else                            // error on value 
                return s + "  --> " + res + "\n\n";
        }



        /// <summary>
        /// Format the output string in case of none queue parameter provided on URI, - Service Queue
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static string Show_Queue(string queue)
        {
            string s = "Queue Service SAS (no 'srt', 'sr', 'tn'):\n";

            string res = SAS_ValidateParam.Queue(queue);

            if (SAS.tn.s == true)           // value validated
                return s + "  " + res + "\n\n";
            else                            // error on value 
                return s + "  --> " + res + "\n\n";
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
                if (startpk.Length == 0)
                    return s += "  --> Empty Start Partition Key value (startpk=" + startpk + ")\n\n";
                if (endpk.Length == 0)
                    return s += "  --> Empty End Partition Key value (endpk=" + endpk + ")\n\n";

                s += "  Allowing Table access from Partition '" + startpk + "' to '" + endpk + "'\n";
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



        public static string Show_rscX(string rscc, string rscd, string rsce, string rscl, string rsct)
        {
            string s = "";

            if (rscc != "not found") s += "  Cache-Control: " + rscc;
            if (rscd != "not found") s += "  Content-Disposition: " + rscd;
            if (rsce != "not found") s += "  Content-Encoding: " + rsce;
            if (rscl != "not found") s += "  Content-Language: " + rscl;
            if (rsct != "not found") s += "  Content-Type: " + rsct;

            if (s != "")
                s += "'rscc', 'rscd', 'rsce', 'rscl', 'rsct' parameters - Override response headers (Blob and File services only):\n" + s;

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

            string res = SAS_ValidateParam.Si(si, srt);

            if (SAS.si.s == false)           // error on value                    
                return s + "  --> " + res + "\n\n";

            if (res == "")
                return ""; // silent return ("not found")

            // value validated
            return s + "  " + res + "\n\n";
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

            string res = SAS_ValidateParam.Sp(sp, srt, sr, tn, sv);

            if (SAS.sp.s == false)           // error on value                    
                return s + "  --> " + res + "\n\n";

            // value validated
            return s + "  " + res + "\n";
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

            string res = SAS_ValidateParam.St_se(st, se);

            if (SAS.st.s == false || SAS.se.s == false)           // error on value                    
                return s + "  --> " + res + "\n\n";

            // value validated
            return s + "  " + res + "\n\n";
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

            string res = SAS_ValidateParam.Sip(sip);

            if (SAS.sip.s == false)           // error on value                    
                return s + "  --> " + res + "\n\n";

            // value validated
            return s + "  " + res + "\n\n";
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
        /// Try to Get the Storage Account Name from any Endpoint, if provided
        /// </summary>
        private void Get_StorageAccountName_FromEndpoint()
        {
            SAS.storageAccountName.v = Get_SASValue(SAS.blobEndpoint, "://", ".");

            if (SAS.storageAccountName.v == "" || SAS.storageAccountName.v == "not found")
                SAS.storageAccountName.v = Get_SASValue(SAS.fileEndpoint, "://", ".");

            if (SAS.storageAccountName.v == "" || SAS.storageAccountName.v == "not found")
                SAS.storageAccountName.v = Get_SASValue(SAS.tableEndpoint, "://", ".");

            if (SAS.storageAccountName.v == "" || SAS.storageAccountName.v == "not found")
                SAS.storageAccountName.v = Get_SASValue(SAS.queueEndpoint, "://", ".");
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
        /// return "" if ok
        /// </summary>
        public static string Get_SpaceNL(string source)
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
        /// Return true if the str is found on ComboBox items list
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool ComboItemFound(ComboBox comboBox, string str)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
                if (str == comboBox.Items[i].ToString())
                    return true;
            return false;
        }



        /// <summary>
        /// Fill the sv ComboBox and the API_Version with the values of the array passed
        /// </summary>
        /// <param name="comboBox1"></param>
        public static void PopulateComboBox_sv(ComboBox comboBox1, string comboBox_sr_txt, string TextBox_tn_txt)
        {
            string s = comboBox1.Text;
            comboBox1.Items.Clear();

            // Check Account SAS
            PopulateComboBox(comboBox1, comboBox_sr_txt, validSV_AccountSAS_ARR);

            // Check Service SAS 
            if (comboBox_sr_txt != "" || TextBox_tn_txt != "")
                PopulateComboBox(comboBox1, comboBox_sr_txt, validSV_ServiceSas_ARR_addon);


            // Validate the old value of the combo box are in the new list, otherwise clean the value
            if (ComboItemFound(comboBox1, s))
                comboBox1.Text = s;
            else
                comboBox1.Text = "";
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
                    if (sr == "f" || sr == "s")
                {
                    if (arr[i].CompareTo("2015-02-21") > 0)    // Service SAS File and Share suported on Service version 2015-02-21 and later
                        comboBox1.Items.Add(arr[i]);            // (from Storage Explorer, seems 2015-02-21 not suported - removing the '=')
                }
                else
                    comboBox1.Items.Add(arr[i]);
            }
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
        /// Set the state on StrParameter param, and return the string
        /// States: true - no error on parameter; false - Error found on paramenter
        /// Used on Show_XXX functions
        /// </summary>
        public static string andSetState(string param, bool state, string s)
        {
            Set_StateParameter(param, state);
            return s;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="state"></param>
        public static void Set_StateParameter(string param, bool state)
        {
            switch (param)
            {
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
                case "si":
                    SAS.si.s = state;
                    break;
                case "queue":
                    SAS.queueName.s = state;
                    break;
            }
        }
        //--------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------


        public static void init_SASstruct()
        {
            SAS.sharedAccessSignature.v = ""; SAS.sharedAccessSignature.s = true;

            SAS.blobEndpoint = "";
            SAS.fileEndpoint = "";
            SAS.tableEndpoint = "";
            SAS.queueEndpoint = "";

            SAS.storageAccountName.v = ""; SAS.storageAccountName.s = true;
            SAS.storageAccountKey.v = ""; SAS.storageAccountKey.s = true;

            SAS.containerName.v = ""; SAS.containerName.s = true;
            SAS.blobName.v = ""; SAS.blobName.s = true;
            SAS.shareName.v = ""; SAS.shareName.s = true;
            SAS.fileName.v = ""; SAS.fileName.s = true;
            SAS.queueName.v = ""; SAS.queueName.s = true;
            // table name in SAS.tn,v

            //SAS.onlySASprovided = true;

            SAS.sv.v = ""; SAS.sv.s = true;
            SAS.ss.v = ""; SAS.ss.s = true;
            SAS.srt.v = ""; SAS.srt.s = true;
            SAS.sp.v = ""; SAS.sp.s = true;
            SAS.se.v = ""; SAS.se.s = true;
            SAS.st.v = ""; SAS.st.s = true;
            SAS.sip.v = ""; SAS.sip.s = true;
            SAS.spr.v = ""; SAS.spr.s = true;
            SAS.sig = "";

            SAS.sr.v = ""; SAS.sr.s = true;
            SAS.tn.v = ""; SAS.tn.s = true;
            SAS.blobSnapshotName.v = ""; SAS.blobSnapshotName.s = true;
            SAS.blobSnapshotTime.v = ""; SAS.blobSnapshotTime.s = true;      // Service SAS only
            SAS.spk = "";           // The bellow are disable by default on UI
            SAS.epk = "";
            SAS.srk = "";
            SAS.erk = "";
            SAS.si.v = ""; SAS.si.s = true;

            SAS.rscc = "";
            SAS.rscd = "";
            SAS.rsce = "";
            SAS.rscl = "";
            SAS.rsct = "";
        }

    }
}
