# Description
This lambda is the same as in .NET6, but uses the .NET 7 and already comes with the API Gateway types, so no extra packages need to be added nor code has to be changed manually.

Only 1 funtion per project is allowed.

The published binaries are pushed to AWS via Docker, so the images can take some time to be sent (mine was 300mb).

# How to create a .NET 7 lambda that can be called from an API Gateway
1. ``` dotnet new serverless.image.EmptyServerless --name --name MyFunctionName```
2. No .sln file will be created. If you wish to have one:
   1. ```cd .\MyFunctionName```
   2. ```dotnet new sln --name MyFunctionName```
   3. ```dotnet sln add .\src\MyFunctionName .\test\MyFunctionName.Tests```

# Deploy
1. Use an existing S3 bucket, or create a new one with: ```aws s3 mb s3://myfunctionname```
2. cd ```src\MyFunctionName```
3. ```dotnet lambda deploy-serverless```
4. Choose a stack name (```MyFunctionName```)
5. Enter the S3 bucket.
6. Wait.

An API Gateway is created and deployed automatically with the same name used for the function, wthi 2 stages: ```Stage``` and ```Prod```.

# Test deployed .NET6 lambda
1. Get the id of the automaticaly created API Gateway with: ```aws apigateway get-rest-apis --query 'items[?name==`MyFunctionName`].[id]' --output text```
2. Build the URL yourself: ```https://<api-gateway-id>.execute-api.<region>.amazonaws.com/<stageName>```
3. GET on that base URL should return the default welcome message, for example: ```Welcome to running ASP.NET Core Minimal API on AWS Lambda```
4. To call each controller method, use the controller name as resource, for example: ```GET https://xxxxxxx.execute-api.us-west-1.amazonaws.com/Stage/calculator/add/1/2```

See below the *original* ```Readme.md``` file created by the template:

---

# Empty AWS Serverless Application Project

This starter project consists of:
* serverless.template - An AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* Function.cs - Class file containing the C# method mapped to the single function declared in the template file
* aws-lambda-tools-defaults.json - Default argument settings for use within Visual Studio and command line deployment tools for AWS

You may also have a test project depending on the options selected.

The generated project contains a Serverless template declaration for a single AWS Lambda function that will be exposed through Amazon API Gateway as a HTTP *Get* operation. Edit the template to customize the function or add more functions and other resources needed by your application, and edit the function code in Function.cs. You can then deploy your Serverless application.

## Packaging as a Docker image.

This project is configured to package the Lambda function as a Docker image. The default configuration for the project and the Dockerfile is to build 
the .NET project on the host machine and then execute the `docker build` command which copies the .NET build artifacts from the host machine into 
the Docker image. 

The `--docker-host-build-output-dir` switch, which is set in the `aws-lambda-tools-defaults.json`, triggers the 
AWS .NET Lambda tooling to build the .NET project into the directory indicated by `--docker-host-build-output-dir`. The Dockerfile 
has a **COPY** command which copies the value from the directory pointed to by `--docker-host-build-output-dir` to the `/var/task` directory inside of the 
image.

Alternatively the Docker file could be written to use [multi-stage](https://docs.docker.com/develop/develop-images/multistage-build/) builds and 
have the .NET project built inside the container. Below is an example of building the .NET project inside the image.

```dockerfile
FROM public.ecr.aws/lambda/dotnet:7 AS base

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim as build
WORKDIR /src
COPY ["LambdaWithApiGateway.csproj", "LambdaWithApiGateway/"]
RUN dotnet restore "LambdaWithApiGateway/LambdaWithApiGateway.csproj"

WORKDIR "/src/LambdaWithApiGateway"
COPY . .
RUN dotnet build "LambdaWithApiGateway.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "LambdaWithApiGateway.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
```

When building the .NET project inside the image you must be sure to copy all of the class libraries the .NET Lambda project is depending on 
as well before the `dotnet build` step. The final published artifacts of the .NET project must be copied to the `/var/task` directory. 
The `--docker-host-build-output-dir` switch can also be removed from the `aws-lambda-tools-defaults.json` to avoid the 
.NET project from being built on the host machine before calling `docker build`.

## Here are some steps to follow from Visual Studio:

To deploy your Serverless application, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed application open the Stack View window by double-clicking the stack name shown beneath the AWS CloudFormation node in the AWS Explorer tree. The Stack View also displays the root URL to your published application.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "LambdaWithApiGateway/test/LambdaWithApiGateway.Tests"
    dotnet test
```

Deploy application
```
    cd "LambdaWithApiGateway/src/LambdaWithApiGateway"
    dotnet lambda deploy-serverless
```
