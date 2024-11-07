# Project Name: Restaurant Reservation Validation System with RabbitMQ

## Overview

This project demonstrates an asynchronous messaging system for validating **restaurant table reservations** using **C#**, **.NET**, **RabbitMQ**, **MSSQL**, and **PostgreSQL**. The main goal is to demonstrate how to implement **asynchronous tasks**, leverage **message queues**, and utilize **multi-threading**.

The system handles customer reservation requests asynchronously, ensuring non-blocking and efficient processing by distributing tasks across different worker threads using **RabbitMQ**. The project also showcases the use of **multi-threading** for handling complex tasks, database interactions, and efficient data storage in both **MSSQL** and **PostgreSQL**.

## Features

- **RabbitMQ Integration**: Message queue setup for efficient processing of customer reservations.
- **Asynchronous Processing**: Use of `async/await` to enable non-blocking task execution.
- **Multi-threading**: Demonstrates usage of threads for parallel processing to improve system throughput.
- **Database Management**: Integration with **MSSQL** and **PostgreSQL** for data storage and retrieval.
- **Validation Service**: Validates incoming reservation messages for required data, correct formats, and valid dates.
- **Centralized Logging**: Application-level logging to help in debugging and monitoring.
- - **Factory Design Pattern**: The **DatabaseManagerFactory** uses the Factory Design Pattern to create instances of database managers based on the database type.   


## Tech Stack

- **Backend**: C#, .NET 8
- **Messaging Queue**: RabbitMQ
- **Databases**: MSSQL, PostgreSQL
- **Database Access**: ADO.NET Library for database connectivity and data access
- **Tools and Libraries**: Newtonsoft.Json for serialization, RabbitMQ.Client, ADO for lightweight database access.

## Architecture Overview

The architecture consists of the following components:

### Message Organization
- **Exchange Type**: The system uses `ExchangeType.Direct` with durable exchanges set to `true`, ensuring that messages are persistent and not lost in case of broker restarts.
- **Routing Keys**: Routing keys are used to direct different types of messages to specific queues. This allows multiple message types to be handled within the same queue.

1. **Validation Service**: A console application that consumes messages from RabbitMQ, validates message data, and publishes results to the appropriate queues. 
2. **Success Service**: A console application that consumes validated messages from the `Success_RabbitMQ` queue and saves them in MSSQL using a stored procedure. When the stored procedure succeeds, the service provides feedback to the Validation Service.
3. **Fail Service**: A console application that consumes failed validation messages from the `Fail_RabbitMQ` queue and saves them in PostgreSQL.
4. **Message Broker (RabbitMQ)**: Handles communication between the services by distributing reservation tasks to worker services.
5. **Database Layer**: Stores reservation data in MSSQL and PostgreSQL. All data is saved with calling the stored procedures using ADO. 

## Getting Started

To see the project working, you need to start all three console applications: **ValidationService**, **SuccessService**, and **FailService**.
There is also an **RQMessageSender** console application that you can use to send custom messages to the main validation queue.

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [RabbitMQ](https://www.rabbitmq.com/download.html)
- [Docker](https://www.docker.com/products/docker-desktop)

### Installation

1. **Clone the repository**  
   git clone https://github.com/peterGospodinov/RestaurantReservation
   cd restaurant-reservation-system
  

2. **Set up Services using Docker Compose**
   - Use Docker Compose to spin up RabbitMQ, MSSQL, and PostgreSQL:
    
     docker-compose up -d
    
   - The `docker-compose.yml` file includes all the necessary services and also runs an `initmssql.sql` and `initpostgresql.sql` script to automatically create the required tables and stored procedures.

3. **Service Configuration**
   - All configuration settings are default and hardcoded within the application. No additional configuration files are required.


## Usage

## System Design

- **RabbitMQ**: Acts as the message broker that decouples processing logic from reservation validation, ensuring that each service remains responsive while worker services process the data asynchronously.
- **Validation Service**: Validates messages to ensure completeness and correctness (e.g., valid phone number format, no missing data) and publishes the result to either the `Success_RabbitMQ` or `Fail_RabbitMQ` queue.
- **Success and Fail Services**: Consume messages from their respective queues, store the data in the appropriate database, and provide feedback to the Validation Service. Validation results are also provided as feedback to indicate if the data was successfully processed or failed.

## Tests

- **Unit Tests**: Available in the `/tests` directory. Run tests with:

  dotnet test
 

## Deployment

1. **Docker Compose**: Use `docker-compose.yml` to spin up all services, including RabbitMQ, MSSQL, and PostgreSQL.
  
   docker-compose up -d


## Contact

If you have any questions or feedback, feel free to reach out:

- **Email**: peter.gospodinov@gmail.com
- **GitHub**: https://github.com/peterGospodinov

---

Thank you for checking out this project! If you find it useful, please consider giving it a ⭐ on GitHub.