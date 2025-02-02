# SoeDay_AcaDemo_20250201
## add extension
az extension add --name containerapp --upgrade
az provider register --namespace Microsoft.App

## Azure ログイン
```powershell
az login --tenant <your-tenantid>
```

## 現在のサブスクリプション表示 ※誤操作防止用
```powershell
az account show
```

## リソースグループ作成
```powershell
$resource_group = '<your-resource-group>'
$environmentName = '<your-environmentName>' # ex. osatest
az group create --name $resource_group --location japaneast
```

## Environmentを作成
```powershell
az deployment group create --resource-group $resource_group --template-file ./deploy/_env/main.bicep --parameters environmentName=$environmentName
```

## 作成されたAzure Container Registryの情報を取得
```powershell
$crServerName = 'cr' + $environmentName + '.azurecr.io'
$crUserName = 'cr' + $environmentName
$crPassword = az acr credential show --name $crUserName --query "passwords[0].value" -o tsv

$crServerName
$crUserName
$crPassword
```

## docker login
```powershell
docker login $crServerName
```
## コンテナを作成しACRへPush
### バックエンド
```powershell
$tagName=$crServerName + '/dapr-backend:latest'
docker build -t dapr-backend:latest .
docker tag dapr-backend $tagName
docker push $tagName
```

### フロントエンド
```powershell
docker build -t dapr-frontend:latest .
$tagName01=$crServerName + '/dapr-frontend:0.1.0'
docker tag dapr-frontend $tagName01
docker push $tagName01
$tagName02=$crServerName + '/dapr-frontend:0.2.0'
docker tag dapr-frontend $tagName02
docker push $tagName02
```
## Container App作成
### バックエンド
```powershell
$crImage = $crServerName + '/dapr-backend:latest'
az deployment group create --resource-group $resource_group --template-file ./deploy/backend-app/main.bicep --parameters environmentName=$environmentName crServerName=$crServerName crUserName=$crUserName crPassword=$crPassword crImage=$crImage
```

### フロントエンド
```powershell
$crImage = $crServerName + '/dapr-frontend:0.1.0'
$revisionSuffix = 'v1'
$oldRevisionSuffix = 'v1'
az deployment group create --resource-group $resource_group --template-file ./deploy/frontend-app/main.bicep --parameters environmentName=$environmentName crServerName=$crServerName crUserName=$crUserName crPassword=$crPassword crImage=$crImage revisionSuffix=$revisionSuffix oldRevisionSuffix=$oldRevisionSuffix
```

## Blue/Green Deployments
### Redeploy
```powershell
$crImage = $crServerName + '/dapr-frontend:0.2.0'
$revisionSuffix = 'v2'
$oldRevisionSuffix = 'v1'
az deployment group create --resource-group $resource_group --template-file ./deploy/frontend-app/main.bicep --parameters environmentName=$environmentName crServerName=$crServerName crUserName=$crUserName crPassword=$crPassword crImage=$crImage revisionSuffix=$revisionSuffix oldRevisionSuffix=$oldRevisionSuffix
```
### Swap
```powershell
$caName = 'ca-' + $environmentName + '-fed'
az containerapp ingress traffic set -n $caName -g $resource_group --revision-weight $caName--v1=0 latest=100
```

### 旧バージョンの Revision をシャットダウン
```powershell
$caName = 'ca-' + $environmentName + '-fed'
az containerapp revision deactivate -n $caName -g $resource_group--revision $caName--v1
```
