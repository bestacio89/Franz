param location string
param kafkaNamespaceName string
param microserviceName string
param environment string
param tags object = {}

resource kafkaNamespace 'Microsoft.EventHub/namespaces@2022-10-01-preview' = {
  name: kafkaNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    kafkaEnabled: true
    isAutoInflateEnabled: true
    maximumThroughputUnits: 4
  }
  tags: tags
}

output kafkaNamespaceId string = kafkaNamespace.id
