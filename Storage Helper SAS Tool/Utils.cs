﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.Storage.File;
//using Microsoft.Azure.Cosmos.Table; // Moved to Cosmos library
using Microsoft.Azure.Storage.Auth;



namespace Storage_Helper_SAS_Tool
{


    //--------------------------------------------------------------------------------------------
    // Date Time Utils Class
    //--------------------------------------------------------------------------------------------
    static class DateTimeUtils
    {


        private const string DateTimeFormat = "{0}-{1}-{2}T{3}:{4}:{5}Z";

        /// <summary>
        /// To the iso8601 date. 
        /// </summary>
        /// <returns>The date</returns>
        public static string ToIso8601Date(this DateTime date)
        {
            return string.Format(
                DateTimeFormat,
                date.Year,
                PadLeft(date.Month),
                PadLeft(date.Day),
                PadLeft(date.Hour),
                PadLeft(date.Minute),
                PadLeft(date.Second));
        }

        /// <summary> 
        /// Froms the iso8601 date. 
        /// </summary>
        /// <returns>The date</returns>
        public static DateTime FromIso8601Date(this string date)
        {
            return DateTime.ParseExact(date.Replace("T", " "), "u", CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string PadLeft(int number)
        {
            if (number < 10)
            {
                return string.Format("0{0}", number);
            }

            return number.ToString(CultureInfo.InvariantCulture);
        }



        /// <summary>
        /// Validate st, se
        /// </summary>
        public static bool Validate_DateTimes(Label label_st, Label label_se)
        {
            try
            {
                if (String.IsNullOrEmpty(SAS_Utils.SAS.st.v))
                    SAS_Utils.SAS.stDateTime = DateTime.UtcNow.ToUniversalTime(); // not used - replacenetnt to null - SAS.st.v should be checked to check if was provided or not
                else
                    SAS_Utils.SAS.stDateTime = Convert.ToDateTime(SAS_Utils.SAS.st.v).ToUniversalTime();
            }
            catch (Exception ex)
            {
                label_st.Foreground = Brushes.Red;
                MessageBox.Show("Invalid date specified on Signed Start Date/Time parameter\n\nSystem error:\n" + ex, "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            try
            {
                SAS_Utils.SAS.seDateTime = Convert.ToDateTime(SAS_Utils.SAS.se.v).ToUniversalTime();
            }
            catch (Exception ex)
            {
                label_se.Foreground = Brushes.Red;
                MessageBox.Show("Invalid date specified on Signed End Date/Time parameter\n\nSystem error:\n" + ex, "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }
    }
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------



    //--------------------------------------------------------------------------------------------
    // Utils Class
    //--------------------------------------------------------------------------------------------
    class Utils
    {

        /// <summary>
        /// Validate source to make sure all the chars in source are in validChars
        /// </summary>
        /// <param name="source"></param>
        /// <param name="validChars"></param>
        /// <returns></returns>
        public static bool ValidateString(string source, string validChars)
        {
            for (int i = 0; i < source.Length; i++)
                if (validChars.IndexOf(source.Substring(i, 1)) == -1)     // Char i on sorce not in validChars
                    return false;
            return true;
        }


        /// <summary>
        /// Count how many chars 'ch' are in source
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static int Count(string source, char ch)
        { 
            int count = 0;
            foreach (char c in source) 
              if (c == ch) count++;
            return count;
        }




        /// <summary>
        /// Converts the permissions specified for the shared access policy to a string.
        /// Same permissions for Policy and 'sr' (Service SAS) parameter 
        /// 
        /// SharedAccessBlobPermissions: Add	16	
        ///                              Create	32	
        ///                              Delete	4	
        ///                              List	8	
        ///                              None	0	
        ///                              Read	1	
        ///                              Write	2
        /// </summary>
        /// <param name="permissions">The shared access permissions.</param>
        /// <returns>The shared access permissions in string format.</returns>
        public static string BlobPermissionsToString(SharedAccessBlobPermissions permissions)
        {
            // The service supports a fixed order => rwdl
            StringBuilder builder = new StringBuilder();
            if ((permissions & SharedAccessBlobPermissions.Read) == SharedAccessBlobPermissions.Read) builder.Append("r");
            if ((permissions & SharedAccessBlobPermissions.Write) == SharedAccessBlobPermissions.Write) builder.Append("w");
            if ((permissions & SharedAccessBlobPermissions.Delete) == SharedAccessBlobPermissions.Delete) builder.Append("d");
            if ((permissions & SharedAccessBlobPermissions.List) == SharedAccessBlobPermissions.List) builder.Append("l");
            if ((permissions & SharedAccessBlobPermissions.Add) == SharedAccessBlobPermissions.Add) builder.Append("a");
            if ((permissions & SharedAccessBlobPermissions.Create) == SharedAccessBlobPermissions.Create) builder.Append("c");
            return builder.ToString();
        }




        /// <summary>
        /// Converts the permissions specified for the shared access policy to a string.
        /// Same permissions for Policy and 'sr' (Service SAS) parameter 
        /// 
        /// SharedAccessQueuePermissions:   Add	            2	
        ///                                 None	        0	
        ///                                 ProcessMessages	8	
        ///                                 Read	        1	
        ///                                 Update	        4
        /// </summary>
        /// <param name="permissions">The shared access permissions.</param>
        /// <returns>The shared access permissions in string format.</returns>
        public static string QueuePermissionsToString(SharedAccessQueuePermissions permissions)
        {
            // The service supports a fixed order => rwdl
            StringBuilder builder = new StringBuilder();
            if ((permissions & SharedAccessQueuePermissions.Read) == SharedAccessQueuePermissions.Read) builder.Append("r");
            if ((permissions & SharedAccessQueuePermissions.Add) == SharedAccessQueuePermissions.Add) builder.Append("a");
            if ((permissions & SharedAccessQueuePermissions.Update) == SharedAccessQueuePermissions.Update) builder.Append("u");
            if ((permissions & SharedAccessQueuePermissions.ProcessMessages) == SharedAccessQueuePermissions.ProcessMessages) builder.Append("p");
            return builder.ToString();
        }



        /// <summary>
        /// Converts the permissions specified for the shared access policy to a string.
        /// Same permissions for Policy and 'sr' (Service SAS) parameter 
        /// 
        /// SharedAccessFilePermissions:  - TODO - not supported by API ???
        /// </summary>
        /// <param name="permissions">The shared access permissions.</param>
        /// <returns>The shared access permissions in string format.</returns>
        public static string FilePermissionsToString(SharedAccessFilePermissions permissions)
        {
            // The service supports a fixed order => rwdl
            StringBuilder builder = new StringBuilder();
            if ((permissions & SharedAccessFilePermissions.Read) == SharedAccessFilePermissions.Read) builder.Append("r");
            if ((permissions & SharedAccessFilePermissions.Write) == SharedAccessFilePermissions.Write) builder.Append("w");
            if ((permissions & SharedAccessFilePermissions.Delete) == SharedAccessFilePermissions.Delete) builder.Append("d");
            if ((permissions & SharedAccessFilePermissions.List) == SharedAccessFilePermissions.List) builder.Append("l");
            if ((permissions & SharedAccessFilePermissions.Create) == SharedAccessFilePermissions.Create) builder.Append("c");
            return builder.ToString();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageCredentials"></param>
        /// <param name="AuthResults"></param>
        public static void Show_Credentials(StorageCredentials storageCredentials, TextBox AuthResults)
        {
            AuthResults.Text += "\n-----------------------------------\n";
            AuthResults.Text += storageCredentials.ToString() + "\n";
            AuthResults.Text += "Account Name: " + storageCredentials.AccountName + "\n";
            AuthResults.Text += "Credentials type: ";
                if (storageCredentials.IsToken)     AuthResults.Text += "Token ";
                if (storageCredentials.IsSharedKey) AuthResults.Text += "SharedKey ";
                if (storageCredentials.IsSAS)       AuthResults.Text += "SAS ";
                if (storageCredentials.IsAnonymous) AuthResults.Text += "Anonymous ";
            AuthResults.Text += "\n";
            AuthResults.Text += "Key Name: " + storageCredentials.KeyName + "\n";
            //AuthResults.Text += "SAS Signature: " + storageCredentials.SASSignature + "\n";   // Exception
            AuthResults.Text += "SAS Token: " + storageCredentials.SASToken + "\n";

            AuthResults.Text += "-----------------------------------\n";
            AuthResults.Text += "Account Key from storageCredentials:\n";
            AuthResults.Text += storageCredentials.ExportBase64EncodedKey();
        }



        /// <summary>
        /// Used to quick check if an TextBox have value
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <returns>false, if empty</returns>
        public static bool TextBoxEmpty(TextBox tb, string msg, string title)
        {
            if (String.IsNullOrEmpty(tb.Text))
            {
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used to quick check if an string variable is empty
        /// </summary>
        /// <param name="s"></param>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool StringEmpty(Label label, string s, string msg, string title)
        {
            if (String.IsNullOrEmpty(s))
            {
                label.Foreground = Brushes.Red;
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return true;
            }
            return false;
        }




        /// <summary>
        /// Get File Name from Uri 
        /// https://storage.blob.core.windows.net/container/folder1/folder1/blob.txt  -> blob.txt
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string Get_FileNameFromUri(string uri)
        {
            return uri.Substring(uri.LastIndexOf('/') + 1);
        }








        /// <summary>
        /// Show messageBox returning false - used to shown message and abort the processing
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool WithMessage(string msg, string title)
        {
            MessageBox.Show(msg, msg, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return false;
        }


        /// <summary>
        /// Remove comma from begin of the string
        /// Used by Listboxes
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveCommaBegin(string str)
        {
            if(str.StartsWith(","))
                return str.Substring(1);    // exclude the first char (",")

            return str;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Convert file extension to Content Type
        /// </summary>
        /// <param name="extension"></param>
        /// <returns>Content Type</returns>
        public string Get_ContentType(string extension)
        { 
            string contentType;
            _contentTypes.TryGetValue(extension, out contentType);

            return contentType;
        }


        /// <summary>
        /// Content Types dictionary
        /// </summary>
        private readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>()
        {
            {".x3d", "application/vnd.hzn-3d-crossword"},
            {".3gp", "video/3gpp"},
            {".3g2", "video/3gpp2"},
            {".mseq", "application/vnd.mseq"},
            {".pwn", "application/vnd.3m.post-it-notes"},
            {".plb", "application/vnd.3gpp.pic-bw-large"},
            {".psb", "application/vnd.3gpp.pic-bw-small"},
            {".pvb", "application/vnd.3gpp.pic-bw-var"},
            {".tcap", "application/vnd.3gpp2.tcap"},
            {".7z", "application/x-7z-compressed"},
            {".abw", "application/x-abiword"},
            {".ace", "application/x-ace-compressed"},
            {".acc", "application/vnd.americandynamics.acc"},
            {".acu", "application/vnd.acucobol"},
            {".atc", "application/vnd.acucorp"},
            {".adp", "audio/adpcm"},
            {".aab", "application/x-authorware-bin"},
            {".aam", "application/x-authorware-map"},
            {".aas", "application/x-authorware-seg"},
            {".air", "application/vnd.adobe.air-application-installer-package+zip"},
            {".swf", "application/x-shockwave-flash"},
            {".fxp", "application/vnd.adobe.fxp"},
            {".pdf", "application/pdf"},
            {".ppd", "application/vnd.cups-ppd"},
            {".dir", "application/x-director"},
            {".xdp", "application/vnd.adobe.xdp+xml"},
            {".xfdf", "application/vnd.adobe.xfdf"},
            {".aac", "audio/x-aac"},
            {".ahead", "application/vnd.ahead.space"},
            {".azf", "application/vnd.airzip.filesecure.azf"},
            {".azs", "application/vnd.airzip.filesecure.azs"},
            {".azw", "application/vnd.amazon.ebook"},
            {".ami", "application/vnd.amiga.ami"},
            {".apk", "application/vnd.android.package-archive"},
            {".cii", "application/vnd.anser-web-certificate-issue-initiation"},
            {".fti", "application/vnd.anser-web-funds-transfer-initiation"},
            {".atx", "application/vnd.antix.game-component"},
            {".mpkg", "application/vnd.apple.installer+xml"},
            {".aw", "application/applixware"},
            {".les", "application/vnd.hhe.lesson-player"},
            {".swi", "application/vnd.aristanetworks.swi"},
            {".s", "text/x-asm"},
            {".atomcat", "application/atomcat+xml"},
            {".atomsvc", "application/atomsvc+xml"},
            {".atom, .xml", "application/atom+xml"},
            {".ac", "application/pkix-attr-cert"},
            {".aif", "audio/x-aiff"},
            {".avi", "video/x-msvideo"},
            {".aep", "application/vnd.audiograph"},
            {".dxf", "image/vnd.dxf"},
            {".dwf", "model/vnd.dwf"},
            {".par", "text/plain-bas"},
            {".bcpio", "application/x-bcpio"},
            {".bin", "application/octet-stream"},
            {".bmp", "image/bmp"},
            {".torrent", "application/x-bittorrent"},
            {".cod", "application/vnd.rim.cod"},
            {".mpm", "application/vnd.blueice.multipass"},
            {".bmi", "application/vnd.bmi"},
            {".sh", "application/x-sh"},
            {".btif", "image/prs.btif"},
            {".rep", "application/vnd.businessobjects"},
            {".bz", "application/x-bzip"},
            {".bz2", "application/x-bzip2"},
            {".csh", "application/x-csh"},
            {".c", "text/x-c"},
            {".cdxml", "application/vnd.chemdraw+xml"},
            {".css", "text/css"},
            {".cdx", "chemical/x-cdx"},
            {".cml", "chemical/x-cml"},
            {".csml", "chemical/x-csml"},
            {".cdbcmsg", "application/vnd.contact.cmsg"},
            {".cla", "application/vnd.claymore"},
            {".c4g", "application/vnd.clonk.c4group"},
            {".sub", "image/vnd.dvb.subtitle"},
            {".cdmia", "application/cdmi-capability"},
            {".cdmic", "application/cdmi-container"},
            {".cdmid", "application/cdmi-domain"},
            {".cdmio", "application/cdmi-object"},
            {".cdmiq", "application/cdmi-queue"},
            {".c11amc", "application/vnd.cluetrust.cartomobile-config"},
            {".c11amz", "application/vnd.cluetrust.cartomobile-config-pkg"},
            {".ras", "image/x-cmu-raster"},
            {".dae", "model/vnd.collada+xml"},
            {".csv", "text/csv"},
            {".cpt", "application/mac-compactpro"},
            {".wmlc", "application/vnd.wap.wmlc"},
            {".cgm", "image/cgm"},
            {".ice", "x-conference/x-cooltalk"},
            {".cmx", "image/x-cmx"},
            {".xar", "application/vnd.xara"},
            {".cmc", "application/vnd.cosmocaller"},
            {".cpio", "application/x-cpio"},
            {".clkx", "application/vnd.crick.clicker"},
            {".clkk", "application/vnd.crick.clicker.keyboard"},
            {".clkp", "application/vnd.crick.clicker.palette"},
            {".clkt", "application/vnd.crick.clicker.template"},
            {".clkw", "application/vnd.crick.clicker.wordbank"},
            {".wbs", "application/vnd.criticaltools.wbs+xml"},
            {".cryptonote", "application/vnd.rig.cryptonote"},
            {".cif", "chemical/x-cif"},
            {".cmdf", "chemical/x-cmdf"},
            {".cu", "application/cu-seeme"},
            {".cww", "application/prs.cww"},
            {".curl", "text/vnd.curl"},
            {".dcurl", "text/vnd.curl.dcurl"},
            {".mcurl", "text/vnd.curl.mcurl"},
            {".scurl", "text/vnd.curl.scurl"},
            {".car", "application/vnd.curl.car"},
            {".pcurl", "application/vnd.curl.pcurl"},
            {".cmp", "application/vnd.yellowriver-custom-menu"},
            {".dssc", "application/dssc+der"},
            {".xdssc", "application/dssc+xml"},
            {".deb", "application/x-debian-package"},
            {".uva", "audio/vnd.dece.audio"},
            {".uvi", "image/vnd.dece.graphic"},
            {".uvh", "video/vnd.dece.hd"},
            {".uvm", "video/vnd.dece.mobile"},
            {".uvu", "video/vnd.uvvu.mp4"},
            {".uvp", "video/vnd.dece.pd"},
            {".uvs", "video/vnd.dece.sd"},
            {".uvv", "video/vnd.dece.video"},
            {".dvi", "application/x-dvi"},
            {".seed", "application/vnd.fdsn.seed"},
            {".dtb", "application/x-dtbook+xml"},
            {".res", "application/x-dtbresource+xml"},
            {".ait", "application/vnd.dvb.ait"},
            {".svc", "application/vnd.dvb.service"},
            {".eol", "audio/vnd.digital-winds"},
            {".djvu", "image/vnd.djvu"},
            {".dtd", "application/xml-dtd"},
            {".mlp", "application/vnd.dolby.mlp"},
            {".wad", "application/x-doom"},
            {".dpg", "application/vnd.dpgraph"},
            {".dra", "audio/vnd.dra"},
            {".dfac", "application/vnd.dreamfactory"},
            {".dts", "audio/vnd.dts"},
            {".dtshd", "audio/vnd.dts.hd"},
            {".dwg", "image/vnd.dwg"},
            {".geo", "application/vnd.dynageo"},
            {".es", "application/ecmascript"},
            {".mag", "application/vnd.ecowin.chart"},
            {".mmr", "image/vnd.fujixerox.edmics-mmr"},
            {".rlc", "image/vnd.fujixerox.edmics-rlc"},
            {".exi", "application/exi"},
            {".mgz", "application/vnd.proteus.magazine"},
            {".epub", "application/epub+zip"},
            {".eml", "message/rfc822"},
            {".nml", "application/vnd.enliven"},
            {".xpr", "application/vnd.is-xpr"},
            {".xif", "image/vnd.xiff"},
            {".xfdl", "application/vnd.xfdl"},
            {".emma", "application/emma+xml"},
            {".ez2", "application/vnd.ezpix-album"},
            {".ez3", "application/vnd.ezpix-package"},
            {".fst", "image/vnd.fst"},
            {".fvt", "video/vnd.fvt"},
            {".fbs", "image/vnd.fastbidsheet"},
            {".fe_launch", "application/vnd.denovo.fcselayout-link"},
            {".f4v", "video/x-f4v"},
            {".flv", "video/x-flv"},
            {".fpx", "image/vnd.fpx"},
            {".npx", "image/vnd.net-fpx"},
            {".flx", "text/vnd.fmi.flexstor"},
            {".fli", "video/x-fli"},
            {".ftc", "application/vnd.fluxtime.clip"},
            {".fdf", "application/vnd.fdf"},
            {".f", "text/x-fortran"},
            {".mif", "application/vnd.mif"},
            {".fm", "application/vnd.framemaker"},
            {".fh", "image/x-freehand"},
            {".fsc", "application/vnd.fsc.weblaunch"},
            {".fnc", "application/vnd.frogans.fnc"},
            {".ltf", "application/vnd.frogans.ltf"},
            {".ddd", "application/vnd.fujixerox.ddd"},
            {".xdw", "application/vnd.fujixerox.docuworks"},
            {".xbd", "application/vnd.fujixerox.docuworks.binder"},
            {".oas", "application/vnd.fujitsu.oasys"},
            {".oa2", "application/vnd.fujitsu.oasys2"},
            {".oa3", "application/vnd.fujitsu.oasys3"},
            {".fg5", "application/vnd.fujitsu.oasysgp"},
            {".bh2", "application/vnd.fujitsu.oasysprs"},
            {".spl", "application/x-futuresplash"},
            {".fzs", "application/vnd.fuzzysheet"},
            {".g3", "image/g3fax"},
            {".gmx", "application/vnd.gmx"},
            {".gtw", "model/vnd.gtw"},
            {".txd", "application/vnd.genomatix.tuxedo"},
            {".ggb", "application/vnd.geogebra.file"},
            {".ggt", "application/vnd.geogebra.tool"},
            {".gdl", "model/vnd.gdl"},
            {".gex", "application/vnd.geometry-explorer"},
            {".gxt", "application/vnd.geonext"},
            {".g2w", "application/vnd.geoplan"},
            {".g3w", "application/vnd.geospace"},
            {".gsf", "application/x-font-ghostscript"},
            {".bdf", "application/x-font-bdf"},
            {".gtar", "application/x-gtar"},
            {".texinfo", "application/x-texinfo"},
            {".gnumeric", "application/x-gnumeric"},
            {".kml", "application/vnd.google-earth.kml+xml"},
            {".kmz", "application/vnd.google-earth.kmz"},
            {".gqf", "application/vnd.grafeq"},
            {".gif", "image/gif"},
            {".gv", "text/vnd.graphviz"},
            {".gac", "application/vnd.groove-account"},
            {".ghf", "application/vnd.groove-help"},
            {".gim", "application/vnd.groove-identity-message"},
            {".grv", "application/vnd.groove-injector"},
            {".gtm", "application/vnd.groove-tool-message"},
            {".tpl", "application/vnd.groove-tool-template"},
            {".vcg", "application/vnd.groove-vcard"},
            {".h261", "video/h261"},
            {".h263", "video/h263"},
            {".h264", "video/h264"},
            {".hpid", "application/vnd.hp-hpid"},
            {".hps", "application/vnd.hp-hps"},
            {".hdf", "application/x-hdf"},
            {".rip", "audio/vnd.rip"},
            {".hbci", "application/vnd.hbci"},
            {".jlt", "application/vnd.hp-jlyt"},
            {".pcl", "application/vnd.hp-pcl"},
            {".hpgl", "application/vnd.hp-hpgl"},
            {".hvs", "application/vnd.yamaha.hv-script"},
            {".hvd", "application/vnd.yamaha.hv-dic"},
            {".hvp", "application/vnd.yamaha.hv-voice"},
            {".sfd-hdstx", "application/vnd.hydrostatix.sof-data"},
            {".stk", "application/hyperstudio"},
            {".hal", "application/vnd.hal+xml"},
            {".html", "text/html"},
            {".irm", "application/vnd.ibm.rights-management"},
            {".sc", "application/vnd.ibm.secure-container"},
            {".ics", "text/calendar"},
            {".icc", "application/vnd.iccprofile"},
            {".ico", "image/x-icon"},
            {".igl", "application/vnd.igloader"},
            {".ief", "image/ief"},
            {".ivp", "application/vnd.immervision-ivp"},
            {".ivu", "application/vnd.immervision-ivu"},
            {".rif", "application/reginfo+xml"},
            {".3dml", "text/vnd.in3d.3dml"},
            {".spot", "text/vnd.in3d.spot"},
            {".igs", "model/iges"},
            {".i2g", "application/vnd.intergeo"},
            {".cdy", "application/vnd.cinderella"},
            {".xpw", "application/vnd.intercon.formnet"},
            {".fcs", "application/vnd.isac.fcs"},
            {".ipfix", "application/ipfix"},
            {".cer", "application/pkix-cert"},
            {".pki", "application/pkixcmp"},
            {".crl", "application/pkix-crl"},
            {".pkipath", "application/pkix-pkipath"},
            {".igm", "application/vnd.insors.igm"},
            {".rcprofile", "application/vnd.ipunplugged.rcprofile"},
            {".irp", "application/vnd.irepository.package+xml"},
            {".jad", "text/vnd.sun.j2me.app-descriptor"},
            {".jar", "application/java-archive"},
            {".class", "application/java-vm"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".ser", "application/java-serialized-object"},
            {".java", "text/x-java-source,java"},
            {".js", "application/javascript"},
            {".json", "application/json"},
            {".joda", "application/vnd.joost.joda-archive"},
            {".jpm", "video/jpm"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg" },
            {".jpgv", "video/jpeg"},
            {".ktz", "application/vnd.kahootz"},
            {".mmd", "application/vnd.chipnuts.karaoke-mmd"},
            {".karbon", "application/vnd.kde.karbon"},
            {".chrt", "application/vnd.kde.kchart"},
            {".kfo", "application/vnd.kde.kformula"},
            {".flw", "application/vnd.kde.kivio"},
            {".kon", "application/vnd.kde.kontour"},
            {".kpr", "application/vnd.kde.kpresenter"},
            {".ksp", "application/vnd.kde.kspread"},
            {".kwd", "application/vnd.kde.kword"},
            {".htke", "application/vnd.kenameaapp"},
            {".kia", "application/vnd.kidspiration"},
            {".kne", "application/vnd.kinar"},
            {".sse", "application/vnd.kodak-descriptor"},
            {".lasxml", "application/vnd.las.las+xml"},
            {".latex", "application/x-latex"},
            {".lbd", "application/vnd.llamagraphics.life-balance.desktop"},
            {".lbe", "application/vnd.llamagraphics.life-balance.exchange+xml"},
            {".jam", "application/vnd.jam"},
            {".123", "application/vnd.lotus-1-2-3"},
            {".apr", "application/vnd.lotus-approach"},
            {".pre", "application/vnd.lotus-freelance"},
            {".nsf", "application/vnd.lotus-notes"},
            {".org", "application/vnd.lotus-organizer"},
            {".scm", "application/vnd.lotus-screencam"},
            {".lwp", "application/vnd.lotus-wordpro"},
            {".lvp", "audio/vnd.lucent.voice"},
            {".m3u", "audio/x-mpegurl"},
            {".m4v", "video/x-m4v"},
            {".hqx", "application/mac-binhex40"},
            {".portpkg", "application/vnd.macports.portpkg"},
            {".mgp", "application/vnd.osgeo.mapguide.package"},
            {".mrc", "application/marc"},
            {".mrcx", "application/marcxml+xml"},
            {".mxf", "application/mxf"},
            {".nbp", "application/vnd.wolfram.player"},
            {".ma", "application/mathematica"},
            {".mathml", "application/mathml+xml"},
            {".mbox", "application/mbox"},
            {".mc1", "application/vnd.medcalcdata"},
            {".mscml", "application/mediaservercontrol+xml"},
            {".cdkey", "application/vnd.mediastation.cdkey"},
            {".mwf", "application/vnd.mfer"},
            {".mfm", "application/vnd.mfmp"},
            {".msh", "model/mesh"},
            {".mads", "application/mads+xml"},
            {".mets", "application/mets+xml"},
            {".mods", "application/mods+xml"},
            {".meta4", "application/metalink4+xml"},
            {".potm", "application/vnd.ms-powerpoint.template.macroenabled.12"},
            {".docm", "application/vnd.ms-word.document.macroenabled.12"},
            {".dotm", "application/vnd.ms-word.template.macroenabled.12"},
            {".mcd", "application/vnd.mcd"},
            {".flo", "application/vnd.micrografx.flo"},
            {".igx", "application/vnd.micrografx.igx"},
            {".es3", "application/vnd.eszigno3+xml"},
            {".mdb", "application/x-msaccess"},
            {".asf", "video/x-ms-asf"},
            {".exe", "application/x-msdownload"},
            {".cil", "application/vnd.ms-artgalry"},
            {".cab", "application/vnd.ms-cab-compressed"},
            {".ims", "application/vnd.ms-ims"},
            {".application", "application/x-ms-application"},
            {".clp", "application/x-msclip"},
            {".mdi", "image/vnd.ms-modi"},
            {".eot", "application/vnd.ms-fontobject"},
            {".xls", "application/vnd.ms-excel"},
            {".xlam", "application/vnd.ms-excel.addin.macroenabled.12"},
            {".xlsb", "application/vnd.ms-excel.sheet.binary.macroenabled.12"},
            {".xltm", "application/vnd.ms-excel.template.macroenabled.12"},
            {".xlsm", "application/vnd.ms-excel.sheet.macroenabled.12"},
            {".chm", "application/vnd.ms-htmlhelp"},
            {".crd", "application/x-mscardfile"},
            {".lrm", "application/vnd.ms-lrm"},
            {".mvb", "application/x-msmediaview"},
            {".mny", "application/x-msmoney"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
            {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
            {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
            {".obd", "application/x-msbinder"},
            {".thmx", "application/vnd.ms-officetheme"},
            {".onetoc", "application/onenote"},
            {".pya", "audio/vnd.ms-playready.media.pya"},
            {".pyv", "video/vnd.ms-playready.media.pyv"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".ppam", "application/vnd.ms-powerpoint.addin.macroenabled.12"},
            {".sldm", "application/vnd.ms-powerpoint.slide.macroenabled.12"},
            {".pptm", "application/vnd.ms-powerpoint.presentation.macroenabled.12"},
            {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroenabled.12"},
            {".mpp", "application/vnd.ms-project"},
            {".pub", "application/x-mspublisher"},
            {".scd", "application/x-msschedule"},
            {".xap", "application/x-silverlight-app"},
            {".stl", "application/vnd.ms-pki.stl"},
            {".cat", "application/vnd.ms-pki.seccat"},
            {".vsd", "application/vnd.visio"},
            {".wm", "video/x-ms-wm"},
            {".wma", "audio/x-ms-wma"},
            {".wax", "audio/x-ms-wax"},
            {".wmx", "video/x-ms-wmx"},
            {".wmd", "application/x-ms-wmd"},
            {".wpl", "application/vnd.ms-wpl"},
            {".wmz", "application/x-ms-wmz"},
            {".wmv", "video/x-ms-wmv"},
            {".wvx", "video/x-ms-wvx"},
            {".wmf", "application/x-msmetafile"},
            {".trm", "application/x-msterminal"},
            {".doc", "application/msword"},
            {".wri", "application/x-mswrite"},
            {".wps", "application/vnd.ms-works"},
            {".xbap", "application/x-ms-xbap"},
            {".xps", "application/vnd.ms-xpsdocument"},
            {".mid", "audio/midi"},
            {".mpy", "application/vnd.ibm.minipay"},
            {".afp", "application/vnd.ibm.modcap"},
            {".rms", "application/vnd.jcp.javame.midlet-rms"},
            {".tmo", "application/vnd.tmobile-livetv"},
            {".prc", "application/x-mobipocket-ebook"},
            {".mbk", "application/vnd.mobius.mbk"},
            {".dis", "application/vnd.mobius.dis"},
            {".plc", "application/vnd.mobius.plc"},
            {".mqy", "application/vnd.mobius.mqy"},
            {".msl", "application/vnd.mobius.msl"},
            {".txf", "application/vnd.mobius.txf"},
            {".daf", "application/vnd.mobius.daf"},
            {".fly", "text/vnd.fly"},
            {".mpc", "application/vnd.mophun.certificate"},
            {".mpn", "application/vnd.mophun.application"},
            {".mj2", "video/mj2"},
            {".mpga", "audio/mpeg"},
            {".mxu", "video/vnd.mpegurl"},
            {".mpeg", "video/mpeg"},
            {".m21", "application/mp21"},
            {".mp4a", "audio/mp4"},
            {".mp4", "video/mp4"},
            {".m3u8", "application/vnd.apple.mpegurl"},
            {".mus", "application/vnd.musician"},
            {".msty", "application/vnd.muvee.style"},
            {".mxml", "application/xv+xml"},
            {".ngdat", "application/vnd.nokia.n-gage.data"},
            {".n-gage", "application/vnd.nokia.n-gage.symbian.install"},
            {".ncx", "application/x-dtbncx+xml"},
            {".nc", "application/x-netcdf"},
            {".nlu", "application/vnd.neurolanguage.nlu"},
            {".dna", "application/vnd.dna"},
            {".nnd", "application/vnd.noblenet-directory"},
            {".nns", "application/vnd.noblenet-sealer"},
            {".nnw", "application/vnd.noblenet-web"},
            {".rpst", "application/vnd.nokia.radio-preset"},
            {".rpss", "application/vnd.nokia.radio-presets"},
            {".n3", "text/n3"},
            {".edm", "application/vnd.novadigm.edm"},
            {".edx", "application/vnd.novadigm.edx"},
            {".ext", "application/vnd.novadigm.ext"},
            {".gph", "application/vnd.flographit"},
            {".ecelp4800", "audio/vnd.nuera.ecelp4800"},
            {".ecelp7470", "audio/vnd.nuera.ecelp7470"},
            {".ecelp9600", "audio/vnd.nuera.ecelp9600"},
            {".oda", "application/oda"},
            {".ogx", "application/ogg"},
            {".oga", "audio/ogg"},
            {".ogv", "video/ogg"},
            {".dd2", "application/vnd.oma.dd2+xml"},
            {".oth", "application/vnd.oasis.opendocument.text-web"},
            {".opf", "application/oebps-package+xml"},
            {".qbo", "application/vnd.intu.qbo"},
            {".oxt", "application/vnd.openofficeorg.extension"},
            {".osf", "application/vnd.yamaha.openscoreformat"},
            {".weba", "audio/webm"},
            {".webm", "video/webm"},
            {".odc", "application/vnd.oasis.opendocument.chart"},
            {".otc", "application/vnd.oasis.opendocument.chart-template"},
            {".odb", "application/vnd.oasis.opendocument.database"},
            {".odf", "application/vnd.oasis.opendocument.formula"},
            {".odft", "application/vnd.oasis.opendocument.formula-template"},
            {".odg", "application/vnd.oasis.opendocument.graphics"},
            {".otg", "application/vnd.oasis.opendocument.graphics-template"},
            {".odi", "application/vnd.oasis.opendocument.image"},
            {".oti", "application/vnd.oasis.opendocument.image-template"},
            {".odp", "application/vnd.oasis.opendocument.presentation"},
            {".otp", "application/vnd.oasis.opendocument.presentation-template"},
            {".ods", "application/vnd.oasis.opendocument.spreadsheet"},
            {".ots", "application/vnd.oasis.opendocument.spreadsheet-template"},
            {".odt", "application/vnd.oasis.opendocument.text"},
            {".odm", "application/vnd.oasis.opendocument.text-master"},
            {".ott", "application/vnd.oasis.opendocument.text-template"},
            {".ktx", "image/ktx"},
            {".sxc", "application/vnd.sun.xml.calc"},
            {".stc", "application/vnd.sun.xml.calc.template"},
            {".sxd", "application/vnd.sun.xml.draw"},
            {".std", "application/vnd.sun.xml.draw.template"},
            {".sxi", "application/vnd.sun.xml.impress"},
            {".sti", "application/vnd.sun.xml.impress.template"},
            {".sxm", "application/vnd.sun.xml.math"},
            {".sxw", "application/vnd.sun.xml.writer"},
            {".sxg", "application/vnd.sun.xml.writer.global"},
            {".stw", "application/vnd.sun.xml.writer.template"},
            {".otf", "application/x-font-otf"},
            {".osfpvg", "application/vnd.yamaha.openscoreformat.osfpvg+xml"},
            {".dp", "application/vnd.osgi.dp"},
            {".pdb", "application/vnd.palm"},
            {".p", "text/x-pascal"},
            {".paw", "application/vnd.pawaafile"},
            {".pclxl", "application/vnd.hp-pclxl"},
            {".efif", "application/vnd.picsel"},
            {".pcx", "image/x-pcx"},
            {".psd", "image/vnd.adobe.photoshop"},
            {".prf", "application/pics-rules"},
            {".pic", "image/x-pict"},
            {".chat", "application/x-chat"},
            {".p10", "application/pkcs10"},
            {".p12", "application/x-pkcs12"},
            {".p7m", "application/pkcs7-mime"},
            {".p7s", "application/pkcs7-signature"},
            {".p7r", "application/x-pkcs7-certreqresp"},
            {".p7b", "application/x-pkcs7-certificates"},
            {".p8", "application/pkcs8"},
            {".plf", "application/vnd.pocketlearn"},
            {".pnm", "image/x-portable-anymap"},
            {".pbm", "image/x-portable-bitmap"},
            {".pcf", "application/x-font-pcf"},
            {".pfr", "application/font-tdpfr"},
            {".pgn", "application/x-chess-pgn"},
            {".pgm", "image/x-portable-graymap"},
            {".png", "image/png"},
            {".ppm", "image/x-portable-pixmap"},
            {".pskcxml", "application/pskc+xml"},
            {".pml", "application/vnd.ctc-posml"},
            {".ai", "application/postscript"},
            {".pfa", "application/x-font-type1"},
            {".pbd", "application/vnd.powerbuilder6"},
            {".pgp", "application/pgp-signature"},
            {".box", "application/vnd.previewsystems.box"},
            {".ptid", "application/vnd.pvi.ptid1"},
            {".pls", "application/pls+xml"},
            {".str", "application/vnd.pg.format"},
            {".ei6", "application/vnd.pg.osasli"},
            {".dsc", "text/prs.lines.tag"},
            {".psf", "application/x-font-linux-psf"},
            {".qps", "application/vnd.publishare-delta-tree"},
            {".wg", "application/vnd.pmi.widget"},
            {".qxd", "application/vnd.quark.quarkxpress"},
            {".esf", "application/vnd.epson.esf"},
            {".msf", "application/vnd.epson.msf"},
            {".ssf", "application/vnd.epson.ssf"},
            {".qam", "application/vnd.epson.quickanime"},
            {".qfx", "application/vnd.intu.qfx"},
            {".qt", "video/quicktime"},
            {".rar", "application/x-rar-compressed"},
            {".ram", "audio/x-pn-realaudio"},
            {".rmp", "audio/x-pn-realaudio-plugin"},
            {".rsd", "application/rsd+xml"},
            {".rm", "application/vnd.rn-realmedia"},
            {".bed", "application/vnd.realvnc.bed"},
            {".mxl", "application/vnd.recordare.musicxml"},
            {".musicxml", "application/vnd.recordare.musicxml+xml"},
            {".rnc", "application/relax-ng-compact-syntax"},
            {".rdz", "application/vnd.data-vision.rdz"},
            {".rdf", "application/rdf+xml"},
            {".rp9", "application/vnd.cloanto.rp9"},
            {".jisp", "application/vnd.jisp"},
            {".rtf", "application/rtf"},
            {".rtx", "text/richtext"},
            {".link66", "application/vnd.route66.link66+xml"},
            {".rss, .xml", "application/rss+xml"},
            {".shf", "application/shf+xml"},
            {".st", "application/vnd.sailingtracker.track"},
            {".svg", "image/svg+xml"},
            {".sus", "application/vnd.sus-calendar"},
            {".sru", "application/sru+xml"},
            {".setpay", "application/set-payment-initiation"},
            {".setreg", "application/set-registration-initiation"},
            {".sema", "application/vnd.sema"},
            {".semd", "application/vnd.semd"},
            {".semf", "application/vnd.semf"},
            {".see", "application/vnd.seemail"},
            {".snf", "application/x-font-snf"},
            {".spq", "application/scvp-vp-request"},
            {".spp", "application/scvp-vp-response"},
            {".scq", "application/scvp-cv-request"},
            {".scs", "application/scvp-cv-response"},
            {".sdp", "application/sdp"},
            {".etx", "text/x-setext"},
            {".movie", "video/x-sgi-movie"},
            {".ifm", "application/vnd.shana.informed.formdata"},
            {".itp", "application/vnd.shana.informed.formtemplate"},
            {".iif", "application/vnd.shana.informed.interchange"},
            {".ipk", "application/vnd.shana.informed.package"},
            {".tfi", "application/thraud+xml"},
            {".shar", "application/x-shar"},
            {".rgb", "image/x-rgb"},
            {".slt", "application/vnd.epson.salt"},
            {".aso", "application/vnd.accpac.simply.aso"},
            {".imp", "application/vnd.accpac.simply.imp"},
            {".twd", "application/vnd.simtech-mindmapper"},
            {".csp", "application/vnd.commonspace"},
            {".saf", "application/vnd.yamaha.smaf-audio"},
            {".mmf", "application/vnd.smaf"},
            {".spf", "application/vnd.yamaha.smaf-phrase"},
            {".teacher", "application/vnd.smart.teacher"},
            {".svd", "application/vnd.svd"},
            {".rq", "application/sparql-query"},
            {".srx", "application/sparql-results+xml"},
            {".gram", "application/srgs"},
            {".grxml", "application/srgs+xml"},
            {".ssml", "application/ssml+xml"},
            {".skp", "application/vnd.koan"},
            {".sgml", "text/sgml"},
            {".sdc", "application/vnd.stardivision.calc"},
            {".sda", "application/vnd.stardivision.draw"},
            {".sdd", "application/vnd.stardivision.impress"},
            {".smf", "application/vnd.stardivision.math"},
            {".sdw", "application/vnd.stardivision.writer"},
            {".sgl", "application/vnd.stardivision.writer-global"},
            {".sm", "application/vnd.stepmania.stepchart"},
            {".sit", "application/x-stuffit"},
            {".sitx", "application/x-stuffitx"},
            {".sdkm", "application/vnd.solent.sdkm+xml"},
            {".xo", "application/vnd.olpc-sugar"},
            {".au", "audio/basic"},
            {".wqd", "application/vnd.wqd"},
            {".sis", "application/vnd.symbian.install"},
            {".smi", "application/smil+xml"},
            {".xsm", "application/vnd.syncml+xml"},
            {".bdm", "application/vnd.syncml.dm+wbxml"},
            {".xdm", "application/vnd.syncml.dm+xml"},
            {".sv4cpio", "application/x-sv4cpio"},
            {".sv4crc", "application/x-sv4crc"},
            {".sbml", "application/sbml+xml"},
            {".tsv", "text/tab-separated-values"},
            {".tiff", "image/tiff"},
            {".tao", "application/vnd.tao.intent-module-archive"},
            {".tar", "application/x-tar"},
            {".tcl", "application/x-tcl"},
            {".tex", "application/x-tex"},
            {".tfm", "application/x-tex-tfm"},
            {".tei", "application/tei+xml"},
            {".txt", "text/plain"},
            {".dxp", "application/vnd.spotfire.dxp"},
            {".sfs", "application/vnd.spotfire.sfs"},
            {".tsd", "application/timestamped-data"},
            {".tpt", "application/vnd.trid.tpt"},
            {".mxs", "application/vnd.triscape.mxs"},
            {".t", "text/troff"},
            {".tra", "application/vnd.trueapp"},
            {".ttf", "application/x-font-ttf"},
            {".ttl", "text/turtle"},
            {".umj", "application/vnd.umajin"},
            {".uoml", "application/vnd.uoml+xml"},
            {".unityweb", "application/vnd.unity"},
            {".ufd", "application/vnd.ufdl"},
            {".uri", "text/uri-list"},
            {".utz", "application/vnd.uiq.theme"},
            {".ustar", "application/x-ustar"},
            {".uu", "text/x-uuencode"},
            {".vcs", "text/x-vcalendar"},
            {".vcf", "text/x-vcard"},
            {".vcd", "application/x-cdlink"},
            {".vsf", "application/vnd.vsf"},
            {".wrl", "model/vrml"},
            {".vcx", "application/vnd.vcx"},
            {".mts", "model/vnd.mts"},
            {".vtu", "model/vnd.vtu"},
            {".vis", "application/vnd.visionary"},
            {".viv", "video/vnd.vivo"},
            {".ccxml", "application/ccxml+xml,"},
            {".vxml", "application/voicexml+xml"},
            {".src", "application/x-wais-source"},
            {".wbxml", "application/vnd.wap.wbxml"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wav", "audio/x-wav"},
            {".davmount", "application/davmount+xml"},
            {".woff", "application/x-font-woff"},
            {".wspolicy", "application/wspolicy+xml"},
            {".webp", "image/webp"},
            {".wtb", "application/vnd.webturbo"},
            {".wgt", "application/widget"},
            {".hlp", "application/winhlp"},
            {".wml", "text/vnd.wap.wml"},
            {".wmls", "text/vnd.wap.wmlscript"},
            {".wmlsc", "application/vnd.wap.wmlscriptc"},
            {".wpd", "application/vnd.wordperfect"},
            {".stf", "application/vnd.wt.stf"},
            {".wsdl", "application/wsdl+xml"},
            {".xbm", "image/x-xbitmap"},
            {".xpm", "image/x-xpixmap"},
            {".xwd", "image/x-xwindowdump"},
            {".der", "application/x-x509-ca-cert"},
            {".fig", "application/x-xfig"},
            {".xhtml", "application/xhtml+xml"},
            {".xml", "application/xml"},
            {".xdf", "application/xcap-diff+xml"},
            {".xenc", "application/xenc+xml"},
            {".xer", "application/patch-ops-error+xml"},
            {".rl", "application/resource-lists+xml"},
            {".rs", "application/rls-services+xml"},
            {".rld", "application/resource-lists-diff+xml"},
            {".xslt", "application/xslt+xml"},
            {".xop", "application/xop+xml"},
            {".xpi", "application/x-xpinstall"},
            {".xspf", "application/xspf+xml"},
            {".xul", "application/vnd.mozilla.xul+xml"},
            {".xyz", "chemical/x-xyz"},
            {".yaml", "text/yaml"},
            {".yang", "application/yang"},
            {".yin", "application/yin+xml"},
            {".zir", "application/vnd.zul"},
            {".zip", "application/zip"},
            {".zmm", "application/vnd.handheld-entertainment+xml"},
            {".zaz", "application/vnd.zzazz.deck+xml"}
        };
        //-----------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------


    }
}