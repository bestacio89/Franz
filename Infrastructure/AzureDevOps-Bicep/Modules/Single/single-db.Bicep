param location string
param entityStorageType string
param microserviceName string
param environment string
param tags object = {}
param adminUser string
@secure()
param adminPassword string

resource pgsql 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = if (entityStorageType == 'PostgreSQL') {
  name: '${microserviceName}-${environment}-pgsql'
  location: location
  properties: {
    administratorLogin: adminUser
    administratorLoginPassword: adminPassword
    version: '14'
    storage: {
      storageSizeGB: 32
    }
  }
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  tags: tags
}

output entityStorageResourceId string = entityStorageType == 'PostgreSQL' ? (pgsql.id ?? '') : ''
