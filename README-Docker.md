# Docker Setup for MassTransit Saga Demo

This guide explains how to run the MassTransit Saga microservices solution using Docker.

## 📋 Prerequisites

- Docker Desktop installed and running
- Docker Compose (included with Docker Desktop)
- At least 4GB RAM allocated to Docker

## 🏗️ Architecture

The solution consists of 5 containers:

1. **RabbitMQ** - Message broker (ports 5672, 15672)
2. **Saga Orchestrator** - State machine coordinator (port 5000)
3. **Order Service** - Order tracking API (port 5001)
4. **Payment Service** - Payment processing API (port 5002)
5. **Inventory Service** - Inventory management API (port 5003)

## 🚀 Quick Start

### Build and Run All Services

```bash
# Build all Docker images
docker-compose build

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### Build Individual Services

```bash
# Build specific service
docker build -f Dockerfile.SagaOrchestrator -t saga-orchestrator .
docker build -f Dockerfile.OrderService -t order-service .
docker build -f Dockerfile.PaymentService -t payment-service .
docker build -f Dockerfile.InventoryService -t inventory-service .
```

### Run Individual Services

```bash
# Run specific service
docker run -d -p 5000:5000 --name saga-orchestrator saga-orchestrator
docker run -d -p 5001:5001 --name order-service order-service
docker run -d -p 5002:5002 --name payment-service payment-service
docker run -d -p 5003:5003 --name inventory-service inventory-service
```

## 🔍 Access the Services

### Swagger UI Endpoints
- **Saga Orchestrator**: http://localhost:5000/swagger
- **Order Service**: http://localhost:5001/swagger
- **Payment Service**: http://localhost:5002/swagger
- **Inventory Service**: http://localhost:5003/swagger

### RabbitMQ Management UI
- **URL**: http://localhost:15672
- **Username**: guest
- **Password**: guest

## 📊 Monitoring

### View Container Status
```bash
docker ps
```

### View Container Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f saga-orchestrator
docker-compose logs -f order-service
docker-compose logs -f payment-service
docker-compose logs -f inventory-service
docker-compose logs -f rabbitmq
```

### Inspect Container
```bash
docker inspect saga-orchestrator
```

### Execute Commands Inside Container
```bash
docker exec -it saga-orchestrator /bin/bash
```

## 🧪 Testing the Solution

### Submit an Order (Successful Flow)

```bash
curl -X POST "http://localhost:5000/api/saga/submit-order" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 100.50,
    "productId": "PROD-123",
    "quantity": 2
  }'
```

### Check Order Status

```bash
curl "http://localhost:5001/api/orders/{orderId}"
```

### View All Orders

```bash
curl "http://localhost:5001/api/orders"
```

### Payment Statistics

```bash
curl "http://localhost:5002/api/payments/statistics"
```

### Inventory Status

```bash
curl "http://localhost:5003/api/inventory"
```

## 🔧 Troubleshooting

### Service Not Starting

```bash
# Check logs
docker-compose logs <service-name>

# Restart service
docker-compose restart <service-name>
```

### Network Issues

```bash
# Check network
docker network inspect masstransitsagademo_saga-network

# Recreate network
docker-compose down
docker-compose up -d
```

### Clean Up Everything

```bash
# Stop and remove all containers, networks, and volumes
docker-compose down -v

# Remove all images
docker rmi $(docker images -q masstransitsagademo*)

# Clean up Docker system
docker system prune -a --volumes
```

### RabbitMQ Connection Issues

```bash
# Check RabbitMQ health
docker exec rabbitmq rabbitmq-diagnostics ping

# View RabbitMQ logs
docker logs rabbitmq
```

## 📝 Configuration

### Environment Variables

Each service supports the following environment variables:

```yaml
ASPNETCORE_URLS: http://+:5000          # Service URL
ASPNETCORE_ENVIRONMENT: Development      # Environment
RabbitMQ__Host: rabbitmq                # RabbitMQ host
RabbitMQ__Username: guest               # RabbitMQ username
RabbitMQ__Password: guest               # RabbitMQ password
```

### Modify docker-compose.yml

To change ports or configuration, edit the `docker-compose.yml` file.

## 🔄 Development Workflow

### Rebuild After Code Changes

```bash
# Rebuild specific service
docker-compose build saga-orchestrator

# Restart service
docker-compose up -d saga-orchestrator
```

### Rebuild All Services

```bash
docker-compose build --no-cache
docker-compose up -d
```

## 📈 Scaling Services

```bash
# Scale specific service
docker-compose up -d --scale payment-service=3

# View scaled instances
docker-compose ps
```

## 🛑 Stopping Services

```bash
# Stop all services
docker-compose stop

# Stop specific service
docker-compose stop order-service

# Stop and remove containers
docker-compose down

# Stop and remove containers, networks, and volumes
docker-compose down -v
```

## 📦 Production Considerations

For production deployment, consider:

1. **Use RabbitMQ Cluster** - High availability
2. **Add Database** - Persistent saga state storage
3. **Implement Health Checks** - Better monitoring
4. **Use Secrets Management** - Secure credentials
5. **Add Logging** - Centralized logging (ELK, Seq)
6. **Configure Resource Limits** - CPU/Memory constraints
7. **Enable HTTPS** - Secure communication
8. **Add API Gateway** - Single entry point
9. **Implement Authentication** - Secure APIs
10. **Add Distributed Tracing** - OpenTelemetry

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [MassTransit Documentation](https://masstransit.io/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
