# Storage Helper SAS Tool

<table>

  <tr>
    <td>
	<b>v1.0.6</b>
	</td>    
    <td>
		<a href="https://github.com/LuisFilipe236/Storage-Helper-SAS-Tool/releases/download/v1.0.6/Storage.Helper.SAS.Tool.Installer.msi">
			<img alt="Storage Helper Tool v1.0.6" src="https://img.shields.io/github/downloads/LuisFilipe236/Storage-Helper-SAS-Tool/v1.0.6/total?label=downloads_v1.0.6">
		</a>
    </td>
  </tr>

  <tr>
    <td>
	<b>v1.0.5</b>
	</td>    
    <td>
		<a href="https://github.com/LuisFilipe236/Storage-Helper-SAS-Tool/releases/download/v1.0.5/Storage.Helper.SAS.Tool.Installer.msi">
			<img alt="Storage Helper Tool v1.0.5" src="https://img.shields.io/github/downloads/LuisFilipe236/Storage-Helper-SAS-Tool/v1.0.5/total?label=downloads_v1.0.5">
		</a>
    </td>
  </tr>

  <tr>
    <td>
	<b>v1.0.4</b>
	</td>    
    <td>
		<a href="https://github.com/LuisFilipe236/Storage-Helper-SAS-Tool/releases/download/v1.0.4/Storage.Helper.SAS.Tool.Installer.msi">
			<img alt="Storage Helper Tool v1.0.4" src="https://img.shields.io/github/downloads/LuisFilipe236/Storage-Helper-SAS-Tool/v1.0.4/total?label=downloads_v1.0.4">
		</a>
    </td>
  </tr>
      
</table>



<b>DISCLAIMER:</b>

This application is to test proposes only, runs only locally and don't use the provided information for any other proposes other than checking the Storage SAS parameters locally.
This application don't save any information on any place and does not use the Storage Account Key or the Storage Account SAS to access the storage resources on any way.
You can run this application or the source code at your own risk.

<hr>

<b>HOW TO USE:</b>

Checking SAS parameters:

Insert the Storage Account SAS or Connection String in the box on the top, and use the 'Check SAS Parameters' button to validate all the fields.
Check the results on the left bottom box. The parameter boxes will be filled with the readed parameters.
Any errors will be marked with '-->' and the parameter boxes in red.

Regenerating SAS:

Use the 'Regenerate SAS' button to create a new Storage Account SAS, based on the values in the parameter Boxes.
To do that the Storage Account Key is needed. All other mandatory values not provided will be asked and marked in red.
The results with the new generated Storage Account SAS will be displayed on the right bottom box.
Use the two 'Signature' boxes to compare the two signatures, if you wish to compare your 'sig' with the generated 'sig'.

Developed by Azure Storage Support Team to test proposes only.

<hr>
  
<b>HOW TO INSTALL:</b>

Click on the MSI last version counter above to download the MSI installer for Windows.

<hr>

<b>TODO:</b>
 - Implement check Policy permissions (service SAS);
 - Support for User Delegation SAS (sv>=2018-11-09 supported on Blob Service SAS only)

 <hr> 

<b>VERSIONS:</b>

<b>v1.0.6:</b>
Changes:
- Moved to Azure Blob storage client library v12.0.0 (now GA available);
- Azure.Storge.Files deprecated - moved to Azure.Storage.Files.Shares, (currently still on v12.0.0 Preview 5);
- Some minor corrections.

<b>v1.0.5:</b>
Changes:
- Alert on two or more '??' in the SAS;
- Adding endpoints on the left output when provided on SAS or Connection string;
- Pre check/uncheck the Service Permissions based on the text, from the SAS
- Implemented the Tips on the Regenerated output. All the limitations identified for each type of SAS are now listed after Regenerate a SAS.

<b>v1.0.4:</b>
Changes:
- Removed support for Storage SDK v11.x and the option to select SDK v11.x / v12.x from interface;
- Removed Api-Version parameter from interface - not used on SDK v12.x and not supported on Cosmos Table;
- Removed Table Name field as query parameter from interface (on right gorup ComboBoxes);
- Corrected the Queue, Table and BlobSnapshot Service SAS Regeneration;
- Added Notes, URI, and link to test SAS on browser, on Regenerated output, in UrlEncecoded and UrlDecoded formats;

<b>v1.0.3:</b>
Changes:

<b>v1.0.2:</b>
Changes:
- Added support for Storage SDK 12.0.0 (preview) 
- Added support for Cosmos Table SDK 2.0.0
