param frontEndType string
param databaseType string
param eventStorageType string
param entityStorageType string
param environment string  // Added environment parameter

module kafkaModule './kafka.bicep' = if (databaseType == 'multi') {
  name: 'kafkaDeployment'
  params: {
    location: resourceGroup().location
    environment: environment
  }
}

module multiDbModule './multi-db.bicep' = if (databaseType == 'multi') {
  name: 'multiDbDeployment'
  params: {
    location: resourceGroup().location
    eventStorageType: eventStorageType
    entityStorageType: entityStorageType
    environment: environment
  }
}

module apiModule './api.bicep' = {
  name: 'apiDeployment'
  params: {
    location: resourceGroup().location
    environment: environment
  }
}

module frontEndModule './frontend.bicep' = if (frontEndType == 'Blazor') {
  name: 'blazorFrontEndDeployment'
  params: {
    location: resourceGroup().location
    frontEndType: 'Blazor'
    environment: environment
  }
} else if (frontEndType == 'Angular') {
  name: 'angularFrontEndDeployment'
  params: {
    location: resourceGroup().location
    frontEndType: 'Angular'
    environment: environment
  }
} else if (frontEndType == 'ASP.NET') {
  name: 'aspNetFrontEndDeployment'
  params: {
    location: resourceGroup().location
    frontEndType: 'ASP.NET'
    environment: environment
  }
}

// Outputs
output kafkaResourceId string = kafkaModule.outputs.kafkaResourceId
output eventStorageResourceId string = multiDbModule.outputs.eventStorageResourceId
output entityStorageResourceId string = multiDbModule.outputs.entityStorageResourceId
output frontEndUrl string = frontEndModule.outputs.frontEndAppUrl
