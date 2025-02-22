param kafkanms string
param databaseType string  // "single" or "multi"
param eventStorageType string  // "Kafka", "CosmosDB", "MongoDB"
param entityStorageType string  // "PostgreSQL", "MariaDB", "MySQL", "SQLServer", "Oracle"
param environment string

// ✅ Deploy Kafka Module Only for Multi-DB Mode
module kafkaModule './Modules/kafka/kafka.bicep' = if (databaseType == 'multi') {
  name: 'kafkaDeployment'
  params: {
    location: resourceGroup().location
    kafkaNamespaceName: kafkanms
  }
}

// ✅ Deploy Multi-DB Module If Multi-DB is Selected
module multiDbModule './modules/Multi/multi-db.bicep' = if (databaseType == 'multi') {
  name: 'multiDbDeployment'
  params: {
    location: resourceGroup().location
    eventStorageType: eventStorageType
    entityStorageType: entityStorageType
  }
}

// ✅ Deploy Single-DB Module If Single-DB is Selected
module singleDbModule './modules/Single/single-db.bicep' = if (databaseType == 'single') {
  name: 'singleDbDeployment'
  params: {
    location: resourceGroup().location
    entityStorageType: entityStorageType
  }
}

// ✅ Outputs (Select from the Correct Module)
output kafkaResourceId string = databaseType == 'multi' ? kafkaModule.outputs.kafkaNamespaceId : ''
output eventStorageResourceId string = databaseType == 'multi' ? multiDbModule.outputs.eventStorageResourceId : ''
output entityStorageResourceId string = databaseType == 'multi' ? multiDbModule.outputs.entityStorageResourceId : singleDbModule.outputs.entityStorageResourceId
