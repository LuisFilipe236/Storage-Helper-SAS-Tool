using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Controls;


//------------------------------------------------
// Grant limited access to Azure Storage resources using shared access signatures (SAS) - Next Steps
// https://docs.microsoft.com/en-gb/azure/storage/common/storage-sas-overview
//------------------------------------------------
// Service SAS examples:
// https://docs.microsoft.com/en-us/rest/api/storageservices/service-sas-examples
//------------------------------------------------

/*
 //------------------------------------------------
 Main()
 {
        // Test method - OK
        // Console.WriteLine("Generated SAS - tested OK: " + RestAPI.RestAPI.GetContainerSASToken_OK("container", "blob.txt");

        // OK - IP/Protocol/Service Version
        // Console.WriteLine("Generated Account SAS: " + Account.GenerateAccountSas(Account.serviceVersion, Account.startTime, Account.expiryTime, true));

        // OK blob
        Console.WriteLine("Generated Service SAS: " + Service.GenerateServiceSas(Account.serviceVersion, Account.startTime, Account.expiryTime, true));
        Console.WriteLine("--- FINISH ---");

        Console.ReadLine();
 }
 //------------------------------------------------
 */

namespace Storage_Helper_SAS_Tool
{

    class RestAPI
    {

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

    }





    //-------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------
    //          Service SAS
    //-------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------
    class Service : RestAPI
    {


        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Container Snapshot Service SAS 
        //---------------------------------------------------------------------------------------------------------------------
        public static bool Regenerate_ServiceSAS_Container(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = "?" + Manual_ServiceSas(VersionControl, debug);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Container:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Container URI:\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (list blobs):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "?restype=container&comp=list" + Uri.UnescapeDataString(sas).Replace("?", "&") + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "?restype=container&comp=list" + sas.Replace("?", "&") + "\n\n";

            return true;
        }


        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Blob Service SAS 
        //---------------------------------------------------------------------------------------------------------------------
        public static bool Regenerate_ServiceSAS_Blob(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = "?" + Manual_ServiceSas(VersionControl, debug);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Blob:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Blob URI:\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobName.v + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get blob):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobName.v + sas + "\n\n";

            return true;
        }



        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Blob Snapshot Service SAS 
        //---------------------------------------------------------------------------------------------------------------------
        public static bool Regenerate_ServiceSAS_BlobSnapshot(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = "?" + Manual_ServiceSas(VersionControl, debug);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Blob Snapshot:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Blob Snapshot URI:\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobSnapshotName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobSnapshotName.v + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get snapshot) (repalce <DateTime> by snapshot datetime):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobSnapshotName.v + "?snapshot=<DateTime>" + Uri.UnescapeDataString(sas).Replace("?", "&") + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + SAS_Utils.SAS.containerName.v + "/" + SAS_Utils.SAS.blobSnapshotName.v + "?snapshot=<DateTime>" + sas.Replace("?", "&") + "\n\n";

            return true;
        }


        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Share Service SAS 
        //---------------------------------------------------------------------------------------------------------------------
        public static bool Regenerate_ServiceSAS_Share(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = "?" + Manual_ServiceSas(VersionControl, debug);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Share:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Share URI:\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (list files):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + "?restype=directory&comp=list" + Uri.UnescapeDataString(sas).Replace("?", "&") + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + "?restype=directory&comp=list" + sas.Replace("?", "&") + "\n\n";

            return true;
        }


        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- File Service SAS 
        //---------------------------------------------------------------------------------------------------------------------
        public static bool Regenerate_ServiceSAS_File(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = "?" + Manual_ServiceSas(VersionControl, debug);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - File:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "File URI:\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + "/" + SAS_Utils.SAS.fileName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + "/" + SAS_Utils.SAS.fileName.v + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get file):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + "/" + SAS_Utils.SAS.fileName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + SAS_Utils.SAS.shareName.v + "/" + SAS_Utils.SAS.fileName.v + sas + "\n\n";

            return true;
        }

        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Queue Service SAS 
        //---------------------------------------------------------------------------------------------------------------------
        public static bool Regenerate_ServiceSAS_Queue(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = "?" + Manual_ServiceSas(VersionControl, debug);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Service SAS - Queue:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Queue URI:\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".queue.core.windows.net/" + SAS_Utils.SAS.queueName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".queue.core.windows.net/" + SAS_Utils.SAS.queueName.v + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (list messages):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".queue.core.windows.net/" + SAS_Utils.SAS.queueName.v + "/messages" + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".queue.core.windows.net/" + SAS_Utils.SAS.queueName.v + "/messages" + sas + "\n\n";

            return true;
        }


        //---------------------------------------------------------------------------------------------------------------------
        //-------------------- Table Service SAS 
        //---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelTableName"></param>
        /// <param name="textBoxAccountName"></param>
        /// <param name="textBoxAccountKey1"></param>
        /// <param name="textBoxTableName"></param>
        /// <param name="textBoxPolicyName"></param>
        /// <param name="BoxAuthResults"></param>
        /// <returns></returns>
        public static bool Regenerate_ServiceSAS_Table(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = Manual_ServiceSas(VersionControl, debug);

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text = "Regenerated Service SAS - Table:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Table URI:\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".table.core.windows.net/" + SAS_Utils.SAS.tableName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".table.core.windows.net/" + SAS_Utils.SAS.tableName.v + sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (list entities):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".table.core.windows.net/" + SAS_Utils.SAS.tableName.v + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".table.core.windows.net/" + SAS_Utils.SAS.tableName.v + sas + "\n\n";

            return true;
        }








        /// <summary>
        /// Regenerate Service Generic SAS without SDK
        /// </summary>
        /// <param name="VersionControl - Usualy sv (Service Version) used to generate the Account SAS"></param>
        /// <param name="debug - Degub mode (true) - Show Canonical-Path-to-Resource and String-to-Sign used to generate the Account SAS"></param>
        /// <returns>Generated Account SAS without starting '?'</returns>
        public static string Manual_ServiceSas(string VersionControl, bool debug = false)
        {
            if (String.Compare(SAS_Utils.SAS.spr.v.ToLower(), "http") == 0) return "'HTTP' ONLY IS NOT SUPPORTED AS SIGNED PROTOCOL";

            string signature = string.Empty;
            string canonicalPathToResource = string.Empty;
            string stringtosign;
            string s = "";

            //-------------------------------------------------------------------------------------
            // Ordering permissions - racwudpl
            SAS_Utils.SAS.sp.v = RestAPI.Order_permissions(SAS_Utils.SAS.sp.v);

            //-------------------------------------------------------------------------------------
            switch (SAS_Utils.SAS.sr.v)
            {
                case "b":   // blob
                    canonicalPathToResource = Get_Service_canonicalPathToResource(VersionControl);
                    stringtosign = Get_Service_stringtosign(VersionControl, "b", SAS_Utils.SAS.st.v, SAS_Utils.SAS.se.v, canonicalPathToResource);
                    break;

                case "bs":   // blob snapshot
                    canonicalPathToResource = Get_Service_canonicalPathToResource(VersionControl);
                    stringtosign = Get_Service_stringtosign(VersionControl, "bs", SAS_Utils.SAS.st.v, SAS_Utils.SAS.se.v, canonicalPathToResource);
                    break;

                case "c":   // container
                    canonicalPathToResource = Get_Service_canonicalPathToResource(VersionControl);
                    stringtosign = Get_Service_stringtosign(VersionControl, "c", SAS_Utils.SAS.st.v, SAS_Utils.SAS.se.v, canonicalPathToResource);
                    break;

                case "f":   // file
                    canonicalPathToResource = Get_Service_canonicalPathToResource(VersionControl);
                    stringtosign = Get_Service_stringtosign(VersionControl, "f", SAS_Utils.SAS.st.v, SAS_Utils.SAS.se.v, canonicalPathToResource);
                    break;

                case "s":   // share
                    canonicalPathToResource = Get_Service_canonicalPathToResource(VersionControl);
                    stringtosign = Get_Service_stringtosign(VersionControl, "s", SAS_Utils.SAS.st.v, SAS_Utils.SAS.se.v, canonicalPathToResource);
                    break;

                default:
                    if (String.IsNullOrEmpty(SAS_Utils.SAS.tableName.v))     // table
                    {
                        canonicalPathToResource = Get_Service_canonicalPathToResource(VersionControl);
                        stringtosign = Get_Service_stringtosign(VersionControl, "t", SAS_Utils.SAS.st.v, SAS_Utils.SAS.se.v, canonicalPathToResource);
                    }
                    else                                    // queue
                    {
                        canonicalPathToResource = Get_Service_canonicalPathToResource(VersionControl);
                        stringtosign = Get_Service_stringtosign(VersionControl, "q", SAS_Utils.SAS.st.v, SAS_Utils.SAS.se.v, canonicalPathToResource);
                    }
                    break;
            }

            //-------------------------------------------------------------------------------------
            //Get a reference to a blob within the container.
            byte[] keyForSigning = System.Convert.FromBase64String(SAS_Utils.SAS.storageAccountKey.v.Trim());

            using (var hmac = new HMACSHA256(keyForSigning))
            {
                signature = System.Convert.ToBase64String(
                   hmac.ComputeHash(Encoding.UTF8.GetBytes(stringtosign))
                );
            }

            //-------------------------------------------------------------------------------------
            string sharedAccessSignature = "";
            sharedAccessSignature += RestAPI.Get_Parameter("sv", VersionControl);
            sharedAccessSignature += RestAPI.Get_Parameter("sr", SAS_Utils.SAS.sr.v);
            sharedAccessSignature += RestAPI.Get_Parameter("st", SAS_Utils.SAS.st.v);
            sharedAccessSignature += RestAPI.Get_Parameter("se", SAS_Utils.SAS.se.v);
            sharedAccessSignature += RestAPI.Get_Parameter("sp", SAS_Utils.SAS.sp.v);
            sharedAccessSignature += RestAPI.Get_Parameter("sip", SAS_Utils.SAS.sip.v);
            sharedAccessSignature += RestAPI.Get_Parameter("spr", SAS_Utils.SAS.spr.v);
            sharedAccessSignature += RestAPI.Get_Parameter("si", SAS_Utils.SAS.si.v);
            sharedAccessSignature += RestAPI.Get_Parameter("sig", signature);

            // Remove the first "&"
            sharedAccessSignature = sharedAccessSignature.TrimStart('&');

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sharedAccessSignature, "sig=", "&"));

            //-------------------------------------------------------------------------------------
            if (debug)
            {
                s += "\n";
                s += "------ Debug Info ----------" + "\n";
                s += "Canonical-Path-to-Resource: " + canonicalPathToResource + "\n";
                s += "---------------------------" + "\n";
                s += "String-to-Sign: \n" + stringtosign;
                s += "---------------------------" + "\n";
            }

            return string.Format(sharedAccessSignature + s);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="VersionControl"></param>
        /// <returns></returns>
        private static string Get_Service_canonicalPathToResource(string VersionControl)
        {
            if (String.Compare(VersionControl, "2015-02-21") >= 0)
                switch (SAS_Utils.SAS.sr.v)
                {
                    case "b":           // blob
                        return ("/blob/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.containerName.v.Trim() + "/" + SAS_Utils.SAS.blobName.v.Trim()).Trim();
                    case "c":           // container 
                        return ("/blob/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.containerName.v.Trim()).Trim();
                    case "f":           // file
                        return ("/file/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.shareName.v.Trim() + "/" + SAS_Utils.SAS.fileName.v.Trim()).Trim();
                    case "s":           // share     
                        return ("/file/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.shareName.v.Trim()).Trim();
                    case "t":           // table
                        return ("/table/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.tableName.v.Trim()).Trim();
                    case "q":           // queue
                        return ("/queue/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.queueName.v.Trim()).Trim();
                }
            else
                switch (SAS_Utils.SAS.sr.v)
                {
                    case "b":           // blob
                        return ("/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.containerName.v.Trim() + "/" + SAS_Utils.SAS.blobName.v.Trim()).Trim();
                    case "c":           // container 
                        return ("/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.containerName.v.Trim()).Trim();
                    case "f":           // file
                        return ("/file/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.shareName.v.Trim() + "/" + SAS_Utils.SAS.fileName.v.Trim()).Trim();
                    case "s":           // share     
                        return ("/file/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.shareName.v.Trim()).Trim();
                    case "t":           // table
                        return ("/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.tableName.v.Trim()).Trim();
                    case "q":           // queue
                        return ("/" + SAS_Utils.SAS.storageAccountName.v.Trim() + "/" + SAS_Utils.SAS.queueName.v.Trim()).Trim();
                }

            return ""; // something else not supported
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="VersionControl"></param>
        /// <param name="sStartTime"></param>
        /// <param name="sExpiryTime"></param>
        /// <param name="canonicalPathToResource"></param>
        /// <returns></returns>
        private static string Get_Service_stringtosign(string VersionControl, string signedresource, string sStartTime, string sExpiryTime, string canonicalPathToResource)
        {
            // adds support for the signed resource and signed blob snapshot time fields
            // for Blob service resources, use the following format:
            if (String.Compare(VersionControl, "2018-11-09") >= 0)
                return SAS_Utils.SAS.sp.v + "\n" +
                       sStartTime + "\n" +
                       sExpiryTime + "\n" +
                       canonicalPathToResource + "\n" +
                       SAS_Utils.SAS.si.v + "\n" +
                       SAS_Utils.SAS.sip.v + "\n" +
                       SAS_Utils.SAS.spr.v + "\n" +
                       SAS_Utils.SAS.sv.v + "\n" +
                       signedresource + "\n" +
                       SAS_Utils.SAS.blobSnapshotTime.v + "\n" +
                       SAS_Utils.SAS.rscc + "\n" +
                       SAS_Utils.SAS.rscd + "\n" +
                       SAS_Utils.SAS.rsce + "\n" +
                       SAS_Utils.SAS.rscl + "\n" +
                       SAS_Utils.SAS.rsct;


            // adds support for the signed IP and signed protocol fields
            if (String.Compare(VersionControl, "2015-04-05") >= 0)
                switch (signedresource)
                {
                    // for Blob or File service resources, use the following format:
                    case "b":   // blob
                    case "f":   // file
                    case "c":   // container - same stringtosign ???
                    case "s":   // share     - same stringtosign ???
                        return SAS_Utils.SAS.sp.v + "\n" +
                               sStartTime + "\n" +
                               sExpiryTime + "\n" +
                               canonicalPathToResource + "\n" +
                               SAS_Utils.SAS.si.v + "\n" +
                               SAS_Utils.SAS.sip.v + "\n" +
                               SAS_Utils.SAS.spr.v + "\n" +
                               SAS_Utils.SAS.sv.v + "\n" +
                               SAS_Utils.SAS.rscc + "\n" +
                               SAS_Utils.SAS.rscd + "\n" +
                               SAS_Utils.SAS.rsce + "\n" +
                               SAS_Utils.SAS.rscl + "\n" +
                               SAS_Utils.SAS.rsct;

                    // for Table service resources, use the following format:
                    case "t":   // table
                        return SAS_Utils.SAS.sp.v + "\n" +
                               sStartTime + "\n" +
                               sExpiryTime + "\n" +
                               canonicalPathToResource + "\n" +
                               SAS_Utils.SAS.si.v + "\n" +
                               SAS_Utils.SAS.sip.v + "\n" +
                               SAS_Utils.SAS.spr.v + "\n" +
                               SAS_Utils.SAS.sv.v + "\n" +
                               SAS_Utils.SAS.spk + "\n" +
                               SAS_Utils.SAS.srk + "\n" +
                               SAS_Utils.SAS.epk + "\n" +
                               SAS_Utils.SAS.erk;

                    // for Queue service resources, use the following format:
                    case "q":   // queue
                        return SAS_Utils.SAS.sp.v + "\n" +
                               sStartTime + "\n" +
                               sExpiryTime + "\n" +
                               canonicalPathToResource + "\n" +
                               SAS_Utils.SAS.si.v + "\n" +
                               SAS_Utils.SAS.sip.v + "\n" +
                               SAS_Utils.SAS.spr.v + "\n" +
                               SAS_Utils.SAS.sv.v;
                }



            if (String.Compare(VersionControl, "2013-08-15") >= 0)          // 2013-08-15 through version 2015-02-21
                switch (signedresource)
                {
                    // for Blob or File service resources using the 2013-08-15 version through version 2015-02-21, use the following format. 
                    // Note that for the File service, SAS is supported beginning with version 2015-02-21
                    case "b":   // blob
                    case "f":   // file
                    case "c":   // container - same stringtosign ???
                    case "s":   // share     - same stringtosign ???
                        if ((signedresource == "f" || signedresource == "s") && VersionControl != "2015-02-21")
                            return "";  // File service, SAS is supported beginning with version 2015-02-21
                        else
                            return SAS_Utils.SAS.sp.v + "\n" +
                                   sStartTime + "\n" +
                                   sExpiryTime + "\n" +
                                   canonicalPathToResource + "\n" +
                                   SAS_Utils.SAS.si.v + "\n" +
                                   SAS_Utils.SAS.sv.v + "\n" +
                                   SAS_Utils.SAS.rscc + "\n" +
                                   SAS_Utils.SAS.rscd + "\n" +
                                   SAS_Utils.SAS.rsce + "\n" +
                                   SAS_Utils.SAS.rscl + "\n" +
                                   SAS_Utils.SAS.rsct;

                    // for Table service resources, use the following format:
                    case "t":   // table
                        return SAS_Utils.SAS.sp.v + "\n" +
                               sStartTime + "\n" +
                               sExpiryTime + "\n" +
                               canonicalPathToResource + "\n" +
                               SAS_Utils.SAS.si.v + "\n" +
                               SAS_Utils.SAS.sv.v + "\n" +
                               SAS_Utils.SAS.spk + "\n" +
                               SAS_Utils.SAS.srk + "\n" +
                               SAS_Utils.SAS.epk + "\n" +
                               SAS_Utils.SAS.erk;

                    // for Queue service resources, use the following format:
                    case "q":   // queue
                        return SAS_Utils.SAS.sp.v + "\n" +
                               sStartTime + "\n" +
                               sExpiryTime + "\n" +
                               canonicalPathToResource + "\n" +
                               SAS_Utils.SAS.si.v + "\n" +
                               SAS_Utils.SAS.sv.v;
                }


            // for Blob service resources for version 2012-02-12, use the following format:
            if (VersionControl == "2012-02-12")
                switch (signedresource)
                {
                    case "b":   // blob
                    case "c":   // container - same stringtosign ???
                        return SAS_Utils.SAS.sp.v + "\n" +
                               sStartTime + "\n" +
                               sExpiryTime + "\n" +
                               canonicalPathToResource + "\n" +
                               SAS_Utils.SAS.si.v + "\n" +
                               SAS_Utils.SAS.sv.v;

                }

            // for Blob service resources for versions prior to 2012-02-12, use the following format:
            if (String.Compare(VersionControl, "2012-02-12") < 0)
                switch (signedresource)
                {
                    case "b":   // blob
                    case "c":   // container - same stringtosign ???
                        return SAS_Utils.SAS.sp.v + "\n" +
                               sStartTime + "\n" +
                               sExpiryTime + "\n" +
                               canonicalPathToResource + "\n" +
                               SAS_Utils.SAS.si.v;
                }

            return ""; // something else not supported
        }

    }



    //-------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------
    //          Account SAS
    //-------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------
    class Account : RestAPI
    {

        /// <summary>
        /// Regenerate Account SAS without SDK, based on ss (Signed Services) parameter
        /// </summary>
        /// <param name="BoxAuthResults - Graphic Text Box to show the results"></param>
        /// <param name="VersionControl - Usualy sv (Service Version) used to generate the Account SAS"></param>
        /// <param name="debug - Degub mode (true) - Show Canonical-Path-to-Resource and String-to-Sign used to generate the Account SAS"></param>
        /// <returns></returns>
        public static bool Regenerate_AccountSAS(TextBox BoxAuthResults, string VersionControl, bool debug = false)
        {
            string sas = "?" + Manual_AccountSas(VersionControl, debug);
            if (sas == "?") return false;

            BoxAuthResults.Text = "\n\n";
            BoxAuthResults.Text += "WARNING:\n";
            BoxAuthResults.Text += "  Different order on parameter values (ex: ss=bfq vs ss=bqf) may generate different valid signature (sig)\n";
            BoxAuthResults.Text += "\n";
            BoxAuthResults.Text += "Regenerated Account SAS token:\n";
            BoxAuthResults.Text += Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += sas + "\n\n";

            BoxAuthResults.Text += "-------------------------------------------------\n";
            if (SAS_Utils.SAS.ss.v.IndexOf("b") != -1)
            {
                BoxAuthResults.Text += "Blob URI:\n";
                //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/" + sas + "\n\n";
            }

            if (SAS_Utils.SAS.ss.v.IndexOf("f") != -1)
            {
                BoxAuthResults.Text += "File URI:\n";
                //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".file.core.windows.net/" + sas + "\n\n";
            }

            if (SAS_Utils.SAS.ss.v.IndexOf("q") != -1)
            {
                BoxAuthResults.Text += "Queue URI:\n";
                //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".queue.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".queue.core.windows.net/" + sas + "\n\n";
            }

            if (SAS_Utils.SAS.ss.v.IndexOf("t") != -1)
            {
                BoxAuthResults.Text += "Table URI:\n";
                //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".table.core.windows.net/" + Uri.UnescapeDataString(sas) + "\n";
                BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".table.core.windows.net/" + sas + "\n\n";
            }

            BoxAuthResults.Text += "-------------------------------------------------\n";
            BoxAuthResults.Text += "Test your SAS on Browser (get blob) (replace <container> and <blob>):\n";
            //BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/<container>/<blob>" + Uri.UnescapeDataString(sas) + "\n";
            BoxAuthResults.Text += "https://" + SAS_Utils.SAS.storageAccountName.v + ".blob.core.windows.net/<container>/<blob>" + sas + "\n\n";


            

            return true;
        }


        /// <summary>
        /// Regenerate Account SAS without SDK
        /// </summary>
        /// <param name="VersionControl - Usualy sv (Service Version) used to generate the Account SAS"></param>
        /// <param name="debug - Degub mode (true) - Show Canonical-Path-to-Resource and String-to-Sign used to generate the Account SAS"></param>
        /// <returns>Generated Account SAS without starting '?'</returns>
        public static string Manual_AccountSas(string VersionControl, bool debug = false)
        {
            if (String.Compare(VersionControl, "2015-04-05") < 0) return "ACCOUNT SAS ONLY SUPPORTS SERVICE VERSION BEGINING AT 2015-04-05";
            if (String.Compare(SAS_Utils.SAS.spr.v.ToLower(), "http") == 0) return "'HTTP' ONLY IS NOT SUPPORTED AS SIGNED PROTOCOL";

            string signature = string.Empty;
            string s = "";

            //-------------------------------------------------------------------------------------
            // Ordering permissions only on Service SAS - Account SAS uses the same order as on ComboBox
            // SAS_Utils.SAS.sp.v = RestAPI.Order_permissions(SAS_Utils.SAS.sp.v);

            //-------------------------------------------------------------------------------------
            /*
             StringToSign = accountname + "\n" +  
                            signedpermissions + "\n" +  
                            signedservice + "\n" +  
                            signedresourcetype + "\n" +  
                            signedstart + "\n" +  
                            signedexpiry + "\n" +  
                            signedIP + "\n" +  
                            signedProtocol + "\n" +  
                            signedversion + "\n" 
             */
            string stringtosign = SAS_Utils.SAS.storageAccountName.v.Trim() + "\n" +
                                    SAS_Utils.SAS.sp.v + "\n" +
                                    SAS_Utils.SAS.ss.v + "\n" +
                                    SAS_Utils.SAS.srt.v + "\n" +
                                    SAS_Utils.SAS.st.v + "\n" +
                                    SAS_Utils.SAS.se.v + "\n" +
                                    SAS_Utils.SAS.sip.v + "\n" +
                                    SAS_Utils.SAS.spr.v + "\n" +
                                    SAS_Utils.SAS.sv.v + "\n";

            //-------------------------------------------------------------------------------------
            byte[] keyForSigning = System.Convert.FromBase64String(SAS_Utils.SAS.storageAccountKey.v.Trim());

            using (var hmac = new HMACSHA256(keyForSigning))
            {
                signature = System.Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(stringtosign))
                );
            }

            //-------------------------------------------------------------------------------------
            string sharedAccessSignature = "";
            sharedAccessSignature += RestAPI.Get_Parameter("sv", SAS_Utils.SAS.sv.v);
            sharedAccessSignature += RestAPI.Get_Parameter("ss", SAS_Utils.SAS.ss.v);
            sharedAccessSignature += RestAPI.Get_Parameter("srt", SAS_Utils.SAS.srt.v);
            sharedAccessSignature += RestAPI.Get_Parameter("st", SAS_Utils.SAS.st.v);
            sharedAccessSignature += RestAPI.Get_Parameter("se", SAS_Utils.SAS.se.v);
            sharedAccessSignature += RestAPI.Get_Parameter("sp", SAS_Utils.SAS.sp.v);
            sharedAccessSignature += RestAPI.Get_Parameter("sip", SAS_Utils.SAS.sip.v);
            sharedAccessSignature += RestAPI.Get_Parameter("spr", SAS_Utils.SAS.spr.v);
            sharedAccessSignature += RestAPI.Get_Parameter("sig", signature);

            // Remove the first "&"
            sharedAccessSignature = sharedAccessSignature.TrimStart('&');

            SAS_Utils.SAS.sig = Uri.UnescapeDataString(SAS_Utils.Get_SASValue(sharedAccessSignature, "sig=", "&"));

            //-------------------------------------------------------------------------------------
            if (debug)
            {
                s += "\n";
                s += "------ Debug Info ----------" + "\n";
                s += "String-to-Sign: \n" + stringtosign;
                s += "---------------------------" + "\n";
            }


            return string.Format(sharedAccessSignature + s);
        }



        //-------------------------------------------------------------------
        // Solution from SDK : signature = ComputeHmac256(keyForSigning, stringtosign);
        // Same results, same sig
        //-------------------------------------------------------------------
        //https://github.com/Azure/azure-storage-net/blob/master/Lib/Common/CloudStorageAccount.cs
        //https://github.com/Azure/azure-storage-net/blob/master/Lib/Common/Core/Auth/SharedAccessSignatureHelper.cs
        //https://github.com/Azure/azure-storage-net/blob/master/Lib/Common/Core/Util/CryptoUtility.cs
        //string signature = SharedAccessSignatureHelper.GetHash(policy, this.Credentials.AccountName, Constants.HeaderConstants.TargetStorageVersion, accountKey.KeyValue);
        //UriQueryBuilder builder = SharedAccessSignatureHelper.GetSignature(policy, signature, accountKey.KeyName, Constants.HeaderConstants.TargetStorageVersion);
        //return builder.ToString();
        internal static string ComputeHmac256(byte[] key, string message)
        {
            using (HashAlgorithm hashAlgorithm = new HMACSHA256(key))
            {
                byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                return Convert.ToBase64String(hashAlgorithm.ComputeHash(messageBuffer));
            }
        }
        //-------------------------------------------------------------------
    }
}

