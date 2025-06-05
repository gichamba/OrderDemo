# Order Management System

## Project Overview

This project implements a simplified order management system using .NET 8, designed with a Clean Architecture approach. It simulates core functionalities for managing customer orders, including creation, status tracking, discount application, and basic analytics.

## Architecture

The solution is structured into four distinct projects, adhering to Clean Architecture principles to promote separation of concerns, testability, and maintainability:

* **`OrderDemo.Domain`**: The heart of the application, containing core business entities (e`Order`, `Customer`), value objects, enums (`CustomerSegment`, `OrderStatus`), and constants. It has no dependencies on other layers.
* **`OrderDemo.Application`**: Defines the application's business logic, use cases, and orchestrates domain entities. It contains DTOs, interfaces for services (`IOrderService`, `IDiscountService`, `IAnalyticsService`, `IOrderDemoContext`), their implementations, and AutoMapper configurations. This layer depends only on `OrderDemo.Domain`.
* **`OrderDemo.Infrastructure`**: Handles external concerns, such as data persistence (Entity Framework Core in-memory database implementation), and provides concrete implementations for interfaces defined in the `Application` layer (e.g., `OrderDemoContext`). It depends on `OrderDemo.Application` and `OrderDemo.Domain`.
* **`OrderDemo.WebApi`**: The entry point of the application, exposing functionality via Minimal APIs. It handles HTTP requests, maps them to application layer commands/queries, and returns appropriate responses. It references `OrderDemo.Application` and `OrderDemo.Infrastructure` (for DI configuration).
* **`OrderDemo.WebApi.IntegrationTests`**: A dedicated project for testing the API endpoints, ensuring the entire stack (from API to database) works as expected.

## Features Implemented

1.  **Order Creation & Management**:
    * Create new orders with customer association and total amount.
    * Retrieve individual orders or a list of all orders.
2.  **Discounting System**:
    * Applies a 10% discount for "New" customers on their first order.
    * Applies a 5% discount for "Loyal" customers who have made 10 or more previous orders.
    * No discount for "Regular" customers or "Loyal" customers with fewer than 10 orders.
3.  **Order Status Tracking**:
    * Orders have a `Pending`, `Processing`, `Shipped`, `Delivered`, or `Cancelled` status.
    * Supports valid state transitions (e.g., `Pending` -> `Processing` or `Cancelled`, `Shipped` -> `Delivered`).
    * Automatically sets `DeliveredDate` when an order transitions to `Delivered`.
4.  **Order Analytics**:
    * Provides an endpoint to retrieve basic analytics, such as the average order value and average fulfillment time (for delivered orders).

## Technologies Used

* **.NET 8**: The core framework for building the application.
* **Entity Framework Core (In-Memory)**: For data persistence during development and testing.
* **Minimal APIs**: To create lightweight and focused HTTP APIs.
* **AutoMapper**: For seamless object-to-object mapping between DTOs and domain entities.
* **xUnit**: The testing framework.
* **FluentAssertions**: For fluent and readable test assertions.
* **OneOf**: For expressing discriminated unions in service layer results, providing clear success, validation, not found, or error states.

## Running the Project

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/gichamba/OrderDemo.git
    cd OrderDemo
    ```
2.  **Build the solution:**
    ```bash
    dotnet build
    ```
3.  **Run the API:**
    Navigate to the `OrderDemo.WebApi` directory and run:
    ```bash
    cd OrderDemo.WebApi
    dotnet run
    ```
    The API will typically run on `http://localhost:5277` or `http://localhost:7277` (HTTPS).
4.  **Run Tests:**
    Navigate to the `OrderDemo.WebApi.IntegrationTests` directory and run:
    ```bash
    cd OrderDemo.WebApi.IntegrationTests
    dotnet test
    ```

## Performance Optimization

**Identified Bottleneck:**
When processing new orders, retrieving customer details (which are needed for discount calculation) could lead to an "N+1 query problem" in a real-world relational database scenario. If the customer's associated `Orders` collection wasn't loaded alongside the customer, accessing `customer.Orders.Count` within the `DiscountService` would trigger an additional, separate database query for each customer's orders. This results in multiple database roundtrips, impacting performance, especially for customers with extensive order histories.

**Implemented Optimization:**
To mitigate this, **eager loading** was implemented in the `OrderService`. When a customer is fetched by `CreateOrderAsync` or `UpdateOrderStatusAsync`, the `.Include(c => c.Orders)` method is used to ensure that the customer's related `Orders` collection is loaded from the database in a **single query**. This eliminates the need for subsequent individual queries for each customer's orders when the `DiscountService` accesses `customer.Orders.Count`.

**Expected Impact:**
This optimization significantly reduces the number of database queries required per order creation or status update from `1 + N` (where N is the number of a customer's existing orders) down to just `1` query. This leads to:
* Faster API response times.
* Reduced load on the database server.
* Minimized network I/O between the application and the database.

## Testing Approach

* **Unit Tests**: Focused on isolated business logic components (e.g., the `DiscountService`'s calculation logic).
* **Integration Tests**: Comprehensive tests using `WebApplicationFactory` to test API endpoints end-to-end, ensuring correct interaction between the API, application services, and the in-memory database. A unique in-memory database is set up for each test class to ensure isolation.

## API Documentation

The API endpoints are self-documented using **Swagger/OpenAPI**. When the application is running in development mode, the Swagger UI is accessible at the `/swagger` endpoint (e.g., `http://localhost:5000/swagger`). This provides an interactive interface for exploring and testing the API.

## Assumptions Made

* For simplicity, user authentication and authorization are out of scope for this assignment.
* Data validation is primarily handled via Data Annotation attributes on DTOs and basic service-level checks. More complex validation could use FluentValidation.
* The in-memory database is used purely for demonstration and testing purposes. A real-world application would use a persistent database.
