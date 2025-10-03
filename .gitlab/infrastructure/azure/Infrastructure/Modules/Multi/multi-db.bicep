param location string
param eventStorageType string
param entityStorageType string

// Event Storage - Cosmos DB or MongoDB
resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2021-03-15' = if (eventStorageType == 'CosmosDB') {
  name: 'cosmosDbForEvents'
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    kind: 'GlobalDocumentDB'
  }
}

resource mongoDb 'Microsoft.DocumentDB/databaseAccounts@2021-03-15' = if (eventStorageType == 'MongoDB') {
  name: 'mongoDbForEvents'
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    kind: 'MongoDB'
  }
}

// Entity Storage - Relational DBs (PostgreSQL, MariaDB, SQL Server, Oracle)
resource postgresqlDb 'Microsoft.DBforPostgreSQL/servers@2021-06-01' = if (entityStorageType == 'PostgreSQL') {
  name: 'postgresqlForEntities'
  location: location
  properties: {
    version: '11'
    sslEnforcement: 'Enabled'
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
  }
}

resource mariaDb 'Microsoft.DBforMariaDB/servers@2021-06-01' = if (entityStorageType == 'MariaDB') {
  name: 'mariaDbForEntities'
  location: location
  properties: {
    version: '10.3'
    sslEnforcement: 'Enabled'
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
  }
}

resource sqlServerDb 'Microsoft.Sql/servers@2021-02-01-preview' = if (entityStorageType == 'SQLServer') {
  name: 'sqlServerForEntities'
  location: location
  properties: {
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
    version: '12.0'
  }
}

resource oracleDb 'Microsoft.Oracle/servers@2021-04-01' = if (entityStorageType == 'Oracle') {
  name: 'oracleDbForEntities'
  location: location
  properties: {
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
    version: '19c'
  }
}

output eventStorageResourceId string = cosmosDb.id ?? mongoDb.id
output entityStorageResourceId string = postgresqlDb.id ?? mariaDb.id ?? sqlServerDb.id ?? oracleDb.id
