# PDFService
A service to work with PDF files

# Platform
.NET Core Web API (.NET 8+)

# How to run
1. Clone the repository
2. Go to the root directory and run:
    ```
    cd API
    dotnet run
    ```
3. Open the browser and navigate to `http://localhost:8700/swagger/index.html`


# How to build and run with docker
1. Clone the repository
2. Go to the root directory and run:
    ```
    docker build -t pdf-service -f Dockerfile.api .
    docker run -p 5000:5000 pdf-service
    ```
    or
    ```
    docker-compose up --build
    ```
3. Open the browser and navigate to `http://localhost:5000/swagger/index.html`
