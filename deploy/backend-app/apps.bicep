param containerAppName string
param location string = resourceGroup().location
param environmentId string
param crServerName string
param crUserName string
param crPassword string
param crImage string
param revisionSuffix string
param isExternalIngress bool
param isDaprenabled bool
param daprAppId string

@allowed([
  'multiple'
  'single'
])
param revisionMode string = 'single'

resource containerApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      secrets: [
        {
          name: 'container-registry-password'
          value: '${crPassword}'
        }
      ]
      registries: [
        {
          server: '${crServerName}'
          username: '${crUserName}'
          passwordSecretRef: 'container-registry-password'
        }
      ]
      activeRevisionsMode: revisionMode
      ingress: {
        external: isExternalIngress
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      dapr: {
        enabled: isDaprenabled
        appId: daprAppId
        appPort: 8080
        appProtocol: 'http'
      }
    }
    template: {
      revisionSuffix: revisionSuffix
      containers: [
        {
          image: crImage
          name: containerAppName
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'TZ'
              value: 'Asia/Tokyo'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-scaling-rule'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
