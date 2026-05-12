@description('Environment name used for resource naming and tags.')
param environmentName string = 'staging'

@description('Azure region for deployable resources.')
param location string = resourceGroup().location

var suffix = uniqueString(resourceGroup().id, environmentName)

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'plan-releaseboard-${environmentName}-${suffix}'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
  tags: {
    app: 'releaseboard'
    environment: environmentName
  }
}

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-releaseboard-api-${environmentName}-${suffix}'
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName
        }
      ]
    }
  }
  tags: {
    app: 'releaseboard'
    environment: environmentName
  }
}

output apiHostName string = apiApp.properties.defaultHostName
