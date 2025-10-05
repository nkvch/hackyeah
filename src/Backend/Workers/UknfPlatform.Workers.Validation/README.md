# UKNF Validation Worker

## Overview
This worker processes report validation jobs asynchronously using RabbitMQ and MassTransit.

## Features
- Async validation processing (Q1 files pass, Q2 files fail)
- PDF result generation with QuestPDF
- XLSX parsing with ClosedXML
- 24-hour timeout protection
- Retry policy with exponential backoff
- Comprehensive logging with Serilog

## Prerequisites
- .NET 8 SDK
- RabbitMQ (via Docker or local install)
- PostgreSQL database
- File storage directory

## Quick Start with Docker

### 1. Start RabbitMQ:
```bash
docker-compose up -d rabbitmq
```

**RabbitMQ Management UI:** http://localhost:15672
- Username: `guest`
- Password: `guest`

### 2. Start Worker:
```bash
cd src/Backend/Workers/UknfPlatform.Workers.Validation
dotnet run
```

## Local Development Setup

### 1. Install RabbitMQ (if not using Docker):
**macOS:**
```bash
brew install rabbitmq
brew services start rabbitmq
```

**Linux:**
```bash
sudo apt-get install rabbitmq-server
sudo systemctl start rabbitmq
```

**Windows:**
Download from: https://www.rabbitmq.com/download.html

### 2. Configure appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=uknf_platform;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "FileStorage": {
    "StorageType": "Local",
    "LocalStoragePath": "./storage/files"
  }
}
```

### 3. Run the worker:
```bash
dotnet run
```

## How It Works

### Report Submission Flow:
1. **User submits report** via API (`POST /api/reports/upload`)
2. **API saves report** with status `Working`
3. **ReportSubmittedEvent** published via MediatR
4. **Event handler** updates status to `Transmitted` and publishes to RabbitMQ
5. **Worker consumes job** from `report-validation-queue`
6. **Worker validates** file using MockReportValidator
7. **Worker generates** PDF result with QuestPDF
8. **Worker updates** report status (`Successful` or `ValidationErrors`)
9. **Notification sent** to user (stub for now)

### Validation Rules (Mock):
- **Q1 files (Q1_2025):** ✅ Pass validation
- **Q2 files (Q2_2025):** ❌ Fail with predefined errors
- **Other periods:** ✅ Basic validation

### Status Transitions:
```
Working → Transmitted → Ongoing → Successful/ValidationErrors/TechnicalError
                              ↓
                       (after 24h) TimeoutError
```

## Monitoring

### RabbitMQ Management UI:
- URL: http://localhost:15672
- Queues: Check `report-validation-queue`
- Messages: Monitor in-flight and completed messages

### Logs:
- Console output (Serilog)
- File: `logs/validation-worker-YYYYMMDD.txt`

### Worker Metrics:
- Prefetch count: 5 messages
- Concurrency: 5 workers
- Retry policy: 3 attempts with exponential backoff

## Testing

### Submit a test report:
```bash
curl -X POST http://localhost:8080/api/reports/upload \
  -H "Authorization: Bearer {token}" \
  -F "entityId=1001" \
  -F "reportType=Quarterly" \
  -F "reportingPeriod=Q1_2025" \
  -F "file=@test_report.xlsx"
```

### Expected behavior:
1. Report status: `Working` (immediate)
2. Report status: `Transmitted` (~1 second)
3. Report status: `Ongoing` (~2 seconds)
4. Report status: `Successful` (~7 seconds total)

## Troubleshooting

### Worker not consuming messages:
- Check RabbitMQ is running: `docker ps | grep rabbitmq`
- Check queue exists: http://localhost:15672/#/queues
- Check connection: Look for "Connected to RabbitMQ" in logs

### Validation fails:
- Check file storage path exists
- Check PostgreSQL connection
- Review logs for exceptions

### Timeout issues:
- Check ValidationTimeoutWorker is running
- Default: 24 hours (configurable in appsettings.json)
- Worker checks every hour

## Configuration

### appsettings.json Options:
```json
{
  "Validation": {
    "MockDelaySeconds": 5,      // Simulated processing time
    "TimeoutHours": 24           // Timeout threshold
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "PrefetchCount": 5,          // Messages to prefetch
    "ConcurrencyLimit": 5        // Parallel workers
  }
}
```

## Production Deployment

### Recommendations:
1. **RabbitMQ:** Use managed service (CloudAMQP, AWS MQ)
2. **File Storage:** Replace LocalFileStorageService with Azure Blob/AWS S3
3. **Logging:** Add Application Insights or ELK stack
4. **Monitoring:** Add Prometheus metrics
5. **Scaling:** Run multiple worker instances
6. **Retry Policy:** Configure dead-letter queue for failed messages

## Next Steps (Story 4.3+):
- Real SignalR notifications (replace stub)
- Email notifications with PDF attachment
- Download validation results
- Report correction workflow
- Admin contestation features

## Support
For issues, check:
- Worker logs: `logs/validation-worker-*.txt`
- RabbitMQ management: http://localhost:15672
- Database: Check Reports and ValidationResults tables


