# Dapr Back-End Web Api

## Build and Push Docker Images
```shell-session
$ docker build -t thara0402/dapr-backend:0.1.0 ./
$ docker run --rm -it -p 8000:80 --name backend thara0402/dapr-backend:0.1.0
$ docker push thara0402/dapr-backend:0.1.0
```

## Create Azure Container Apps environment
```shell-session
$ az group create -n <ResourceGroup Name> -l japaneast
$ az deployment group create -f ./deploy/env/main.bicep -g <ResourceGroup Name>
```

## Dapr - Service to Service calls

### Create Azure Container Apps for backend
```shell-session
$ az deployment group create -f ./deploy/app-dapr/main.bicep -g <ResourceGroup Name>
```

### Create Azure Container Apps for frontend
[Dapr Front-End Web Application](https://github.com/thara0402/dapr-frontend)

## Local Development
[Install Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/)
### Install
```shell-session
$ dapr init
$ docker ps -a
CONTAINER ID   IMAGE               COMMAND                  CREATED       STATUS                PORTS                                                 NAMES
8f669c78ed71   daprio/dapr:1.4.3   "./placement"            2 weeks ago   Up 8 days             0.0.0.0:6050->50005/tcp, :::6050->50005/tcp           dapr_placement
7c6ffeb5723c   openzipkin/zipkin   "start-zipkin"           2 weeks ago   Up 8 days (healthy)   9410/tcp, 0.0.0.0:9411->9411/tcp, :::9411->9411/tcp   dapr_zipkin
09d482bdba03   redis               "docker-entrypoint.s…"   2 weeks ago   Up 8 days             0.0.0.0:6379->6379/tcp, :::6379->6379/tcp             dapr_redis
```
### Reconfigure
```shell-session
$ dapr unisntall --all
$ dapr init
```
### Docker Compose
```yaml
version: '3.4'

services:
  daprfrontend:
    image: ${DOCKER_REGISTRY-}daprfrontend
    build:
      context: .
      dockerfile: DaprFrontEnd/Dockerfile
    ports:
      - "51000:50001"

  daprfrontend-dapr:
    image: "daprio/daprd:latest"
    command: [ "./daprd", "-app-id", "daprfrontend", "-app-port", "80" ]
    depends_on:
      - daprfrontend
    network_mode: "service:daprfrontend"

  daprbackend:
    image: ${DOCKER_REGISTRY-}daprbackend
    build:
      context: .
      dockerfile: DaprBackEnd/Dockerfile
    ports:
      - "52000:50001"

  daprbackend-dapr:
    image: "daprio/daprd:latest"
    command: [ "./daprd", "-app-id", "daprbackend", "-app-port", "80" ]
    depends_on:
      - daprbackend
    network_mode: "service:daprbackend"
```