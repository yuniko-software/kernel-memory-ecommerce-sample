## Kernel Memory: E-commerce Sample


![Build](https://github.com/demid-ns/kernel-memory-ecommerce-sample/actions/workflows/ci-build.yml/badge.svg)

Support the project by giving it a star! Your feedback and contributions are greatly appreciated.

## Introduction

This repository contains a sample .NET project demonstrating the use of [Kernel Memory](https://github.com/microsoft/kernel-memory) for semantic search and Retrieval-Augmented Generation (RAG) on a small commercial products dataset. 
It mimics an **e-shop** environment where users can search for products, and the application retrieves the most relevant results.

The project features a [serverless setup](https://microsoft.github.io/kernel-memory/serverless) of Kernel Memory, with services embedded directly in the .NET application. 
You can run this sample using either **Postgres** (with [pgVector](https://github.com/pgvector/pgvector)) or [Qdrant](https://github.com/qdrant/qdrant) as the vector database.

This sample uses **OpenAI's `gpt-4o-mini`** as the language model and **`text-embedding-ada-002`** as the embedding model. Other models are also supported; check the [Kernel Memory repository](https://github.com/microsoft/kernel-memory) for all supported models.

## Setup

1. **Configure API Key**:
   
   Open the `appsettings.json` file in the project root and insert your API token under `KernelMemory:Services:OpenAI:APIKey`. This key is required to authenticate with the OpenAI services used in the sample.

   ```json
   {
     "KernelMemory": {
       "Services": {
         "OpenAI": {
           "APIKey": "your-api-key-here"
         }
       }
     }
   }
   ```
   
2. **Run the Application**:
   
   To start the services, run `docker-compose up -d` **from the repository root**.

   Alternatively, you can run the application through the `docker-compose` startup project directly from your IDE ([Visual Studio](https://learn.microsoft.com/en-us/visualstudio/containers/tutorial-multicontainer?view=vs-2022)/Rider/VS Code)

3. **Ingest Sample Dataset**:
   
   After the application is running, open your browser and navigate to [http://localhost:9000](http://localhost:9000).
   From there, you can ingest the sample dataset located at `/utils/dataset/products.csv` ([link](./utils/dataset/products.csv))

## Accessing the Application

- [http://localhost:9000](http://localhost:9000) - Application UI
- [http://localhost:9000/swagger](http://localhost:9000/swagger) - Swagger API Documentation
- [http://localhost:5341](http://localhost:5341) - Seq (observability, structured logs, traces). Default login: **`admin`**; Default password: **`password`**
- [http://localhost:6333/dashboard](http://localhost:6333/dashboard) - Qdrant Dashboard

## Contribution

Feel free to open discussions, submit pull requests, or share suggestions to help improve the project! The authors are very friendly and open to feedback and contributions.


   
