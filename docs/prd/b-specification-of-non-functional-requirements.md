# B. Specification of non-functional requirements

## 1. File management

- Uploading files of different formats
- Support for large files using chunked upload
- Possibility to compress/decompress ZIP files
- Preparing the system to use an external virus scanning service
- Sharing files with other users
- Managing File Permissions
- Hierarchical directory system
- Versioning of files
- File metadata
- Search by metadata and content

## 2. Security

### 2.1 Data security

- Encryption of sensitive data
- Communication Encryption (HTTPS)
- Validation of input data
- Protection against common attacks (CSRF, XSS, SQL Injection)
- Secure password storage

### 2.2 Audit and Login

- Tracking user activity
- Logging security events
- Anomaly detection

## 3. Performance and scalability

### 3.1 Optimization of performance

- Caching of data (cache)

- Lazy loading for lists and large data sets
- Pagination of results
- Optimization of database queries

## 4. Documentation

### 4.1 API documentation

- Swagger/OpenAPI
- Description of all REST endpoints
- Examples of requests and responses

### 4.2 Technical documentation

- README file with installation and configuration instructions
- Diagram of Entity Relationships
- System architecture documentation
- Instructions for implementation (deployment)

### 4.3 User documentation

- User manual for the portal

## 5. Implementation and start-up

### 5.1 Container environment

- Docker Compose to run all components
- Connected services (frontend, backend, database, queue system)

### 5.2 Configuration

- Parameterization by environmental variables
- External configuration files
