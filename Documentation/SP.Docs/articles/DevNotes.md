## Ignore notes below
When developing plug-ins, Custom1, 2 and 3 have to be set to default\actual values. 
Should not stay null.


# 
For Serilog to work correct while running under a service account, the logSettings.json needs to have a full path.
C:\\Your installation path\\logs\\


------------------
https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

Add-Migration InitialCreate
update-database

Add-Migration AddAttackTypeToBlocks
update-database

Add-Migration AddCustomFieldsToAccessAttempts
update-database

Add-Migration IpRanges
update-database
