using System;
using System.Windows;
using Microsoft.Azure.Storage.Auth;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using System.Diagnostics;


/// <summary>
/// --------------------- Azure Authentication - Service Proncipal -------------------------------------------
/// How to: Use the portal to create an Azure AD application and service principal that can access resources
/// https://docs.microsoft.com/pt-pt/azure/active-directory/develop/howto-create-service-principal-portal
///
/// --------------------- Azure Authentication - Microsoft Graph API -----------------------------------------
/// Call the Microsoft Graph API from a Windows Desktop app
/// https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-windows-desktop
/// Register the application:
/// https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-windows-desktop#register-your-application
/// Add the code to initialize MSAL
/// https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-windows-desktop#add-the-code-to-initialize-msal
/// Related docs:
/// Service-to-service authentication to Azure Key Vault using .NET
/// https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication#using-the-library
///
/// ------------------------------------- Azure Authentication - Managed Identities ---------------------------- 
/// https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad?toc=%2fazure%2fstorage%2fblobs%2ftoc.json#overview-of-azure-ad-for-blobs-and-queues
/// When a security principal(a user, group, or application) attempts to access a blob or queue resource, the request must be authorized, unless it is a blob available for anonymous access.
/// With Azure AD, access to a resource is a two-step process. 
/// First, the security principal's identity is authenticated and an OAuth 2.0 token is returned. 
/// Next, the token is passed as part of a request to the Blob or Queue service and used by the service to authorize access to the specified resource.
/// 
/// The authentication step requires that an application request an OAuth 2.0 access token at runtime. 
/// If an application is running from within an Azure entity such as an Azure VM, a virtual machine scale set, or an Azure Functions app,
/// It can use a managed identity to access blobs or queues
/// 
/// Enable managed identities on a VM - TODO - why is working without VM ???
/// https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-msi?toc=%2fazure%2fstorage%2fblobs%2ftoc.json#enable-managed-identities-on-a-vm
/// Authorize access to blobs and queues with Azure Active Directory and managed identities for Azure Resources
/// https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-msi?toc=%2fazure%2fstorage%2fblobs%2ftoc.json#net-code-example-create-a-block-blob
/// ---------------------------------------------------------------------------------------------------------------
/// </summary>

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
///---------------------------------------------------------------------------------------------------------
/// Create a service SAS
/// https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-service-sas
///   Delegates access to a resource in just one of the storage services: the Blob, Queue, Table, or File service.
///   A SAS may also specify the supported IP address or address range from which requests can originate
///   the supported protocol with which a request can be made
///   an optional access policy identifier associated with the request.
///---------------------------------------------------------------------------------------------------------
/// </summary>

namespace Storage_Helper_SAS_Tool
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //----------------------------------------------------------------------------
        private string StorageSDK_11_Version = "v11.0.0";
        private string StorageSDK_12_Version = "v12.0.0-preview.3";

        public StorageCredentials storageCredentials;


        //----------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Populate ComboBoxes with values of Service Version Arrays (defined on SAS_Utils)
            SAS_Utils.PopulateComboBox_sv(ComboBox_sv, "", "");

            // Splash Info
            SplashInfo();
        }
        



        /// <summary>
        /// Splash Information
        /// </summary>
        private void SplashInfo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            //-----------------------------------------------------------
            // GitHub API - Get a single release details
            // https://developer.github.com/v3/repos/releases/#get-a-single-release
            //
            // GitHub release counters
            // https://shields.io/category/downloads
            //
            // GitHub - Get a single Release by Tag name
            // https://api.github.com/repos/LuisFilipe236/Storage-Helper-SAS-Tool/releases/tags/v1.0.1
            //-----------------------------------------------------------

            BoxAuthResults_Right.Text = "x\n\n"; // 'x' used to remove the splash on click
            BoxAuthResults_Right.Text += "Storage Helper SAS Tool - version " + version + " (beta)\n";
            BoxAuthResults_Right.Text += "Developed by: Azure Storage Support Team\n";
            BoxAuthResults_Right.Text += "-------------------------------------------------------------------------------------------------\n";
            BoxAuthResults_Right.Text += "\n";

            BoxAuthResults_Right.Text += "DISCLAIMER: \n";
            BoxAuthResults_Right.Text += "This application is to test proposes only, runs only locally and don't use the provided information for any other proposes other than checking the Storage SAS parameters locally.\n";
            BoxAuthResults_Right.Text += "This application don't save any information on any place and does not use the Storage Account Key or the Storage Account SAS to access the storage resources on any way.\n";
            BoxAuthResults_Right.Text += "You can run this application or the source code at your own risk.\n";
            BoxAuthResults_Right.Text += "\n";

            BoxAuthResults_Right.Text += "HOW TO USE:\n";
            BoxAuthResults_Right.Text += "Checking SAS parameters:\n";
            BoxAuthResults_Right.Text += "Insert the Storage Account SAS or Connection String in the box on the top, and use the 'Check SAS Parameters' button to validate all the fields.\n";
            BoxAuthResults_Right.Text += "Check the results on the left bottom box. The parameter boxes will be filled with the readed parameters.\n";
            BoxAuthResults_Right.Text += "Any errors will be marked with '-->' and the parameter boxes in red.\n";
            BoxAuthResults_Right.Text += "\n";

            BoxAuthResults_Right.Text += "Regenerating SAS:\n";
            BoxAuthResults_Right.Text += "Use the 'Regenerate SAS' button to create a new Storage Account SAS, based on the values in the parameter Boxes.\n";
            BoxAuthResults_Right.Text += "To do that the Storage Account Key is needed. All other mandatory values not provided will be asked and marked in red.\n";
            BoxAuthResults_Right.Text += "The results with the new generated Storage Account SAS will be displayed on the right bottom box.\n";
            BoxAuthResults_Right.Text += "Use the two 'Signature' boxes to compare the two signatures, if you wish to compare your 'sig' with the generated 'sig'.\n";
            BoxAuthResults_Right.Text += "\n";

            BoxAuthResults_Right.Text += "To see this help splash again you can click on the '?' at the top right.\n";
            BoxAuthResults_Right.Text += "The source code are public available on GitHub at https://github.com/LuisFilipe236/Storage-Helper-SAS-Tool\n";
            BoxAuthResults_Right.Text += "The .msi installer package is available on GitHub (use 'Download' button) at https://github.com/LuisFilipe236/Storage-Helper-SAS-Tool/raw/master/Storage%20Helper%20SAS%20Tool%20Installer/Debug/Storage%20Helper%20SAS%20Tool%20Installer.msi\n";
            BoxAuthResults_Right.Text += "\n";

            BoxAuthResults_Right.Text += "-------------------------------------------------------------------------------------------------\n";
            BoxAuthResults_Right.Text += "==> TODO:";
            BoxAuthResults_Right.Text += "     - Remove the optional not used paramenters from the SAS string, on SDK v11\n";
            BoxAuthResults_Right.Text += "     - In Service Versions before 2012-02-12, the duration between signedstart and signedexpiry cannot exceed one hour\n";
            BoxAuthResults_Right.Text += "     - Check the se alredy exired on regeneration time\n";
        }






        private void RemoveSplash()
        {
            if (BoxAuthResults_Right.Text.StartsWith("x"))
                BoxAuthResults_Right.Text = "";
        }



 

        /// <summary>
        /// Test parameters on provided Account SAS
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/Constructing-an-Account-SAS?redirectedfrom=MSDN#constructing-the-account-sas-uri
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTestAccountSAS_Click(object sender, RoutedEventArgs e)
        {
            RemoveSplash();

            new SAS_Utils().SAS_Validate(System.Uri.UnescapeDataString(InputBoxSAS.Text.Trim()), BoxAuthResults);   // unscaped and removing leading and trailling whitespaces

            if(InputBoxSAS.Text == "") labelInsertSAS.Foreground = Brushes.Red;

            Set_ValuesFromStruct_ToBoxes();

            if (SAS_Utils.Show_Services() != "") return;

            Set_Color_Signature();
        }






        private void ButtonRegenerateSAS_Click(object sender, RoutedEventArgs e)
        {
            RemoveSplash();
            labelMessages.Content = ""; // Clean messages at bottom

            if (Utils.StringEmpty(labelAccountKey, textBoxAccountKey1.Text, "Please provide the Storage Account Key to regenerate a SAS", "Error"))
                return;

            if (Utils.StringEmpty(labelAccountName, textBoxAccountName.Text, "Missing Account Name", "Error"))
            { SAS_Utils.SAS.containerName.s = false; return; }

            if (textBoxAccountName.Text == "not found")
            {
                labelAccountName.Foreground = Brushes.Red;
                MessageBox.Show("Missing Account Name", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                SAS_Utils.SAS.containerName.s = false;
                return;
            }


            if (ComboBox_srt.Text == "" && ComboBox_sr.Text == "" && textBox_tn.Text == "")
            {
                label_srt.Foreground = Brushes.Red;
                label_sr.Foreground = Brushes.Red;
                label_tn.Foreground = Brushes.Red;
                MessageBox.Show("Please provide the 'Signed Resource Type' for an account SAS, or 'Signed Resource' or 'Table Name' for a Service SAS", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }


            if ((ComboBox_sr.Text == "s" || ComboBox_sr.Text == "f") && ComboBox_sp.Text.IndexOf("l") == -1)
            {
                label_sp.Foreground = Brushes.Red;
                MessageBox.Show("Please provide at least 'l' on Signed Permissions", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }


            if (ComboBox_ss.Text == "" && ComboBox_srt.Text != "")
            {
                label_ss.Foreground = Brushes.Red;
                MessageBox.Show("Please provide the Signed Services to regenerate a Account SAS", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (Utils.StringEmpty(label_sp, ComboBox_sp.Text, "Please provide at least " + (ComboBox_sr.Text == "c" || ComboBox_sr.Text == "s" ? "'l'" : "'r'") + " Signed Permissions to regenerate a SAS", "Error"))
                return;

            if (Utils.StringEmpty(label_sv, ComboBox_sv.Text, "Please provide the Service Version to regenerate a SAS", "Error"))
                return;

            // -------------------------- Check start and Expiry date format ------------------------------
            if (Utils.StringEmpty(label_se, textBox_se.Text, "Please provide the Signed Expiry Date/Time to regenerate a SAS", "Error"))
                return;

            if (textBox_se.Text.Length != 20 && textBox_se.Text.Length != 17 && textBox_se.Text.Length != 10)
                label_se.Foreground = Brushes.Red;

            if (textBox_st.Text != "" && textBox_st.Text.Length != 20 && textBox_st.Text.Length != 17 && textBox_st.Text.Length != 10)
                label_st.Foreground = Brushes.Red;

            if (label_se.Foreground == Brushes.Red && label_st.Foreground != Brushes.Red)
            {
                MessageBox.Show("Incorrect format on Signed Expiry Date/Time ", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (label_se.Foreground != Brushes.Red && label_st.Foreground == Brushes.Red)
            {
                MessageBox.Show("Incorrect format on Signed Start Date/Time ", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (label_se.Foreground == Brushes.Red && label_st.Foreground == Brushes.Red)
            {
                MessageBox.Show("Incorrect format on Signed Start and Signed Expiry Date/Time ", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }


            // Service SAS - In versions before 2012-02-12, the duration between signedstart and signedexpiry cannot exceed one hour unless a container policy is used.
            if ((ComboBox_sr.Text != "" || textBox_tn.Text != "") && String.Compare(ComboBox_sv.Text, "2012-02-12") < 0)
            {
                DateTime st;
                if (textBox_st.Text == "")
                    st = DateTime.Now.ToUniversalTime();
                else
                    st = Convert.ToDateTime(textBox_st.Text).ToUniversalTime();

                DateTime se = Convert.ToDateTime(textBox_se.Text).ToUniversalTime();
                TimeSpan ts = (se - st);

                if (ts.Hours>1 || (ts.Hours==1 && (ts.Minutes > 0 || ts.Seconds>0)))        // difference beetween se and st with more than 1 hour
                {
                    label_st.Foreground = Brushes.Red;
                    MessageBox.Show("Service SAS - In Service Versions before 2012-02-12, the duration between signedstart and signedexpiry cannot exceed one hour unless a container policy is used.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            


            //-----------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------

            // Copy the values From Combo and Text Boxes To Struct, to be used to regenerate the SAS
            SAS_Utils.init_SASstruct();
            Get_ValuesFromBoxes_ToStruct();

            // Validate dates
            if (!DateTimeUtils.Validate_DateTimes(label_st, label_se))
                return;

            // Validate IPs
            string s = SAS_Utils.Validate_Sip();
            if (s != "")
                MessageBox.Show(s, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);


            // Choose between SDK v11 or v12_preview
            //------------------------------------------------
            if (checkBoxPreRelease12.IsChecked==true)
                RegenerateSAS_SDKv12_preview();
            else
                RegenerateSAS_SDKv11();
            //------------------------------------------------

            textBox_sig_right.Text = SAS_Utils.SAS.sig;

            // Change the textBox_signature_right according to the value of the textBox_signature_left
            Set_Color_Signature();
        }


        /// <summary>
        /// Regenerate SAS using Storage SDK v11.0.0
        /// </summary>
        private void RegenerateSAS_SDKv11()
        {
            // Regenerate Account SAS (srt) from SAS structure values
            if (SAS_Utils.SAS.srt.v != "not found" && SAS_Utils.SAS.srt.v != "")
                SAS_Create_v11.Regenerate_AccountSAS(textBoxAccountName, textBoxAccountKey1, BoxAuthResults_Right, ComboBox_ss.Text);

            // Regenerate Service SAS (sr) from SAS structure values (blob, container, file, share, blob Snapshot, Queue ??)
            if (SAS_Utils.SAS.sr.v != "not found" && SAS_Utils.SAS.sr.v != "")
                switch (SAS_Utils.SAS.sr.v)   // service SAS
                {
                    case "b":   // blob
                        SAS_Create_v11.Regenerate_ServiceSAS_Blob(labelContainerName, labelBlobName, textBoxAccountName, textBoxAccountKey1, textBoxContainerName, textBoxBlobName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "c":   // container
                        SAS_Create_v11.Regenerate_ServiceSAS_Container(labelContainerName, textBoxAccountName, textBoxAccountKey1, textBoxContainerName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "f":   // file
                        SAS_Create_v11.Regenerate_ServiceSAS_File(labelShareName, labelFileName, textBoxAccountName, textBoxAccountKey1, textBoxShareName, textBoxFileName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "s":   // share
                        SAS_Create_v11.Regenerate_ServiceSAS_Share(labelShareName, textBoxAccountName, textBoxAccountKey1, textBoxShareName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "bs":  // Blob Shapshot
                        SAS_Create_v11.Regenerate_ServiceSAS_BlobSnapshot(BoxAuthResults_Right);   // <-- TODO - Not implemeted - lack on documentation
                        break;
                    case "??":  // Queue Shapshot               // <-- TODO - how identify Queue - not supported, but CloudQueue.GetSharedAccessSignature(policy) exists ??? - lack on documentation
                        SAS_Create_v11.Regenerate_ServiceSAS_Queue(labelQueueName, textBoxAccountName, textBoxAccountKey1, textBoxQueueName, textBox_si, BoxAuthResults_Right);
                        break;
                }

            // Regenerate Service SAS (tn) from SAS structure values (table only) - uses Microsoft.Azure.Cosmos.Table library
            if (SAS_Utils.SAS.tn.v != "not found" && SAS_Utils.SAS.tn.v != "")
                SAS_Create_v11.Regenerate_ServiceSAS_Table(labelTableName, textBoxAccountName, textBoxAccountKey1, textBoxTableName, textBox_si, BoxAuthResults_Right);

            BoxAuthResults_Right.Text += "-------------------------------------------------\n";
            BoxAuthResults_Right.Text += "Regenerated using Storage SDK " + StorageSDK_11_Version + "\n";
        }


        /// <summary>
        /// Regenerate SAS using Storage SDK v12.0.0_Preview
        /// </summary>
        private void RegenerateSAS_SDKv12_preview()
        {
            // Regenerate Account SAS (srt) from SAS structure values
            if (SAS_Utils.SAS.srt.v != "not found" && SAS_Utils.SAS.srt.v != "")
                SAS_Create_v12.Regenerate_AccountSAS(textBoxAccountName, textBoxAccountKey1, BoxAuthResults_Right, ComboBox_ss.Text);

            // Regenerate Service SAS (sr) from SAS structure values (blob, container, file, share, blob Snapshot, Queue ??)
            if (SAS_Utils.SAS.sr.v != "not found" && SAS_Utils.SAS.sr.v != "")
                switch (SAS_Utils.SAS.sr.v)   // service SAS
                {
                    case "b":   // blob
                        SAS_Create_v12.Regenerate_ServiceSAS_Blob(labelContainerName, labelBlobName, textBoxAccountName, textBoxAccountKey1, textBoxContainerName, textBoxBlobName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "c":   // container
                        SAS_Create_v12.Regenerate_ServiceSAS_Container(labelContainerName, textBoxAccountName, textBoxAccountKey1, textBoxContainerName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "bs":  // Blob Shapshot
                        SAS_Create_v12.Regenerate_ServiceSAS_BlobSnapshot(labelContainerName, labelBlobSnapshotName, textBoxAccountName, textBoxAccountKey1, textBoxContainerName, textBoxBlobSnapshotName, textBox_si, BoxAuthResults_Right);   
                        break;
                    case "f":   // file
                        SAS_Create_v12.Regenerate_ServiceSAS_File(labelShareName, labelFileName, textBoxAccountName, textBoxAccountKey1, textBoxShareName, textBoxFileName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "s":   // share
                        SAS_Create_v12.Regenerate_ServiceSAS_Share(labelShareName, textBoxAccountName, textBoxAccountKey1, textBoxShareName, textBox_si, BoxAuthResults_Right);
                        break;
                    case "??":  // Queue Shapshot               // <-- TODO - how identify Queue - not supported, but CloudQueue.GetSharedAccessSignature(policy) exists ??? - lack on documentation
                        SAS_Create_v12.Regenerate_ServiceSAS_Queue(labelQueueName, textBoxAccountName, textBoxAccountKey1, textBoxQueueName, textBox_si, BoxAuthResults_Right);
                        break;
                }

            // Regenerate Table Service SAS not supported - uses Microsoft.Azure.Cosmos.Table library
            if (SAS_Utils.SAS.tn.v != "not found" && SAS_Utils.SAS.tn.v != "")
                MessageBox.Show("Regenerate Table Service SAS not supported on this SDK version (TODO)", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            BoxAuthResults_Right.Text += "-------------------------------------------------\n";
            BoxAuthResults_Right.Text += "Regenerated using Storage SDK "+StorageSDK_12_Version+"\n";

        }

        /// <summary>
        /// null - change color on both TextBoxes
        /// </summary>
        private void Set_Color_Signature()
        {
            if(textBox_sig_left.Text == "" || textBox_sig_right.Text == "")     
            {
                textBox_sig_right.Foreground = Brushes.Black;
                textBox_sig_left.Foreground  = Brushes.Black;
                return;
            }

            if (textBox_sig_right.Text == textBox_sig_left.Text)
            { 
                textBox_sig_right.Foreground = Brushes.Green;
                textBox_sig_left.Foreground  = Brushes.Green;
            }
            else
            {
                textBox_sig_right.Foreground = Brushes.Red;
                textBox_sig_left.Foreground = Brushes.Red;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// Search for 'find' in 's' and return true if found
        /// Used to fill the CheckBox when ComboBox DropDownOpened
        /// </summary>
        /// <param name="s"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        private bool Test(string s, string find)
        {
            if (s.IndexOf(find) != -1)
                return true;
            else
                return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_ss_DropDownClosed(object sender, EventArgs e)
        {
            string s = "";
            if (ss_blob.IsChecked == true) s += "b";
            if (ss_file.IsChecked == true) s += "f";
            if (ss_table.IsChecked == true) s += "t";
            if (ss_queue.IsChecked == true) s += "q";

            ComboBox_ss.Text = s;
            if (s != "")
                label_ss.Foreground = Brushes.Black;
        }

        private void ComboBox_ss_DropDownOpened(object sender, EventArgs e)
        {
            ss_blob.IsChecked = Test(ComboBox_ss.Text, "b");
            ss_file.IsChecked = Test(ComboBox_ss.Text, "f");
            ss_table.IsChecked = Test(ComboBox_ss.Text, "t");
            ss_queue.IsChecked = Test(ComboBox_ss.Text, "q");
        }




        /// <summary>
        /// sp - Signed Permissions - r,w,d,l,a,c,u,p
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_sp_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox_sp_UpdateText();
        }

        /// <summary>
        /// Update Text on ComboBox sp based on the ChecBox selections
        /// </summary>
        private void ComboBox_sp_UpdateText()
        {
            string s = "";
            if (sp_read.IsChecked == true) s += "r";
            if (sp_write.IsChecked == true) s += "w";
            if (sp_delete.IsChecked == true) s += "d";
            if (sp_list.IsChecked == true) s += "l";
            if (sp_add.IsChecked == true) s += "a";
            if (sp_create.IsChecked == true) s += "c";
            if (sp_update.IsChecked == true) s += "u";
            if (sp_process.IsChecked == true) s += "p";

            ComboBox_sp.Text = s;
            if(s != "")
                label_sp.Foreground = Brushes.Black;
        }

        private void ComboBox_sp_DropDownOpened(object sender, EventArgs e)
        {
            sp_read.IsChecked = Test(ComboBox_sp.Text, "r");
            sp_write.IsChecked = Test(ComboBox_sp.Text, "w");
            sp_delete.IsChecked = Test(ComboBox_sp.Text, "d");
            sp_list.IsChecked = Test(ComboBox_sp.Text, "l");
            sp_add.IsChecked = Test(ComboBox_sp.Text, "a");
            sp_create.IsChecked = Test(ComboBox_sp.Text, "c");
            sp_update.IsChecked = Test(ComboBox_sp.Text, "u");
            sp_process.IsChecked = Test(ComboBox_sp.Text, "p");
        }

        private void ComboBox_ss_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_DropDownClosed_1(object sender, EventArgs e)
        {
            
        }


        public string ss_WithColor(string color, string msg)
        {
            if (color=="Red")
                label_ss.Foreground = System.Windows.Media.Brushes.Red; 
            if(color=="Black")
                label_ss.Foreground = System.Windows.Media.Brushes.Black;
            return msg;
        }


        /// <summary>
        /// SignedResource    sr:           { b,c }   // blob, container - for blobs
        ///                                 { s,f }   // share, file     - for files, version 2015-02-21 and later
        ///                                 {bs}      // blob snapshot,               version 2018-11-09 and later
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_sr_DropDownClosed(object sender, EventArgs e)
        {
            string s = "";
            if (sr_blob.IsChecked == true) s += "b";
            if (sr_container.IsChecked == true) s += "c";

            if (sr_share.IsChecked == true) s += "s";
            if (sr_file.IsChecked == true) s += "f";

            //if (sr_queue.IsChecked == true) s += "q";           // TODO - check if this the correct option

            if (sr_blobSnapshot.IsChecked == true) s += "bs";   

            ComboBox_sr.Text = s;

            if (s != "")
            {
                ComboBox_apiVersion.Text = "";     // clear API-Version
                ComboBox_srt.Text = "";         // clear srt
                // ComboBox_srt.IsEnabled = false; // leaving enable to make it possible to change to Account SAS

                ComboBox_ss.Text = "";         // clear ss
                ComboBox_ss.IsEnabled = false;

                textBox_tn.Text = "";          // clear tn
                SetStatus_PartitionRowBoxes(false);

                textBox_si.IsEnabled = true;
                label_PolicyName.IsEnabled = true;

                label_srt.Foreground = Brushes.Black;
                label_sr.Foreground = Brushes.Black;
                label_tn.Foreground = Brushes.Black;

                // Setting the sp (signed permissions) based on Service SAS sr
                // https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-service-sas#permissions-for-a-blob
                switch (s)
                {
                    // blob - racwd
                    case "b":
                        sp_read.IsEnabled   = true;
                        sp_write.IsEnabled  = true;
                        sp_delete.IsEnabled = true;  // Uncheck the disable checkboxes
                        sp_list.IsEnabled   = false; sp_list.IsChecked    = false;
                        sp_add.IsEnabled    = true;
                        sp_create.IsEnabled = true;
                        sp_update.IsEnabled = false; sp_update.IsChecked  = false;
                        sp_process.IsEnabled= false; sp_process.IsChecked = false;
                        break;

                    // container - racwdl
                    case "c":
                        sp_read.IsEnabled   = true;
                        sp_write.IsEnabled  = true;
                        sp_delete.IsEnabled = true;
                        sp_list.IsEnabled   = true;
                        sp_add.IsEnabled    = true;
                        sp_create.IsEnabled = true;  // Uncheck the disable checkboxes
                        sp_update.IsEnabled = false; sp_update.IsChecked  = false;
                        sp_process.IsEnabled= false; sp_process.IsChecked = false;
                        break;

                    // file - rcwd
                    case "f":
                        sp_read.IsEnabled   = true;
                        sp_write.IsEnabled  = true;
                        sp_delete.IsEnabled = true;  // Uncheck the disable checkboxes
                        sp_list.IsEnabled   = false; sp_list.IsChecked = false;
                        sp_add.IsEnabled    = false; sp_add.IsChecked  = false;
                        sp_create.IsEnabled = true;
                        sp_update.IsEnabled = false; sp_update.IsChecked  = false;
                        sp_process.IsEnabled= false; sp_process.IsChecked = false;
                        labelMessages.Content = "Service File SAS suported on Service version 2015-02-21 and later. From Storage Explorer, seems 2015-02-21 is not suppoerted. 2015-02-21 removed from the list.";
                        break;

                    // share - rcwdl
                    case "s":
                        sp_read.IsEnabled   = true;
                        sp_write.IsEnabled  = true;
                        sp_delete.IsEnabled = true;
                        sp_list.IsEnabled   = true;  // Uncheck the disable checkboxes
                        sp_add.IsEnabled    = false; sp_add.IsChecked     = false;
                        sp_create.IsEnabled = true;
                        sp_update.IsEnabled = false; sp_update.IsChecked  = false;
                        sp_process.IsEnabled= false; sp_process.IsChecked = false;
                        labelMessages.Content = "Service Share SAS suported on Service version 2015-02-21 and later. From Storage Explorer, seems 2015-02-21 is not suppoerted. 2015-02-21 removed from the list.";

                        break;

                    // queue - raup     // TODO - q for queues ????
                    case "q":           // TODO - how specify the queue name ????
                        sp_read.IsEnabled   = true;  // Uncheck the disable checkboxes
                        sp_write.IsEnabled  = false; sp_write.IsChecked  = false;
                        sp_delete.IsEnabled = false; sp_delete.IsChecked = false;
                        sp_list.IsEnabled   = false; sp_list.IsChecked   = false;
                        sp_add.IsEnabled    = true;
                        sp_create.IsEnabled = false; sp_create.IsChecked = false;
                        sp_update.IsEnabled = true;
                        sp_process.IsEnabled= true;
                        break;

                    // blob snapshot - drw                 // v12.0.0.0-preview
                    case "bs":                          
                        sp_read.IsEnabled   = true;  // Uncheck the disable checkboxes
                        sp_write.IsEnabled  = true; 
                        sp_delete.IsEnabled = true; 
                        sp_list.IsEnabled   = false; sp_list.IsChecked   = false;
                        sp_add.IsEnabled    = false; sp_add.IsEnabled    = false;
                        sp_create.IsEnabled = false; sp_create.IsChecked = false;
                        sp_update.IsEnabled = false; sp_update.IsEnabled = false;
                        sp_process.IsEnabled= false; sp_process.IsEnabled= false;
                        break;
                }
                ComboBox_sp_UpdateText();
            }

            SAS_Utils.PopulateComboBox_sv(ComboBox_sv, ComboBox_sr.Text, textBox_tn.Text);
            SAS_Utils.PopulateComboBox_sv(ComboBox_apiVersion, ComboBox_sr.Text, textBox_tn.Text);
        }

        /// <summary>
        /// ComboBox sr - Drop Down Opened
        /// 
        /// TODO: No support for SAS response headers:
        /// To define values for certain response headers to be returned when the shared access signature is used in a request, 
        /// you can specify response headers in query parameters. 
        /// This feature is supported beginning with version 2013-08-15 for the Blob service and version 2015-02-21 for the File service. 
        /// Shared access signatures using this feature must include the sv parameter set to 2013-08-15 or later for the Blob service, 
        /// or to 2015-02-21 or later for the File service.
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/create-service-sas#specifying-query-parameters-to-override-response-headers-blob-and-file-services-only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_sr_DropDownOpened(object sender, EventArgs e)
        {
            if(checkBoxPreRelease12.IsChecked == true)
            {
                sr_blobSnapshot.IsChecked = Test(ComboBox_sr.Text, "bs");
                if (sr_blobSnapshot.IsChecked == true) return; // avoid mark 'b' and 's'
            }
            else
            {
                sr_blobSnapshot.IsChecked = false;
                sr_blobSnapshot.IsEnabled = false;
                labelMessages.Content = "Blob Snapshot is only supported by Storage SDK v12.0.0_preview";
            }

            sr_blob.IsChecked = Test(ComboBox_sr.Text, "b");
            sr_container.IsChecked = Test(ComboBox_sr.Text, "c");

            sr_share.IsChecked = Test(ComboBox_sr.Text, "s");
            sr_file.IsChecked = Test(ComboBox_sr.Text, "f");

            // sr_queue.IsChecked = Test(ComboBox_sr.Text, "q");       // TODO - to check if 'q' is the correct option
        }


        private void Sr_blob_Click(object sender, RoutedEventArgs e)
        {
            // allow only one selected at same time
            sr_container.IsChecked = false;
            sr_share.IsChecked = false;
            sr_file.IsChecked = false;
            //q  sr_queue.IsChecked = false;
            sr_blobSnapshot.IsChecked = false;
        }

        private void Sr_container_Click(object sender, RoutedEventArgs e)
        {
            // allow only one selected at same time
            sr_blob.IsChecked = false;
            sr_share.IsChecked = false;
            sr_file.IsChecked = false;
            //q  sr_queue.IsChecked = false;
            sr_blobSnapshot.IsChecked = false;
        }

        private void Sr_share_Click(object sender, RoutedEventArgs e)
        {
            // allow only one selected at same time
            sr_container.IsChecked = false;
            sr_blob.IsChecked = false;
            sr_file.IsChecked = false;
            //q  sr_queue.IsChecked = false;
            sr_blobSnapshot.IsChecked = false;
        }

        private void Sr_file_Click(object sender, RoutedEventArgs e)
        {
            // allow only one selected at same time
            sr_container.IsChecked = false;
            sr_share.IsChecked = false;
            sr_blob.IsChecked = false;
            //q  sr_queue.IsChecked = false;
            sr_blobSnapshot.IsChecked = false;
        }

        private void Sr_queue_Click(object sender, RoutedEventArgs e)
        {
            // allow only one selected at same time
            sr_container.IsChecked = false;
            sr_share.IsChecked = false;
            sr_blob.IsChecked = false;
            sr_file.IsChecked = false;
            sr_blobSnapshot.IsChecked = false;
        }

        private void Sr_blobSnapshot_Click(object sender, RoutedEventArgs e)
        {
            // allow only one selected at same time
            sr_container.IsChecked = false;
            sr_share.IsChecked = false;
            sr_file.IsChecked = false;
            //q sr_queue.IsChecked = false;
            sr_blob.IsChecked = false;
        }

        private void ComboBox_srt_DropDownClosed(object sender, EventArgs e)
        {
            string s = "";
            if (srt_service.IsChecked == true) s += "s";
            if (srt_container.IsChecked == true) s += "c";
            if (srt_object.IsChecked == true) s += "o";

            ComboBox_srt.Text = s;

            if(s != "")
            {
                ComboBox_sr.Text = "";      // clear sr
                // ComboBox_sr.IsEnabled = true;        // leaving enable to make it possible to change to Service SAS - B,C,S,F

                textBox_tn.Text = "";          // clear tn
                // textBox_tn.IsEnabled = true;        // leaving enable to make it possible to change to Service SAS - table

                ComboBox_ss.IsEnabled = true;

                SetStatus_PartitionRowBoxes(false);
                textBox_si.IsEnabled = false;
                label_PolicyName.IsEnabled = false;

                label_srt.Foreground = Brushes.Black;
                label_sr.Foreground = Brushes.Black;
                label_tn.Foreground = Brushes.Black;

                // Setting the sp (signed permissions) for Account SAS
                // https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-account-sas?redirectedfrom=MSDN#account-sas-permissions-by-operation
                // https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-account-sas?redirectedfrom=MSDN#specifying-account-sas-parameters
                // used by blob, file, table, queue
                sp_read.IsEnabled   = true; 
                sp_write.IsEnabled  = true; 
                sp_delete.IsEnabled = true; // Uncheck the disable checkboxes
                sp_list.IsEnabled   = true; //sp_list.IsChecked   = false;
                sp_create.IsEnabled = true;
                sp_add.IsEnabled    = true;     // Table, Queue only
                sp_update.IsEnabled = true;     // Table, Queue only
                sp_process.IsEnabled= true;     // Queue only
                ComboBox_sp_UpdateText();
            }

            SAS_Utils.PopulateComboBox_sv(ComboBox_sv, ComboBox_sr.Text, textBox_tn.Text);
            SAS_Utils.PopulateComboBox_sv(ComboBox_apiVersion, ComboBox_sr.Text, textBox_tn.Text);
        }

        private void ComboBox_srt_DropDownOpened(object sender, EventArgs e)
        {
            srt_service.IsChecked = Test(ComboBox_srt.Text, "s");
            srt_container.IsChecked = Test(ComboBox_srt.Text, "c");
            srt_object.IsChecked = Test(ComboBox_srt.Text, "o");
        }


        private void ComboBox_sv_DropDownOpened(object sender, EventArgs e)
        {
            SAS_Utils.PopulateComboBox_sv(ComboBox_sv, ComboBox_sr.Text, textBox_tn.Text);
        }

        private void ComboBox_apiVersion_DropDownOpened(object sender, EventArgs e)
        {
            SAS_Utils.PopulateComboBox_sv(ComboBox_apiVersion, ComboBox_sr.Text, textBox_tn.Text);
        }

        private void TextBox_tn_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_tn.Text != "")
            {
                ComboBox_apiVersion.Text = "";  // clear API-Version
                ComboBox_srt.Text = "";      // clear srt
                // ComboBox_srt.IsEnabled = false;      // leaving enable to make it possible to change to Account SAS

                ComboBox_ss.Text = "";         // clear ss
                ComboBox_ss.IsEnabled = false;

                ComboBox_sr.Text = "";      // clear sr
                // ComboBox_sr.IsEnabled = false;     // leaving enable to make it possible to change to Account SAS


                textBox_si.IsEnabled = true;
                label_PolicyName.IsEnabled = true;

                // enable partitinon and row limits
                SetStatus_PartitionRowBoxes(true);

                textBoxTableName.Text = textBox_tn.Text;
                labelTableName.Foreground = Brushes.Black;

                label_srt.Foreground = Brushes.Black;
                label_sr.Foreground = Brushes.Black;
                label_tn.Foreground = Brushes.Black;


                // Setting the sp (signed permissions) based on Service SAS tn
                // https://docs.microsoft.com/pt-pt/rest/api/storageservices/create-service-sas#permissions-for-a-table
                // queue - raup
                sp_read.IsEnabled   = true; // Uncheck the disable checkboxes
                sp_write.IsEnabled  = false; sp_write.IsChecked  = false;
                sp_delete.IsEnabled = true; 
                sp_list.IsEnabled   = false; sp_list.IsChecked   = false;
                sp_add.IsEnabled    = true; 
                sp_create.IsEnabled = false; sp_create.IsChecked = false;
                sp_update.IsEnabled = true; 
                sp_process.IsEnabled= false; sp_process.IsChecked = false;
                ComboBox_sp_UpdateText();
            }

            SAS_Utils.PopulateComboBox_sv(ComboBox_sv, ComboBox_sr.Text, textBox_tn.Text);
            SAS_Utils.PopulateComboBox_sv(ComboBox_apiVersion, ComboBox_sr.Text, textBox_tn.Text);
        }


        private void SetStatus_PartitionRowBoxes(bool status)
        {
            if(status == false)
            { 
                textBox_erk.Text = "";         // clear erk
                textBox_srk.Text = "";         // clear srk
                textBox_epk.Text = "";         // clear epk
                textBox_spk.Text = "";         // clear spk
            }

            textBox_erk.IsEnabled = status;
            textBox_srk.IsEnabled = status;
            textBox_epk.IsEnabled = status;
            textBox_spk.IsEnabled = status;

            label_erk.IsEnabled = status;
            label_srk.IsEnabled = status;
            label_epk.IsEnabled = status;
            label_spk.IsEnabled = status;
        }






        /// <summary>
        /// Update SAS struct with values from graphic Combo and Text Boxes with values from the decoded SAS.
        /// Only update the textBox's if no issue found on the specified service
        /// Used after "Check SAS Parameters"
        /// </summary>
        private void Set_ValuesFromStruct_ToBoxes()
        {
            // sharedAccessSignature; // complete SAS without the endpoints

            // blobEndpoint;
            // fileEndpoint;
            // tableEndpoint;
            // queueEndpoint;

            textBoxAccountName.Text = SAS_Utils.SAS.storageAccountName.v;   // storage Account Name, if provided on any Endpoint

            textBoxContainerName.Text = SAS_Utils.SAS.containerName.v;
            textBoxBlobName.Text = SAS_Utils.SAS.blobName.v;
            textBoxShareName.Text = SAS_Utils.SAS.shareName.v;
            textBoxFileName.Text = SAS_Utils.SAS.fileName.v;
            textBoxQueueName.Text = SAS_Utils.SAS.queueName.v;
            textBoxTableName.Text = (SAS_Utils.SAS.tn.v == "not found" ? SAS_Utils.SAS.tableName.v : SAS_Utils.SAS.tn.v);  // uses tn if Access SAS, otherwise uses tableName if exists 


            // onlySASprovided;        // true if the endpoints not provided

            ComboBox_ss.Text = (SAS_Utils.SAS.ss.v == "not found" ? "" : SAS_Utils.SAS.ss.v);
            ComboBox_srt.Text = (SAS_Utils.SAS.srt.v == "not found" ? "" : SAS_Utils.SAS.srt.v);
            ComboBox_sp.Text = (SAS_Utils.SAS.sp.v == "not found" ? "" : SAS_Utils.SAS.sp.v);
            textBox_se.Text = (SAS_Utils.SAS.se.v == "not found" ? "" : SAS_Utils.SAS.se.v);
            textBox_st.Text = (SAS_Utils.SAS.st.v == "not found" ? "" : SAS_Utils.SAS.st.v);
            textBox_sip.Text = (SAS_Utils.SAS.sip.v == "not found" ? "" : SAS_Utils.SAS.sip.v);
            ComboBox_spr.Text = (SAS_Utils.SAS.spr.v == "not found" ? "" : SAS_Utils.SAS.spr.v);

            textBox_sig_left.Text = (SAS_Utils.SAS.sig == "not found" ? "" : SAS_Utils.SAS.sig);  // Encripted signature
            Set_Color_Signature(); 

            ComboBox_sr.Text = (SAS_Utils.SAS.sr.v == "not found" ? "" : SAS_Utils.SAS.sr.v);
            textBox_tn.Text = (SAS_Utils.SAS.tn.v == "not found" ? "" : SAS_Utils.SAS.tn.v);
            textBox_erk.Text = (SAS_Utils.SAS.erk == "not found" ? "" : SAS_Utils.SAS.erk);
            textBox_srk.Text = (SAS_Utils.SAS.srk == "not found" ? "" : SAS_Utils.SAS.srk);
            textBox_epk.Text = (SAS_Utils.SAS.epk == "not found" ? "" : SAS_Utils.SAS.epk);
            textBox_spk.Text = (SAS_Utils.SAS.spk == "not found" ? "" : SAS_Utils.SAS.spk);
            textBox_si.Text = (SAS_Utils.SAS.si == "not found" ? "" : SAS_Utils.SAS.si);        // Policy Name

            SAS_Utils.PopulateComboBox_sv(ComboBox_apiVersion, ComboBox_sr.Text, textBox_tn.Text);
            SAS_Utils.PopulateComboBox_sv(ComboBox_sv, ComboBox_sr.Text, textBox_tn.Text);
            ComboBox_apiVersion.SelectedIndex = ComboBox_apiVersion.Items.IndexOf((SAS_Utils.SAS.apiVersion.v == "not found" ? "" : SAS_Utils.SAS.apiVersion.v));
            ComboBox_sv.SelectedIndex = ComboBox_sv.Items.IndexOf((SAS_Utils.SAS.sv.v == "not found" ? "" : SAS_Utils.SAS.sv.v));

            // Set label colors in case of error
            //----------------------------
            labelAccountName.Foreground = (SAS_Utils.SAS.storageAccountName.s ? Brushes.Black : Brushes.Red);

            labelContainerName.Foreground = (SAS_Utils.SAS.containerName.s ? Brushes.Black : Brushes.Red);
            labelBlobName.Foreground = (SAS_Utils.SAS.blobName.s ? Brushes.Black : Brushes.Red);
            labelBlobSnapshotName.Foreground = (SAS_Utils.SAS.blobSnapshotName.s ? Brushes.Black : Brushes.Red);
            labelShareName.Foreground = (SAS_Utils.SAS.shareName.s ? Brushes.Black : Brushes.Red);
            labelFileName.Foreground = (SAS_Utils.SAS.fileName.s ? Brushes.Black : Brushes.Red);
            labelQueueName.Foreground = (SAS_Utils.SAS.queueName.s ? Brushes.Black : Brushes.Red);
            labelTableName.Foreground = (SAS_Utils.SAS.tableName.s ? Brushes.Black : Brushes.Red);

            label_sv.Foreground = (SAS_Utils.SAS.sv.s ? Brushes.Black : Brushes.Red);
            label_ss.Foreground = (SAS_Utils.SAS.ss.s ? Brushes.Black : Brushes.Red);
            label_srt.Foreground = (SAS_Utils.SAS.srt.s ? Brushes.Black : Brushes.Red);
            label_sp.Foreground = (SAS_Utils.SAS.sp.s ? Brushes.Black : Brushes.Red);
            label_se.Foreground = (SAS_Utils.SAS.se.s ? Brushes.Black : Brushes.Red);
            label_st.Foreground = (SAS_Utils.SAS.st.s ? Brushes.Black : Brushes.Red);
            label_sip.Foreground = (SAS_Utils.SAS.sip.s ? Brushes.Black : Brushes.Red);
            label_spr.Foreground = (SAS_Utils.SAS.spr.s ? Brushes.Black : Brushes.Red);

            label_sr.Foreground = (SAS_Utils.SAS.sr.s ? Brushes.Black : Brushes.Red);
            label_tn.Foreground = (SAS_Utils.SAS.tn.s ? Brushes.Black : Brushes.Red);

            // Other labels
            labelAccountKey.Foreground = Brushes.Black;
            labelInsertSAS.Foreground = Brushes.Black;
            label_sip.Foreground = Brushes.Black;
            //----------------------------
        }




        /// <summary>
        /// Update graphic Combo and Text Boxes with values from the SAS struct
        /// Used before "Regenerate SAS"
        /// </summary>
        private void Get_ValuesFromBoxes_ToStruct()
        {
            // sharedAccessSignature; // complete SAS without the endpoints

            // blobEndpoint;
            // fileEndpoint;
            // tableEndpoint;
            // queueEndpoint;

            SAS_Utils.SAS.storageAccountName.v = textBoxAccountName.Text;

            SAS_Utils.SAS.containerName.v = textBoxContainerName.Text;
            SAS_Utils.SAS.blobName.v = textBoxBlobName.Text;
            SAS_Utils.SAS.shareName.v = textBoxShareName.Text;
            SAS_Utils.SAS.fileName.v = textBoxFileName.Text;
            SAS_Utils.SAS.queueName.v = textBoxQueueName.Text;
            SAS_Utils.SAS.tableName.v = textBoxTableName.Text;

            // onlySASprovided;        // true if the endpoints not provided

            SAS_Utils.SAS.apiVersion.v = ComboBox_apiVersion.Text;
            SAS_Utils.SAS.sv.v = ComboBox_sv.Text;
            SAS_Utils.SAS.ss.v = ComboBox_ss.Text;
            SAS_Utils.SAS.srt.v = ComboBox_srt.Text;
            SAS_Utils.SAS.sp.v = ComboBox_sp.Text;
            SAS_Utils.SAS.se.v = textBox_se.Text;
            SAS_Utils.SAS.st.v = textBox_st.Text;
            SAS_Utils.SAS.sip.v = textBox_sip.Text;
            SAS_Utils.SAS.spr.v = ComboBox_spr.Text;
            SAS_Utils.SAS.sig = textBox_sig_left.Text;  // Encripted signatute from left, to validate existing data

            SAS_Utils.SAS.sr.v = ComboBox_sr.Text;
            SAS_Utils.SAS.tn.v = textBox_tn.Text;
            SAS_Utils.SAS.blobSnapshotName.v = textBoxBlobSnapshotName.Text;    // v12.0.0.0_preview
            SAS_Utils.SAS.erk = textBox_erk.Text;
            SAS_Utils.SAS.srk = textBox_srk.Text;
            SAS_Utils.SAS.epk = textBox_epk.Text;
            SAS_Utils.SAS.spk = textBox_spk.Text;
            SAS_Utils.SAS.si = textBox_si.Text;


            // used by v12.0.0.0_preview
            SAS_Utils.fromIP[0] = 0;
            SAS_Utils.fromIP[1] = 0;
            SAS_Utils.fromIP[2] = 0;
            SAS_Utils.fromIP[3] = 0;

            SAS_Utils.toIP[0] = 0;
            SAS_Utils.toIP[1] = 0;
            SAS_Utils.toIP[2] = 0;
            SAS_Utils.toIP[3] = 0;
        }

        private void BoxAuthResults_Right_GotFocus(object sender, RoutedEventArgs e)
        {
            RemoveSplash();
        }

        private void ComboBox_sv_DropDownClosed(object sender, EventArgs e)
        {
            if(ComboBox_sv.Text != "")
                label_sv.Foreground = Brushes.Black;
        }

        private void TextBox_se_LostFocus(object sender, RoutedEventArgs e)
        {
            if(textBox_se.Text != "")
                if(DateTime.Compare(DateTime.UtcNow.ToUniversalTime(), Convert.ToDateTime(textBox_se.Text).ToUniversalTime()) > 0)
                {
                    label_se.Foreground = Brushes.Red;
                    MessageBox.Show("Expiry Date already expired", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                    label_se.Foreground = Brushes.Black;
        }

        private void TextBoxAccountKey1_LostFocus(object sender, RoutedEventArgs e)
        {
            if(textBoxAccountKey1.Text != "")
                labelAccountKey.Foreground = Brushes.Black;
        }

        private void TextBoxAccountName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxAccountName.Text != "")
                labelAccountName.Foreground = Brushes.Black;
        }

        private void TextBoxContainerName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxContainerName.Text != "")
                labelContainerName.Foreground = Brushes.Black;
        }

        private void TextBoxBlobName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxBlobName.Text != "")
                labelBlobName.Foreground = Brushes.Black;
        }

        private void TextBoxBlobSnapshotName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxBlobSnapshotName.Text != "")
                labelBlobSnapshotName.Foreground = Brushes.Black;
        }

        private void TextBoxShareName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxShareName.Text != "")
                labelShareName.Foreground = Brushes.Black;
        }

        private void TextBoxFileName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxFileName.Text != "")
                labelFileName.Foreground = Brushes.Black;
        }

        private void TextBoxQueueName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxQueueName.Text != "")
                labelQueueName.Foreground = Brushes.Black;
        }

        private void TextBoxTableName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxTableName.Text != "")
                labelTableName.Foreground = Brushes.Black;
        }

        private void TextBox_st_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox_st.Text != "")
                label_st.Foreground = Brushes.Black;
        }




        private void CheckBoxPreRelease12_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxPreRelease12.IsChecked == true)
            {
                textBox_tn.Text = "";          // clear tn
                textBoxTableName.Text = "";     // clear Table Name
                ComboBox_apiVersion.Text = "";     // clear API-Version

                textBox_tn.IsEnabled = false;
                textBoxTableName.IsEnabled = false;
                ComboBox_apiVersion.IsEnabled = false;

                textBoxBlobSnapshotName.IsEnabled = true;
                sr_blobSnapshot.IsEnabled = true;

                labelMessages.Content = "Table Service are not supported by Storage SDK v12.0.0_preview. API Version replaced by Service Version on Storage SDK v12.0.0_preview";
            }
            else
            {
                textBox_tn.IsEnabled = true;
                textBoxTableName.IsEnabled = true;
                ComboBox_apiVersion.IsEnabled = true;
                labelMessages.Content = "";

                textBoxBlobSnapshotName.IsEnabled = false;
                sr_blobSnapshot.IsEnabled = false;

                labelMessages.Content = "Storage SDK v11.x - Account SAS: API Version not supported. Service SAS: No optional parameters supported. Only supports 'Signed Permissions', 'Start' and 'Expiry Date Time'.  Please use SDK v12.0.0.0 instead (Table Service SAS not supported)";
            }
        }

        private void Sp_write_Click(object sender, RoutedEventArgs e)
        {
            if(ComboBox_sr.Text != "" || textBox_tn.Text!="")
                labelMessages.Content = "Selecting the 'Write' permission invatidate the Service SAS on Azure Storage Explorer for some reason.";
        }

        private void LabelHelp_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SplashInfo();
        }
    }
}
