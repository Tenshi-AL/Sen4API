# About
Sen4 is a study project aimed at helping with small group project management.

Technologies & Libraries Used
- ASP.NET Core – backend framework
- Entity Framework Core – ORM for database access
- SignalR – real-time communication
- AutoMapper – object mapping
- FluentValidation – input validation
- Serilog – logging
- MinIO – file storage solution
- Docker – containerization
- Seq

## Features
- User authentication and authorization  
- Create, read, update, and delete (CRUD) projects and tasks  
- Upload and download task-related files (PDF, PNG, and other formats)  
- Assign responsibilities for tasks

## Install
Clone the project:
```bash
https://github.com/Tenshi-AL/sen4-client.git
```
You can start the whole stack using the following docker-compose.yml file:
```yaml
networks:
  Sen4Network:
    driver: bridge

services:      
  Sen4API:
    image: sen4
    ports:
      - 8080:8080
    networks:
      - Sen4Network
    depends_on:
      - Sen4Db
    build:
      context: .
      dockerfile: Sen4/Dockerfile
      
  Sen4Db:
    image: postgres:12.19
    ports:
      - 5433:5432
    networks:
      - Sen4Network
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: root
  
  Sen4Seq:
    image: datalust/seq:latest
    container_name: seq
    restart: unless-stopped
    mem_limit: 5g
    memswap_limit: 5g
    environment:
      - ACCEPT_EULA=Y
    networks:
      - Sen4Network
    ports:
      - 8020:80
      - 5341:5341
        
  minio:
    image: minio/minio:latest
    command: server --console-address ":9001" /data/
    networks:
      - Sen4Network
    ports:
      - "9000:9000"
      - "9001:9001"
    environment:
      MINIO_ROOT_USER: ozontech
      MINIO_ROOT_PASSWORD: minio123
    healthcheck:
      test: [ "CMD", "curl", "-f", "http://localhost:9000/minio/health/live" ]
      interval: 30s
      timeout: 20s
      retries: 3
```
Then run:
docker-compose up --build
## Usage
Once the containers are up and running, you can start using the application. For example, request that return task list:
```url
http://localhost:8080/ProjectTask/3fa85f64-5717-4562-b3fc-2c963f66afa6
```
In order check logs you can use Seq
