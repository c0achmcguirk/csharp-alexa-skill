# CSharp Alexa Skill

This project was created by Paul Oliver to demonstrate how you can write an Alexa Skill using CSharp running on AWS Lambda.

The Skill is called **Country Info**. It returns the capital and population of a country asked by the user.

**Alexa, ask Country Info about Canada**

*About Canada. The capital is Ottawa and the population is 36,155,487.*

This project consists of:
* Function.cs - class file containing a class with a single function handler method
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* project.json - .NET Core project file with build and tool declarations for the Amazon.Lambda.Tools Nuget package

## Here are some steps to follow from Visual Studio:

To deploy your function to AWS Lambda, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed function open its Function View window by double-clicking the function name shown beneath the AWS Lambda node in the AWS Explorer tree.

To perform testing against your deployed function use the Test Invoke tab in the opened Function View window.

To update the runtime configuration of your deployed function use the Configuration tab in the opened Function View window.

To view execution logs of invocations of your function use the Logs tab in the opened Function View window.

## Here are some steps to follow to get started from the command line:

Once you have edited your function you can use the following command lines to build, test and deploy your function to AWS Lambda from the command line (these examples assume the project name is *EmptyFunction*):

Restore dependencies
```
    cd "LambdaAlexa"
    dotnet restore
```

Deploy function to AWS Lambda
```
    cd "LambdaAlexa"
    dotnet lambda deploy-function
```
