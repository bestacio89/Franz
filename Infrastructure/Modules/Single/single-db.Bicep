param location string
param entityStorageType string // "PostgreSQL", "MariaDB", "MySQL", "SQLServer", "Oracle"

// ========================================
// ✅ Conditional Database Deployment
// ========================================
resource postgresqlDb 'Microsoft.DBforPostgreSQL/flexibleServers@2022-03-01' = if (entityStorageType == 'PostgreSQL') {
  name: 'postgresqlForEntities'
  location: location
  properties: {
    version: '11'
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
    storageProfile: {
      storageMB: 32768
    }
  }
  sku: {
    name: 'Standard_D2ds_v4'
    tier: 'GeneralPurpose'
  }
}

resource mariaDb 'Microsoft.DBforMariaDB/servers@2021-06-01' = if (entityStorageType == 'MariaDB') {
  name: 'mariaDbForEntities'
  location: location
  properties: {
    version: '10.3'
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
    storageProfile: {
      storageMB: 32768
    }
    sslEnforcement: 'Enabled'
  }
  sku: {
    name: 'GP_Gen5_2'
    tier: 'GeneralPurpose'
  }
}

resource mysqlDb 'Microsoft.DBforMySQL/flexibleServers@2022-03-01' = if (entityStorageType == 'MySQL') {
  name: 'mysqlForEntities'
  location: location
  properties: {
    version: '8.0'
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
    storageProfile: {
      storageMB: 32768
    }
    sslEnforcement: 'Enabled'
  }
  sku: {
    name: 'Standard_D2ds_v4'
    tier: 'GeneralPurpose'
  }
}

resource sqlServerDb 'Microsoft.Sql/servers@2020-11-01-preview' = if (entityStorageType == 'SQLServer') {
  name: 'sqlServerForEntities'
  location: location
  properties: {
    administratorLogin: 'adminUser'
    administratorLoginPassword: 'adminPassword'
    version: '12.0'
  }
}

// ✅ Oracle Database as a VM (Since Azure does not support Oracle PaaS)
resource oracleVm 'Microsoft.Compute/virtualMachines@2021-07-01' = if (entityStorageType == 'Oracle') {
  name: 'oracleDbVM'
  location: location
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_D4s_v3'
    }
    storageProfile: {
      imageReference: {
        publisher: 'oracle'
        offer: 'oracle-database-19c'
        sku: 'standard'
        version: 'latest'
      }
    }
    osProfile: {
      computerName: 'oracle-db'
      adminUsername: 'adminUser'
      adminPassword: 'adminPassword'
    }
  }
}

// ========================================
// ✅ OUTPUT (Ensure Only One is Selected)
// ========================================
output entityStorageResourceId string = postgresqlDb.id ?? mariaDb.id ?? sqlServerDb.id ?? oracleVm.id
