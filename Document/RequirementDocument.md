# Email-Service Requirement Document

## Table of Contents
1. Introduction
2. Overall Description
3. System Features and Requirements
4. Other Requirements

## 1. Introduction
In our projects, almost all of them utilize features related to sending emails to users. We are encountering some issues during this process, such as:
- During the development process, we use some real customer data. We have encountered some instances during testing where we sent a significant number of emails to the actual customer system.
- We use an email server, but currently lack a mechanism to manage the sending and receiving of emails. This has led to instances where, during server issues, the emails we send during that period are not processed and go unmanaged
- etc.

The problem at hand is that we need to implement a solution that enables us to manage the logic related to email sending within our projects. Our Solution is that we create an Email Service system with microservice architecture and deploy on K8s, Our service will meet the requirements described in more detail in the next sections

## 2. Overall Description:
### Project Overall:
- Our project will be built from scratch, After implementation, existing projects can integrate our project to use the solution it brings.
- The main function of the project is to provide a solution that supports other projects in sending and managing emails.
- Feature requests will be collected from projects that have problems sending and managing emails, from which we will provide a solution
- Requirements for implementation, design, resources used, will be flexibly designed to suit the projects used our solution
### Architecture Overall:
We need to implement an asynchronous email service using .Net that can be deployed on Kubernetes. Other system can deploy our service to their cluster or can integrate with our cloud service. They will send request to our Service to using our functions
#### Advantage:
- Scalability: Microservices can be independently scaled, allowing for efficient resource utilization
- Flexibility: Microservices can be developed and deployed independently, enabling a more flexible development and release cycle. We can flexibly design and develop logic as well as settings related to systems such as email server, cache server, database server, message queue server, log server
- Isolation: Services are isolated, reducing the impact of failures and making it easier to maintain and update specific components. We can provide some general communication methods so that services in the same network or other systems can communicate with our service, It will be easy to integrate our solution into the projects that we have done
#### Dis-Advantage:
- Complexity in Deployment: We may encounter some challenges in Deployment, managing multiple services, communication between themin new projects or projects that have already been implemented before, We may need more resources in the Infrastructure of the current system
## 3. System Features and Requirements
### A. System Feature:
We need to support these features:
- Send Email entry: Other system can send request to our service, our service can handle logic to send an email to receivers
- Manage the email:
    * Have a retention policy
    * Tracking email status
    * Email sending strategies (blacklist/whitelist)
    * Retry logic for failed emails
- Admin portal: We may provide an admin portal in Email Service (which is used internally for tracking and controlling the Email logic process) protected by an alternate port and conditional access. In the customer system, we can provide some other way for them to monitor the process like log
### External Interface Requirement:
We need to provide an SDK, other Service can use the SDK to send request to our Email Service
### Funtional Requirement:
- Our solution needs an authentication mechanism, for example: identity authentication through OpenID Connect
- Our solution need have the compatibility with multiple cloud platforms (e.g., Azure, AWS), support for various storage systems (SQL Storage, Redis, Memory) and messaging services (like RocketMQ, RabbitMQ), multiple logging method (to ECK or file system). We may only be able to support a few platforms in the initial version
- Our functions need to be well monitored by logging to ensure debugging and operational control
### Non-Funtional Requirements:
- Performance:
- Security:
- Software Quality Attributes: We need a process for develop, test and deploy the solution, and able to updating and maintaining the system. We may collect the feedback from users then continuously improve the system 
## 4. Other Requirements:
