param location string
param kafkaNamespaceName string
param skuName string = 'Standard'  // Default SKU for Event Hub
param partitionCount int = 4       // Default number of partitions

// Kafka Event Hub Namespace
resource kafkaNamespace 'Microsoft.EventHub/namespaces@2021-11-01' = {
  name: kafkaNamespaceName
  location: location
  properties: {
    sku: {
      name: skuName
      tier: skuName
    }
    compatibilityLevel: 'Kafka_2_4'
  }
}

// Kafka Topics (Multiple if needed)
resource kafkaTopic 'Microsoft.EventHub/namespaces/eventHubs@2021-11-01' = {
  name: '${kafkaNamespace.name}/eventHubTopic'
  location: location
  properties: {
    partitionCount: partitionCount
    messageRetentionInDays: 7
  }
}

// Output Kafka resource IDs
output kafkaNamespaceId string = kafkaNamespace.id
output kafkaTopicId string = kafkaTopic.id
