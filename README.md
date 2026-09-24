
# BuildTruck-Backend

Backend de la aplicación BuildTruck, con microservicios

# Overview

BuildTruck Backend provides the server-side services required by the BuildTruck platform. The backend is responsible for exposing REST APIs, managing business logic, handling authentication, connecting with the database, and supporting the communication between the main application modules.

The project follows a service-oriented structure composed of a main backend API and separated microservices for specific domains. This organization improves maintainability and allows each service to focus on a clear business responsibility.

## Backend Services

The backend is organized into the following services:

- **BuildTruck Core API**: contains the main business modules of the platform, such as personnel, materials, machinery, incidents, documentation, notifications, statistics, and configuration.
- **BuildTruck User Service API**: handles authentication, user management, roles, profile information, password management, and user-related operations.
- **BuildTruck Project Service API**: manages project creation, project assignment, project status, manager/supervisor relationships, and project queries.

This structure supports the growth of the platform and allows the team to document, deploy, and validate each service independently.

## API Documentation

The backend services are documented using Swagger/OpenAPI. This allows developers to inspect available endpoints, request parameters, response structures, authentication requirements, and supported HTTP methods.

Deployed Swagger documentation:

- BuildTruck Core API: https://buildtruck-api.duckdns.org/swagger/index.html
- BuildTruck User Service API: https://buildtruck-api.duckdns.org/user-service/swagger/index.html
- BuildTruck Project Service API: https://buildtruck-api.duckdns.org/project-service/swagger/index.html

## Deployment Notes

The backend is deployed in a cloud environment and exposes the services through a public API domain. The deployment uses a reverse proxy configuration to route requests to the corresponding backend service.

Main deployment responsibilities include:

- Exposing the Core API and microservices through public routes.
- Providing Swagger/OpenAPI documentation for each service.
- Supporting authentication using JWT tokens.
- Centralizing service access through a shared backend domain.
- Enabling monitoring and log review through cloud infrastructure tools.

This setup allows the frontend application and development team to interact with the backend services from a deployed environment.

## API Execution and Validation

The backend services can be validated using tools such as Postman. The main execution flow includes authenticating a user, storing the JWT token, and using it to access protected endpoints.

Recommended validation flow:

1. Authenticate with the User Service using the login endpoint.
2. Store the returned JWT token.
3. Validate the authenticated user endpoint.
4. Execute project creation and project query endpoints.
5. Review responses, status codes, and returned JSON structures.

This process helps verify that the deployed Web Services are available and working correctly in the cloud environment.

## Monitoring and Collaboration

The backend deployment is supported by cloud monitoring tools that help the team review service activity, logs, and infrastructure behavior. Logs and runtime events are useful for validating authentication attempts, endpoint execution, database queries, and possible runtime errors.

Team collaboration is managed through GitHub, where changes are tracked using commits. This allows the team to review implementation progress, identify contributions, and maintain a clear history of backend changes during each Sprint.

Useful collaboration evidence includes:

- GitHub commit history.
- GitHub Insights Pulse.
- GitHub Insights Contributors.
- CloudWatch logs and alarms.
- Swagger/OpenAPI documentation.
- Postman execution evidence.



 
