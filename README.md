# Umbraco.DataTypes.DynamicGrid
Dynamic Grid Data Type for Umbraco (Codes receieved from CodePlex)



## Project Description
The Dynamic Grid Data Type for Umbraco is a custom ASCX/C# control that was created to store tabular data as an Umbraco "Data Type". There's an ability to add/remove rows/columns and it writes the whole grid to the database as an XML string. All done via UpdatePanels.

## NOTE:
Changes in 0.8.1.0: Renamed nodes when stored from "NewDataSet" and "Dimensions" to "Data" and "Row". If you refer to "NewDataSet" and / or "Dimensions" in XSLT, make sure to update.

## TODO:
* Package control as an Umbraco Package (partway there - see documentation)
* Create an Umbraco XSLT Extension to convert the XML to an HTML table so it can be displayed on the front-end.
* Allow adding more than one instance of Dynamic Grid Data in a single Document Type
* User-configurable default column names done

## To Install:

Download the .zip file and install as a package in Umbraco. This will add the necessary references to the Dynamic Data Grid type, but you will still need to create your own "Data Type" in Umbraco, and set its "Render Control" to "Dynamic Grid". If anyone can explain to me how to have my package create a data type on install and set its Render Control, I'm all ears. I can do the former but not the latter.
