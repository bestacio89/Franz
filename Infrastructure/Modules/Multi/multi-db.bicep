param location string
param eventStorageType string
param entityStorageType string
param microserviceName string
param environment string
param tags object = {}
param adminUser string
@secure() 
param adminPassword string

// Cosmos DB (Mongo API)
resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = if (eventStorageType == 'CosmosDB') {
  name: '${microserviceName}-${environment}-cosmos'
  location: location
  kind: 'MongoDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    apiProperties: {
      serverVersion: '4.0'
    }
    capabilities: [
      {
        name: 'EnableMongo'
      }
    ]
  }
  tags: tags
}

// PostgreSQL Flexible Server
resource pgsql 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = if (entityStorageType == 'PostgreSQL') {
  name: '${microserviceName}-${environment}-pgsql'
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    administratorLogin: adminUser
    administratorLoginPassword: adminPassword
    version: '14'
    storage: {
      storageSizeGB: 32
    }
    highAvailability: {
      mode: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
  }
  tags: tags
}

// Outputs (safe)
output eventStorageResourceId string = eventStorageType == 'CosmosDB' ? cosmosDb.id : ''
output entityStorageResourceId string = entityStorageType == 'PostgreSQL' ? pgsql.id : ''
