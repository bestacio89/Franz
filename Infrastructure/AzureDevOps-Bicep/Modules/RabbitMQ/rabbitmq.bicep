param location string
param rabbitmqName string
param microserviceName string
param environment string
param tags object = {}

resource rabbitmqNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: rabbitmqName
  location: location
  sku: {
    name: 'Premium'
    tier: 'Premium'
  }
  tags: tags
}

output rabbitmqId string = rabbitmqNamespace.id
