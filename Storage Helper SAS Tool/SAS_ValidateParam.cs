using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage_Helper_SAS_Tool
{
    /// <summary>
    /// ---------------------------------------------------------------------------------------------------------
    /// Validate all the SAS parameters
    /// Used to validate the string provided on InputBoxSAS after click on "Check SAS parameters", and to 
    /// validate the values on grapic TextBoxes on click on "Regenerate SAS" 
    /// ---------------------------------------------------------------------------------------------------------
    /// </summary>

    class SAS_ValidateParam
    {
        /// <summary>
        /// Validate api-version (optional)
        /// </summary>
        /// <param name="apiVersion"></param>
        /// <returns></returns>
        public static string Api_Version(string apiVersion)
        {
            // no api-version (optional) provided - exit without any information
            if (apiVersion == "not found")
                return SAS_Utils.andSetState("apiVersion", true, "");

            // api-version without any value
            if (String.IsNullOrEmpty(apiVersion))
                return SAS_Utils.andSetState("apiVersion", false, "'api-version' provided without any value (api-version=" + apiVersion + ")");

            // wrong api-version length and format - yyyy-mm-dd
            if (apiVersion.Length != 10 || apiVersion.Substring(4, 1) != "-" || apiVersion.Substring(7, 1) != "-")
                return SAS_Utils.andSetState("apiVersion", false, "Invalid format on 'api-version' provided. Format should be yyyy-mm-dd (api-version=" + apiVersion + ")");

            // api-version date validation - TODO - validate the storage service versions
            try
            {
                DateTime d = Convert.ToDateTime(apiVersion);
            }
            catch (Exception ex)
            {
                return SAS_Utils.andSetState("apiVersion", false, "Invalid date on 'api-version' provided (api-version=" + apiVersion + "): " + ex.Message + "");
            }

            // value validated
            return SAS_Utils.andSetState("apiVersion", true, "");
        }



        public static string Ss(string ss, string spr, string srt)
        {

            if (srt == "not found")     // Service SAS - ss não requerido
                if (ss == "not found")
                    return SAS_Utils.andSetState("ss", true, "");
                else
                    return SAS_Utils.andSetState("ss", false, "'ss' Not required on Service SAS, but provided");

            // no (required) sp provided
            if (ss == "not found")
                return SAS_Utils.andSetState("ss", false, "'ss' Required but not provided");

            // ss without any value
            if (String.IsNullOrEmpty(ss))
                return SAS_Utils.andSetState("ss", false, "'ss' provided without any value (ss=" + ss + ")");

            // found chars not supported by ss paramenter
            if (!Utils.ValidateString(ss, "bfqt"))
                return SAS_Utils.andSetState("ss", false, "Invalid 'ss' parameter provided: (ss=" + ss + ")");


            // value validated
            return SAS_Utils.andSetState("ss", true, "");
        }



        public static string Spr(string spr, string sr, string tn, string sv)
        {
            //-------------------- Valid values -------------------------
            // no (optional) spr provided
            if (spr == "not found")
                return SAS_Utils.andSetState("spr", true, "HTTPS, HTTP protocols allowed (spr optional not provided)");

            // both protocols provided on spr 
            if (spr.IndexOf("http,https") != -1 || spr.IndexOf("https,http") != -1)
                return SAS_Utils.andSetState("spr", true, "HTTPS, HTTP protocols allowed.");

            // https protocol provided on spr
            if (spr == "https")
                return SAS_Utils.andSetState("spr", true, "HTTPS protocol only allowed.");


            //-------------------- invalid values -------------------------
            // spr empty
            if (spr.Length == 0)
                return SAS_Utils.andSetState("spr", false, "Empty optional Allowed Protocols value (spr=" + spr + ")");

            // on Service SAS the 'spr' (protocol) is only supported on Service Version >= 2015-04-05
            if ((sr != "not found" || tn != "not found") && sv.CompareTo("2015-04-05") < 0)
                return SAS_Utils.andSetState("spr", false, "'spr' parameter only supported on Service Version ('sv') 2015-04-05 or later (sv=" + sv + ")");

            // found chars not supported by spr paramenter
            if (!Utils.ValidateString(spr, "https,"))
                return SAS_Utils.andSetState("spr", false, "Invalid protocol provided on 'spr' parameter  (spr=" + spr + ")");

            // http protocol provided on spr (needed to prevent find http in https or httpxxx)
            if (spr == "http")
                return SAS_Utils.andSetState("spr", false, "HTTP protocol only is not a allowed.");

            // wrong protocol
            return SAS_Utils.andSetState("spr", false, "wrong protocol provided on 'spr' parameter  (spr=" + spr + ")");
        }




        public static string Sv(string sv, string sr, string srt, string tn)
        {
            // no (required) sv provided
            if (sv == "not found")
                return SAS_Utils.andSetState("sv", false, "'sv' Required but not provided)");

            // sv empty
            if (sv.Length == 0)
                return SAS_Utils.andSetState("sv", false, "Empty Storage Version value (sv=" + sv + ")");

            //----------------------- Valid sv from the arrays -------------------------------
            // Check Account SAS
            for (int i = 0; i < SAS_Utils.validSV_AccountSAS_ARR.Length; i++)
                if (SAS_Utils.validSV_AccountSAS_ARR[i] == sv)
                    return SAS_Utils.andSetState("sv", true, "Valid Storage Service Version for Account SAS  (sv=" + sv + ")");

            // Check Service SAS
            if (sr != "" || tn != "")
                for (int i = 0; i < SAS_Utils.validSV_ServiceSas_ARR_addon.Length; i++)
                    if (SAS_Utils.validSV_ServiceSas_ARR_addon[i] == sv)
                        return SAS_Utils.andSetState("sv", true, "Valid Storage Service Version for Service SAS (sv=" + sv + ")");
            //----------------------------------------------------------------------------------

            return SAS_Utils.andSetState("sv", false, "Invalid Storage Service Version  (sv=" + sv + ")\n" +
                        "      Please visit the link below to check for new versions that may not yet be supported by this Storage Helper SAS Tool:\n" +
                        "      https://docs.microsoft.com/en-us/rest/api/storageservices/previous-azure-storage-service-versions");
        }






        public static string Sr(string sr, string sv)
        {
            // no (required) sr provided
            if (sr == "not found")
                return SAS_Utils.andSetState("sr", false, "'sr' Required but not provided");

            if (sr.Length > 1 && sr != "bs")
                return SAS_Utils.andSetState("sr", false, "Only one signed resource (b,c,s,f,bs) can be provided  (sr=" + sr + ")");

            // found chars not supported by sr paramenter
            if (!Utils.ValidateString(sr, "bcsf"))
                return SAS_Utils.andSetState("sr", false, "Invalid Signed Resource  (sr=" + sr + ")");

            // Verifying the Service Version for Files
            if ((sr == "s" || sr == "f") && sv.CompareTo("2015-02-21") < 0)
                return SAS_Utils.andSetState("sr", false, "The File and Share resouces (s,f) are only supported on version 2015-02-21 and later (sr=" + sr + ", sv=" + sv + ")");

            // Verifying the Service Version for Blob Snapshots
            if (sr == "bs" && sv.CompareTo("2018-11-09") < 0)
                return SAS_Utils.andSetState("sr", false, "The Blob Snapshot resouce (bs) are only supported on version 2018-11-09 and later (sr=" + sr + ", sv=" + sv + ")");

            //-------------------- sr validated ---------------------------------
            bool state = true;
            string s = "";
            switch (sr)
            {
                case "b":
                    s = "Access to Blob (grants access to the content and metadata of the blob)";
                    break;

                case "c":
                    s = "Access to Container (grants access to the content and metadata of any blob in the container, and to the list of blobs in the container)";
                    break;

                case "s":
                    s = "Access to File Share (grants access to the content and metadata of any file in the share, and to the list of directories and files in the share)";
                    break;

                case "f":
                    s = "Access to File (grants access to the content and metadata of the file.)";
                    break;

                case "bs":
                    s = "Access to Blob Snapshot (grants access to the content and metadata of the specific snapshot, but not the corresponding root blob)";
                    break;

                default:
                    s = "Invalid Signed Resource Types  (sr=" + sr + ")";
                    state = false;
                    break;
            }

            return SAS_Utils.andSetState("sr", state, s);
        }



        public static string Tn(string tn)
        {
            // no (required) tn provided
            if (tn == "not found")
                return SAS_Utils.andSetState("tn", false, "'tn' Required but not provided");

            //-------------------- tn validated ---------------------------------
            return SAS_Utils.andSetState("tn", true, "Access to Table '" + tn + "'");
        }



        public static string Si(string si, string srt)
        {
            // no si provided (optional)
            if (si == "not found")
                return SAS_Utils.andSetState("si", true, "");

            if (si.Length > 64)
                return SAS_Utils.andSetState("si", false, "Invalid Access Policy Name - Max 64 chars supported (si=" + si + " - current " + si.Length.ToString() + " chars)");

            if (srt != "not found")
                return SAS_Utils.andSetState("si", false, "Policy Name ('si') not supported on Account SAS (srt=" + srt + ", si=" + si + ")");

            // value validated
            return SAS_Utils.andSetState("si", true, "Access Policy Name used: '" + si + "'\n" + "  Policy Permissions: TODO ");
        }



        /// <summary>
        /// Validate sp based on the str, sr or tn
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="srt"></param>
        /// <param name="sr"></param>
        /// <param name="tn"></param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Sp(string sp, string srt, string sr, string tn, string sv)
        {

            // no (required) sp provided
            if (sp == "not found")
                return SAS_Utils.andSetState("sp", false, "'sp' Required but not provided");

            if (sp.Length == 0)
                return SAS_Utils.andSetState("sp", false, "Empty Signed Permissions (sp=" + sp + ")");

            //---------------------------------------------
            // State (true/false) defined inside Show_sp_srt()
            if (srt != "not found")
                return Sp_srt(sp, srt, sv);

            if (sr != "not found")
                return Sp_sr(sp, sr, sv);

            if (tn != "not found")
                return Sp_tn(sp, tn, sv);

            // How define Queue ???? - TODO
            //if (sr != "not found")
            //    return sp_queue(sp, sv);

            return SAS_Utils.andSetState("sp", false, "No 'srt', 'sr', or 'tn' provided");
        }


        /// <summary>
        /// Validate sp based on srt - Account SAS for Service, Object, Containers
        /// https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-account-sas#specifying-account-sas-parameters
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="srt"></param>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static string Sp_srt(string sp, string srt, string sv)
        {
            string s = "";
            string v = "Valid permissions for Account SAS are 'rwdlacup'";
            bool state = true;

            // found chars not supported by sp paramenter
            if (!Utils.ValidateString(sp, "rwdlacup") || sp.Length > 8)
                return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions for Account SAS (sp=" + sp + ", srt=" + srt + "). " + v);

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

            SAS_Utils.SAS.sp.s = state;
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
        public static string Sp_sr(string sp, string sr, string sv)
        {
            string s = "";
            string v = "";

            switch (sr)
            {
                case "b":
                    v = "Valid permissions for Blobs are 'rwdac'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdac") || sp.Length > 5)
                        return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions for Blobs (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v);

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
                        SAS_Utils.SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "c":
                    v = "Valid permissions for Containers are 'rwdacl'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdacl") || sp.Length > 6)
                        return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions for Containers (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v);

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
                        SAS_Utils.SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "s":
                    v = "Valid permissions for File Shares are 'rwdlc'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdlc") || sp.Length > 5)
                        return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions to File Share (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v);

                    if (sv.CompareTo("2015-02-21") < 0)
                        return SAS_Utils.andSetState("sp", false, "Invalid Service Version to use File Share permissions on SAS. Needed sv=2015-02-21 or later (sr=" + sr + ", sv=" + sv + ")");

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
                        SAS_Utils.SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "f":
                    v = "Valid permissions for Files are 'rwdc'";

                    // found chars not supported by sp paramenter
                    if (!Utils.ValidateString(sp, "rwdc") || sp.Length > 4)
                        return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions for File (Service SAS) (sp=" + sp + ", sr=" + sr + "). " + v);

                    if (sv.CompareTo("2015-02-21") < 0)
                        return SAS_Utils.andSetState("sp", false, "Invalid Service Version to File permissions on SAS. Needed sv=2015-02-21 or later (sr=" + sr + ", sv=" + sv + ")");

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
                        SAS_Utils.SAS.sp.s = false;
                    }
                    //----------------------------------------------

                    break;

                case "bs":
                    // Valid permissions ??? - TODO
                    //----------------------------------------------
                    v = "Valid permissions for Blob Shapshots are 'rwdlacup' - TODO ???? ";

                    s += "  Permissions for Blob Shapshots (Service SAS) ('sr'=bs):\n";

                    if (sv.CompareTo("2018-11-09") < 0)
                        return SAS_Utils.andSetState("sp", false, "Invalid Service Version to use Blob Shapshots (Service SAS). Needed sv=2018-11-09 or later (sr=" + sr + ", sv=" + sv + ")");

                    if (sp.IndexOf("r") == -1 && sp.IndexOf("w") == -1 && sp.IndexOf("d") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("c") == -1 && sp.IndexOf("u") == -1 && sp.IndexOf("p") == -1)
                        return SAS_Utils.andSetState("sp", false, "No Permissions for Blob Shapshots (sp=" + sp + "). " + v);

                    return SAS_Utils.andSetState("sp", true, "Valid Permissions sp=" + sp + ", for Blob Shapshots");
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
        public static string Sp_tn(string sp, string tn, string sv)
        {
            string s = "";
            string v = "Valid permissions for Tables are 'raud'";

            // found chars not supported by sp paramenter
            if (!Utils.ValidateString(sp, "raud") || sp.Length > 4)
                return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions for Table (Service SAS) (sp=" + sp + "). " + v);

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
                return SAS_Utils.andSetState("sp", false, "No Permissions for Tables (sp=" + sp + ")");
            //----------------------------------------------

            return s;
        }


        public static string Sp_queue(string sp, string sv)
        {
            string v = "Valid permissions for Queues are 'raup'";

            // found chars not supported by sp paramenter
            if (!Utils.ValidateString(sp, "raup") || sp.Length > 4)
                return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions for Queue (Service SAS) (sp=" + sp + "). " + v);

            //----------------------------------------------
            if (sp.IndexOf("r") != -1)
                return SAS_Utils.andSetState("sp", true, "Read metadata and properties, including message count. Peek at messages.");

            if (sp.IndexOf("a") != -1)
                return SAS_Utils.andSetState("sp", true, "Add messages to the queue.");

            if (sp.IndexOf("u") != -1)
                return SAS_Utils.andSetState("sp", true, "Update messages in the queue. Note: Use the Process permission with Update so you can first get the message you want to update.");

            if (sp.IndexOf("p") != -1)
                return SAS_Utils.andSetState("sp", true, "Get and delete messages from the queue.");

            if (sp.IndexOf("r") == -1 && sp.IndexOf("a") == -1 && sp.IndexOf("u") == -1 && sp.IndexOf("p") == -1)
                return SAS_Utils.andSetState("sp", false, "No Permissions for Queues (sp=" + sp + ")");
            //----------------------------------------------

            return SAS_Utils.andSetState("sp", false, "Invalid Signed Permissions for Queue (Service SAS) (sp=" + sp + "). " + v);
        }





        public static string St_se(string st, string se)
        {
            DateTime testDate;
            DateTime seDate;
            DateTime stDate;

            if (se == "not found")  // se Required
                return SAS_Utils.andSetState("se", false, "Required Signed Expiry Date/Time 'se' value is missing");

            if (se.Length == 0)  // se Required
                return SAS_Utils.andSetState("se", false, "Empty required Signed Expiry Date/Time value (se=" + se + ")");

            // Invalid format on Expiry Datime (by lenght)
            if (se.Length != 20 && se.Length != 17 && se.Length != 10)
                return SAS_Utils.andSetState("se", false, "Incorrect format on Signed Expiry Date/Time (se=" + se + ")");

            // Testing valid 'se' date
            try
            {
                seDate = Convert.ToDateTime(se).ToUniversalTime();        // Convert.ToDateTime(se) -> convert to local time - > need to convert again to in UTC time
            }
            catch (Exception ex)
            {
                return SAS_Utils.andSetState("se", false, "Invalid End date (se=" + se + "): " + ex.Message);
            }

            if (seDate.CompareTo(DateTime.Now.ToUniversalTime()) < 0)      // Test ending date in UTC
                return SAS_Utils.andSetState("se", false, "Already Expired - End date (se=" + seDate.ToString().Replace("/", "-") + ", current UTC time=" + DateTime.Now.ToUniversalTime().ToString().Replace("/", "-") + ", current local time=" + DateTime.Now.ToString().Replace("/", "-") + ")");



            //-------------------------------------------------------------------------------------
            // st tests
            //-------------------------------------------------------------------------------------
            if (st.Length == 0)  // se Optional
                return SAS_Utils.andSetState("st", false, "Empty optional Signed Start Date/Time value (st=" + st + ")");

            string s2 = "";
            if (st != "not found")
            {
                // Invalid Start Datime format (by lenght), if provided
                if (st.Length != 20 && st.Length != 17 && st.Length != 10)
                    return SAS_Utils.andSetState("st", false, "Incorrect format on Signed Start Date/Time (st=" + st + ")");

                // Testing valid 'st' date
                try
                {
                    stDate = Convert.ToDateTime(st).ToUniversalTime();        // Convert.ToDateTime(se) -> convert to local time - > need to convert again to in UTC time
                }
                catch (Exception ex)
                {
                    return SAS_Utils.andSetState("st", false, "Invalid Start date (st=" + st + "): " + ex.Message);
                }

                // Test starting date in UTC
                if (stDate.CompareTo(DateTime.Now.ToUniversalTime()) > 0)
                    return SAS_Utils.andSetState("st", false, "SAS not valid yet, at the current date/time (st=" + stDate.ToString().Replace("/", "-") + ", current UTC time=" + DateTime.Now.ToUniversalTime().ToString().Replace("/", "-") + ", current local time=" + DateTime.Now.ToString().Replace("/", "-") + ")");

                // Test Starting date after Ending date
                if (stDate.CompareTo(seDate) > 0)
                    return SAS_Utils.andSetState("st", false, "Signed Start date/time after Signed Expiry Date/Time (st=" + st + ", se=" + se + ")");


                // testing Length st (SignedStart) - optional
                try
                {
                    switch (st.Length)
                    {
                        case 20:    // YYYY-MM-DDThh:mm:ssZ 
                            testDate = DateTimeUtils.FromIso8601Date(st);   // testDate used only to firing the exception if incorrect format found
                            s2 = "  Valid from " + st;
                            break;
                        case 17:    // YYYY-MM-DDThh:mmZ 
                            testDate = Convert.ToDateTime(st);
                            s2 = "  Valid from " + st;
                            break;
                        case 10:    // YYYY-MM-DD
                            testDate = Convert.ToDateTime(st);
                            s2 = "  Valid from " + st;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    return SAS_Utils.andSetState("st", false, "Invalid Start date (st=" + st + "): " + ex.Message);
                }
            }
            //-------------------------------------------------------------------------------------


            string s = "";
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
                    s = "  Valid up to " + se;
                else                    // SignedStart provided and valid
                    s = s2 + " to " + se;
            }
            catch (Exception ex)
            {
                return SAS_Utils.andSetState("se", false, "Invalid End date (se=" + se + "): " + ex.Message);
            }


            //-------------------- se validated ---------------------------------
            return SAS_Utils.andSetState("se", true, s);
        }





        public static string Sip(string sip)
        {
            // sip not foud (optional)
            if (sip == "not found")
                return SAS_Utils.andSetState("sip", true, "All client IP addresses allowed (sip optional not provided)");

            // sip empty
            if (sip.Length == 0)
                return SAS_Utils.andSetState("sip", false, "Empty optional Signed IP value (sip=" + sip + ")");

            // '-' found at the end
            int i = sip.IndexOf("-");
            int p = sip.IndexOf(".");
            if (i == sip.Length - 1 || p == sip.Length - 1)
                return SAS_Utils.andSetState("sip", false, "Invalid IP address range - IP address could not end with '-' or '.'  (sip=" + sip + ")");

            // '-' found at the begin 
            if (i == 0 || p == 0)
                return SAS_Utils.andSetState("sip", false, "Invalid IP address range - IP address could not start with '-' or '.' (sip=" + sip + ")");

            // Two or more '-' found
            i = sip.IndexOf("--");
            p = sip.IndexOf("..");
            if (i != -1 || p != -1)
                return SAS_Utils.andSetState("sip", false, "Invalid IP address range - Two or more '-' or '.' found (sip=" + sip + ")");


            // -------- IP Bytes validation --------------------
            string s2 = Validate_Sip(sip);
            if (s2 != "")  // error found on IP
                return SAS_Utils.andSetState("sip", false, s2);


            //------------- Ok, IP validated -----------------
            if (SAS_Utils.toIP[0] == 0)      // toIP empty - not used
                return SAS_Utils.andSetState("sip", true, "IP address: " + SAS_Utils.fromIP[0] + "." + SAS_Utils.fromIP[1] + "." + SAS_Utils.fromIP[2] + "." + SAS_Utils.fromIP[3]);
            else
                return SAS_Utils.andSetState("sip", true, "IP address range from: " + SAS_Utils.fromIP[0] + "." + SAS_Utils.fromIP[1] + "." + SAS_Utils.fromIP[2] + "." + SAS_Utils.fromIP[3] + " to " + SAS_Utils.toIP[0] + "." + SAS_Utils.toIP[1] + "." + SAS_Utils.toIP[2] + "." + SAS_Utils.toIP[3]);

        }



        /*
        /// <summary>
        /// Validate sip on the SAS struct
        /// </summary>
        /// <returns>"" empty means IPs validated - no error found</returns>
        public static string Validate_Sip()
        {
            string s = Validate_Sip(SAS_Utils.SAS.sip.v);
            if (s != "")  // error found on IP
                return SAS_Utils.andSetState("sip", false, s);

            return ""; // empty means IPs validated - no error found
        }
        */


        /// <summary>
        /// Validate sip string passed on parameter
        /// Save Bytes on structs fromIP[]  and toIP[] to be used on SDK v12
        /// </summary>
        /// <param name="sip"></param>
        /// <returns>"" empty means IPs validated - no error found</returns>
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
                                return "Invalid IP address (sip=" + sip + ")";
                            strByte = sip2.Substring(0, j2);    // b - one byte from IP
                            sip2 = sip2.Substring(j2 + 1);      // remove the readed byte
                        }
                        else // last byte
                            strByte = sip2;    // b - last byte from IP

                        try
                        {
                            IPbyte[myIP, myByte] = Int32.Parse(strByte);
                            if (((myByte == 0 && IPbyte[myIP, myByte] == 0) || (myByte > 0 && IPbyte[myIP, myByte] < 0)) || IPbyte[myIP, myByte] > 255) // p=0 - 1st byte could not be 0
                                return "Invalid IP address (sip=" + sip + ")";
                        }
                        catch (Exception ex)
                        {
                            return "Invalid IP address (sip=" + sip + "): " + ex.Message;
                        }
                    }
                }
            }

            // Start IP greater than End IP
            if (!String.IsNullOrEmpty(IP[1]))
                if (IPbyte[0, 0] > IPbyte[1, 0])
                    return "Invalid IP address range - Start IP greater than End IP (sip=" + sip + ")";


            // used by v12.0.0.0_preview
            //------------------------------------------
            // copy IP Bytes to fromIP[]
            for (int bf = 0; bf < 4; bf++)
                SAS_Utils.fromIP[bf] = (byte)IPbyte[0, bf];

            // copy IP Bytes to toIP[] 
            for (int bf = 0; bf < 4; bf++)
                SAS_Utils.toIP[bf] = (byte)IPbyte[1, bf];
            //------------------------------------------

            return "";  // empty means IPs validated - no error found
        }





        /// <summary>
        /// Format the output string for Endpoints provided on connection string
        /// </summary>
        public static string EndpointFormat(string endpoint, string service, string spr)
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
        /// 
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="srt"></param>
        /// <param name="tn"></param>
        /// <returns></returns>
        public static string Sr_srt_tn(string sr, string srt, string tn)
        {
            // no (required) srt provided
            if (sr == "not found" && srt == "not found" && tn == "not found")
                return "";

            // Two or more srt, sr or tn provided
            if ((sr != "not found" && srt != "not found") || (sr != "not found" && tn != "not found") || (srt != "not found" && tn != "not found"))
                return "all";

            if (sr != "not found")
                return "sr";

            if (srt != "not found")
                return "srt";

            if (tn != "not found")
                return "tn";

            return "";
        }




        public static string Srt_sr_tn(string s)
        {
            switch (s)
            {
                case "":
                    SAS_Utils.SAS.sr.s = false;     // Mark to be red: srt, sr, tn
                    SAS_Utils.SAS.tn.s = false;
                    return SAS_Utils.andSetState("srt", false, s += "No 'sr', 'srt' or 'tn' provided. One of them should be provided.");

                case "all":
                    SAS_Utils.SAS.sr.s = false;     // Mark to be red: srt, sr, tn
                    SAS_Utils.SAS.tn.s = false;
                    return SAS_Utils.andSetState("srt", false, s += "Only one 'sr', 'srt' or 'tn' can be provided");
            }

            return "";
        }


    }
}
