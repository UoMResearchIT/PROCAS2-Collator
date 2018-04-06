# PROCAS2-Collator
PROCAS 2 Collator Software

For the Predicting the Risk of Cancer At Screening research project at the Centre for Health Informatics, University of Manchester.

Database is implemented using Code First, i.e. no need to first create the database, just set the connectionstring appropriately and it will do
it for you. The initial data for the lookup tables can be loaded using the SQL in PROCAS2.Data/InitialData.

Stack:

Visual Studio 2015 project

ASP.NET MVC/WebApi
Azure integration

Azure SQL database (production), LocalDB for dev.
Azure ServiceBus and Blob services.
