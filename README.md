# SoeDay_AcaDemo_20250201
SoEDay Azure Container Apps Bule Green Deployments

# add extension
az extension add --name containerapp --upgrade
az provider register --namespace Microsoft.App

# Azure ログイン
```powershell
az login --tenant <your-tenantid>
```

# 現在のサブスクリプション表示 ※誤操作防止用
```powershell
az account show
```

```powershell
$resource_group = '<your-resource-group>'
$environmentName = '<your-environmentName>' # ex. osatest
```

# リソースグループ作成
```powershell
az group create --name $resource_group --location japaneast
```

# Environmentを作成
```powershell
az deployment group create --resource-group $resource_group --template-file ./deploy/_env/main.bicep --parameters environmentName=$environmentName
```

# 作成されたAzure Container Registryの情報を取得
```powershell
$crServerName = 'cr' + $environmentName + '.azurecr.io'
$crUserName = 'cr' + $environmentName
$crPassword = az acr credential show --name $crUserName --query "passwords[0].value" -o tsv

$crServerName
$crUserName
$crPassword
```

# docker login
```powershell
docker login $crServerName
```

# コンテナ作成 バックエンド
```powershell
$tagName=$crServerName + '/dapr-backend:latest'
docker build -t dapr-backend:latest .
docker tag dapr-backend $tagName
docker push $tagName
```

# コンテナ作成 フロントエンド
```powershell
docker build -t dapr-frontend:latest .
$tagName01=$crServerName + '/dapr-frontend:0.1.0'
docker tag dapr-frontend $tagName01
docker push $tagName01
$tagName02=$crServerName + '/dapr-frontend:0.2.0'
docker tag dapr-frontend $tagName02
docker push $tagName02
```

# Container App作成 バックエンド
```powershell
$crImage = $crServerName + '/dapr-backend:latest'
az deployment group create --resource-group $resource_group --template-file ./deploy/backend-app/main.bicep --parameters environmentName=$environmentName crServerName=$crServerName crUserName=$crUserName crPassword=$crPassword crImage=$crImage
```

# Container App作成 フロントエンド
```powershell
$crImage = $crServerName + '/dapr-frontend:0.1.0'
$revisionSuffix = 'v1'
$oldRevisionSuffix = 'v1'
az deployment group create --resource-group $resource_group --template-file ./deploy/frontend-app/main.bicep --parameters environmentName=$environmentName crServerName=$crServerName crUserName=$crUserName crPassword=$crPassword crImage=$crImage revisionSuffix=$revisionSuffix oldRevisionSuffix=$oldRevisionSuffix
```

# Blue/Green Deployments
# containerImage のタグを 0.2.0、revisionSuffix のバージョンを v2 に更新することで、先ほどデプロイした Revision（ dapr-frontend--v1）へのトラフィックを 100% のまま、新しい Revision をデプロイ
```powershell
$crImage = $crServerName + '/dapr-frontend:0.2.0'
$revisionSuffix = 'v2'
$oldRevisionSuffix = 'v1'
az deployment group create --resource-group $resource_group --template-file ./deploy/frontend-app/main.bicep --parameters environmentName=$environmentName crServerName=$crServerName crUserName=$crUserName crPassword=$crPassword crImage=$crImage revisionSuffix=$revisionSuffix oldRevisionSuffix=$oldRevisionSuffix
```

# traffic が「0」の新しい Revision には、ユーザーに公開される FQDN とは異なる FQDN が割り当てられるので、リリース前のテストが可能

# リリース前のテストが完了したとして、Azure CLI を使って Swap
```powershell
$caName = 'ca-' + $environmentName + '-fed'
az containerapp ingress traffic set -n $caName -g $resource_group --revision-weight $caName--v1=0 latest=100
```

# 旧バージョンの Revision をシャットダウン
```powershell
$caName = 'ca-' + $environmentName + '-fed'
az containerapp revision deactivate -n $caName -g $resource_group--revision $caName--v1
```

