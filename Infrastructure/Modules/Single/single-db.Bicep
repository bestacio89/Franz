trigger:
- main

variables:
  frontEndType: 'Blazor'  # This can be changed based on user input
  databaseType: 'single'   # Choose between 'single' or 'multi' for the database setup
  singleDbChoice: 'SQLServer' # Choose the database for single DB setup (PostgreSQL, MariaDB, SQLServer, Oracle)

jobs:
- job: Deploy
  pool:
    vmImage: 'ubuntu-latest'
  steps:
    - task: AzureCLI@2
      inputs:
        azureSubscription: 'your-azure-subscription'
        scriptType: 'bash'
        scriptLocation: 'inlineScript'
        inlineScript: |
          az deployment group create \
            --resource-group your-resource-group \
            --template-file main.bicep \
            --parameters frontEndType=$(frontEndType) databaseType=$(databaseType) singleDbChoice=$(singleDbChoice)
