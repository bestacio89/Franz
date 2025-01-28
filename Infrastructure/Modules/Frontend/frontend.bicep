param location string
param databaseType string

// Kafka for Event Storage
resource kafkaCluster 'Microsoft.EventHub/namespaces@2021-11-01' = if (databaseType == 'multi') {
  name: 'eventKafkaNamespace'
  location: location
  properties: {
    sku: {
      name: 'Standard'
      tier: 'Standard'
    }
    compatibilityLevel: 'Kafka_2_4'
  }
}

// MongoDB for Event Storage
resource mongoDb 'Microsoft.DocumentDB/databaseAccounts@2021-03-15' = if (databaseType == 'multi') {
  name: 'eventMongoDbAccount'
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    kind: 'MongoDB'
  }
}

// PostgreSQL for Entity Storage
resource postgresql 'Microsoft.DBforPostgreSQL/servers@2021-06-01' = if (databaseType == 'multi') {
  name: 'entityPostgresServer'
  location: location
  properties: {
    version: '11'
    sslEnforcement: 'Enabled'
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
  }
}

output kafkaResourceId string = kafkaCluster.id
output mongoResourceId string = mongoDb.id
output postgresqlResourceId string = postgresql.id
