// ========================================
// 🔹 Global Parameters
// ========================================
param location string = resourceGroup().location
param microserviceName string
param environment string

@allowed([ 'single', 'multi' ])
param databaseType string

@allowed([ 'CosmosDB', 'MongoDB' ])
param eventStorageType string

@allowed([ 'PostgreSQL', 'MariaDB', 'MySQL', 'SQLServer', 'Oracle' ])
param entityStorageType string

@allowed([ 'Kafka', 'RabbitMQ', 'Both' ])
param messagingType string

param kafkaNamespaceName string = '${microserviceName}-${environment}-kafka'
param rabbitmqNamespaceName string = '${microserviceName}-${environment}-rabbit'

param tags object = {}

@secure()
param adminPassword string
param adminUser string = 'dbadmin'

// ========================================
// 🔹 Modules
// ========================================

// Kafka
module kafkaModule './modules/kafka/kafka.bicep' = if (messagingType == 'Kafka' || messagingType == 'Both') {
  name: 'kafkaDeployment'
  params: {
    location: location
    kafkaNamespaceName: kafkaNamespaceName
    microserviceName: microserviceName
    environment: environment
    tags: tags
  }
}

// RabbitMQ
module rabbitmqModule './modules/rabbitmq/rabbitmq.bicep' = if (messagingType == 'RabbitMQ' || messagingType == 'Both') {
  name: 'rabbitmqDeployment'
  params: {
    location: location
    rabbitmqName: rabbitmqNamespaceName
    microserviceName: microserviceName
    environment: environment
    tags: tags
  }
}

// Multi-DB
module multiDbModule './modules/Multi/multi-db.bicep' = if (databaseType == 'multi') {
  name: 'multiDbDeployment'
  params: {
    location: location
    eventStorageType: eventStorageType
    entityStorageType: entityStorageType
    microserviceName: microserviceName
    environment: environment
    tags: tags
    adminUser: adminUser
    adminPassword: adminPassword
  }
}

// Single-DB
module singleDbModule './modules/Single/single-db.bicep' = if (databaseType == 'single') {
  name: 'singleDbDeployment'
  params: {
    location: location
    entityStorageType: entityStorageType
    microserviceName: microserviceName
    environment: environment
    tags: tags
    adminUser: adminUser
    adminPassword: adminPassword
  }
}

// Key Vault
module keyVaultModule './modules/keyvault/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    location: location
    microserviceName: microserviceName
    environment: environment
    tags: tags
  }
}

// ========================================
// 🔹 Outputs (always safe strings)
// ========================================
output kafkaResourceId string = contains(['Kafka','Both'], messagingType) ? (kafkaModule.outputs.kafkaNamespaceId ?? '') : ''
output rabbitmqResourceId string = contains(['RabbitMQ','Both'], messagingType) ? (rabbitmqModule.outputs.rabbitmqId ?? '') : ''
output eventStorageResourceId string = databaseType == 'multi' ? (multiDbModule.outputs.eventStorageResourceId ?? '') : ''
output entityStorageResourceId string = databaseType == 'multi' ? (multiDbModule.outputs.entityStorageResourceId ?? '') : (singleDbModule.outputs.entityStorageResourceId ?? '')
output keyVaultId string = keyVaultModule.outputs.keyVaultId
output keyVaultUri string = keyVaultModule.outputs.keyVaultUri
output messagingTypeOut string = messagingType
output databaseTypeOut string = databaseType
