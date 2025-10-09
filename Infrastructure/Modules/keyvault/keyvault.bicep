param location string
param microserviceName string
param environment string
param tags object = {}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${microserviceName}-${environment}-kv'
  location: location
  properties: {
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    enablePurgeProtection: true
    sku: {
      name: 'standard'
      family: 'A'
    }
  }
  tags: tags
}

output keyVaultId string = keyVault.id
output keyVaultUri string = keyVault.properties.vaultUri
