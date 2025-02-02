param environmentName string
param aceName string = 'ce-${environmentName}'
param containerAppName string = 'ca-${environmentName}-bed'
param location string = resourceGroup().location
param crServerName string
param crUserName string
@secure()
param crPassword string
param crImage string
param revisionSuffix string = ''
param isExternalIngress bool = false
param revisionMode string = 'single'
param isDaprenabled bool = true
param daprAppId string = 'dapr-backend'

resource environment 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: aceName
}

module apps 'apps.bicep' = {
  name: 'container-apps'
  params: {
    containerAppName: containerAppName
    location: location
    environmentId: environment.id
    crServerName: crServerName
    crUserName: crUserName
    crPassword: crPassword
    crImage: crImage
    revisionSuffix: revisionSuffix
    revisionMode: revisionMode
    isExternalIngress: isExternalIngress
    isDaprenabled: isDaprenabled
    daprAppId: daprAppId
  }
}
