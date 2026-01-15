# BankSystem DemoShop

Demo e-commerce application for testing bank payment integrations.

## Architecture
- ASP.NET Core 3.1 MVC
- Entity Framework Core with SQL Server
- AWS RDS with IAM authentication
- Integration with BankSystem payment API

## Features
- Product catalog management
- Shopping cart functionality
- Order processing
- Bank payment integration
- Payment status tracking

## Getting Started

### Prerequisites
- .NET Core 3.1 SDK
- Docker
- AWS CLI configured

### Local Development

```bash
# Restore dependencies
dotnet restore src/DemoShop.sln

# Run locally
cd src/DemoShop.Web
dotnet run

# Access the application
open http://localhost:5000
```

### Docker Build

```bash
# Build from repository root
docker build -t demoshop:latest .

# Run locally
docker run -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e ASPNETCORE_ENVIRONMENT=Development \
  demoshop:latest
```

### Deploy to EKS

```bash
# Apply Kubernetes manifests
kubectl apply -f kubernetes/

# Check deployment status
kubectl get pods -n banksystem -l app=demoshop
kubectl logs -f deployment/demoshop -n banksystem

# Check service endpoint
kubectl get svc demoshop-service -n banksystem
```

## Configuration

### Environment Variables
- `RdsAuthentication__UseIamAuthentication` - Enable IAM auth (true/false)
- `RdsAuthentication__RdsEndpoint` - RDS endpoint
- `RdsAuthentication__DbUser` - Database username (demoshop_app)
- `RdsAuthentication__AwsRegion` - AWS region
- `RdsAuthentication__FallbackPassword` - Password for fallback

### Database Setup
```bash
# Run migrations
cd src/DemoShop.Web
dotnet ef database update
```

## Project Structure

```
banksystem-demoshop/
├── Dockerfile              # Multi-stage Docker build
├── README.md              # This file
├── kubernetes/            # Kubernetes manifests
│   ├── deployment.yaml    # Deployment, Service, ServiceAccount
│   └── ingress.yaml       # ALB Ingress configuration
├── terraform/             # Infrastructure as Code
├── docs/                  # Documentation
└── src/                   # Source code
    ├── DemoShop.sln
    ├── DemoShop.Models/
    ├── DemoShop.Data/
    ├── DemoShop.Services/
    ├── DemoShop.Services.Models/
    └── DemoShop.Web/
```

## API Endpoints

- `GET /` - Home page with product catalog
- `GET /products` - List all products
- `POST /cart/add` - Add item to cart
- `POST /order/checkout` - Process order and payment
- `GET /health` - Health check endpoint

## Testing

```bash
# Run unit tests
dotnet test src/DemoShop.Tests/

# Integration tests
dotnet test src/DemoShop.IntegrationTests/
```

## CI/CD

GitHub Actions workflow automatically:
1. Builds Docker image
2. Pushes to Amazon ECR
3. Deploys to EKS cluster
4. Runs smoke tests

## Monitoring

- Health checks: `/health` endpoint
- Kubernetes probes: Liveness and Readiness
- CloudWatch logs integration

## License

MIT
